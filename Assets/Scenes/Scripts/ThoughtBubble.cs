using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Hierarchy (same for Pig and Dog):
//   Pig_ThoughtBubble  [Canvas – World Space, this script]
//     └── BubbleRoot
//           ├── Background   [Image – ThoughtBubble sprite, Simple]
//           ├── Dish1Image   [Image]
//           ├── PlusLabel    [TextMeshPro]
//           └── Dish2Image   [Image]
//
// Inspector fields: background, dish1Image, plusLabel, dish2Image
//
// POSITIONING
// ───────────
// Set headAnchor to an empty child Transform placed at the top of each
// character's head (Pig gets its own, Dog gets its own). The bubble then
// simply follows that Transform every frame — since headAnchor is a CHILD
// of the character, it automatically tracks X/Y/Z no matter which counter
// slot the character is standing in.
//
// This replaces the old "search for a SkinnedMeshRenderer and read its mesh
// bounds" approach, which is fragile in exactly the way you're hitting:
// GetComponentInChildren<SkinnedMeshRenderer>() returns the FIRST renderer
// it finds in the hierarchy — not necessarily the main body. If a character
// has more than one SkinnedMeshRenderer (ears, snout, accessories, clothing)
// it can silently grab the wrong one, with wildly different mesh bounds.
// Since Pig and Dog are different models, it's entirely possible the search
// order "happens" to land on the right mesh for Dog and the wrong one for
// Pig — which matches what you're seeing (Dog fine, Pig way off), even
// though X/Z are computed identically for both.
//
// If headAnchor is left empty, the old auto-detect logic still runs as a
// fallback so nothing breaks immediately — but assigning a headAnchor per
// character is strongly recommended and will fix this permanently.
public class ThoughtBubble : MonoBehaviour
{
    [Header("References")]
    public Image           background;
    public Image           dish1Image;
    public TextMeshProUGUI plusLabel;
    public Image           dish2Image;

    [Header("Head Anchor (recommended — fixes per-character position bugs)")]
    [Tooltip("Empty child Transform positioned at the top of THIS character's head. " +
             "Create one per character (Pig_HeadAnchor, Dog_HeadAnchor, ...) and drag " +
             "it here. If left empty, falls back to auto-detecting a SkinnedMeshRenderer " +
             "on the parent, which is unreliable when a character has more than one " +
             "SkinnedMeshRenderer.")]
    public Transform headAnchor;

    [Header("Bubble World Width (metres)")]
    [Tooltip("Width of the bubble when showing a SINGLE dish image.")]
    public float singleBubbleWidth = 0.55f;
    [Tooltip("Width of the bubble when showing TWO dish images with +.")]
    public float plusBubbleWidth   = 0.9f;

    [Header("Content Sizing (canvas units)")]
    [Tooltip("Size of each dish image square inside the bubble.")]
    public float dishSize = 40f;
    [Tooltip("Gap between dish1/plus and plus/dish2. Lower this to pull the '+' " +
             "closer to the dishes on either side of it.")]
    public float spacing  = 10f;
    [Tooltip("Width of the '+' label's own box. Lower this if there's still visible " +
             "empty space around the plus sign itself even after reducing Spacing to ~0 — " +
             "the box reserves this much room regardless of how big the glyph looks.")]
    public float plusWidth = 24f;
    [Tooltip("Padding around content inside the background.")]
    public float padding  = 20f;

    [Header("Head Offset")]
    [Tooltip("World metres above the head anchor / top of the character mesh.")]
    public float extraAboveHead = 0.18f;

    [Header("Animation")]
    public float popDuration = 0.22f;

    /// <summary>
    /// Current world-space height (in metres) of the bubble body, updated every
    /// time Show() runs. PatienceStars reads this to stack itself exactly above
    /// the bubble instead of guessing its own separate head offset — so the two
    /// pieces of UI can never drift apart the way they were doing before.
    /// </summary>
    public float CurrentWorldHeight { get; private set; }

    // ── internals ───────────────────────────────────────────────────────────
    private Camera               mainCam;
    private Coroutine            popCoroutine;
    private Vector3              shownScale;
    private bool                 isVisible;
    private SkinnedMeshRenderer  smr;   // only used by the legacy fallback path
    private float                logTimer; // debug position logging (see LateUpdate)
    private Coroutine            urgentCoroutine;
    private bool                 isUrgent;

    void Awake()
    {
        mainCam = Camera.main;
        transform.localScale = Vector3.zero;
    }

    void LateUpdate()
    {
        if (transform.parent == null) return;

        Vector3 p;
        string  source;

        if (headAnchor != null)
        {
            // Preferred path: headAnchor is a child of the character, so its
            // world position already tracks X/Z/Y correctly no matter which
            // counter slot the character is standing in.
            p = headAnchor.position;
            p.y += extraAboveHead;
            source = $"headAnchor='{headAnchor.name}' (headAnchor world pos={headAnchor.position:F2})";
        }
        else
        {
            // Legacy fallback: guess the head height from mesh bounds.
            if (smr == null)
                smr = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();

            if (smr == null)
            {
                Debug.LogWarning($"🗨️ ThoughtBubble on parent='{transform.parent?.name}': no headAnchor set AND no SkinnedMeshRenderer found under parent — bubble is not being positioned at all this frame.");
                return; // nothing to position against yet, skip this frame
            }

            p = transform.parent.position;      // correct X and Z
            p.y = smr.bounds.max.y + extraAboveHead;
            source = $"SMR-fallback smr='{smr.name}' (bounds.max.y={smr.bounds.max.y:F2})";
        }

        transform.position = p;

        // Billboard: face camera when visible.
        if (mainCam != null && isVisible)
            transform.rotation = Quaternion.LookRotation(
                transform.position - mainCam.transform.position);

        // Debug: print resolved position once a second so you can compare
        // against where the character actually is and catch a mis-wired
        // reference (wrong headAnchor, wrong SMR, wrong CustomerController
        // field) without guessing. Remove this block once things line up.
        logTimer += Time.deltaTime;
        if (logTimer > 1f)
        {
            logTimer = 0f;
            Debug.Log($"🗨️ ThoughtBubble GO='{gameObject.name}' parent='{transform.parent?.name}' visible={isVisible} using {source} -> world pos={transform.position:F2}");
        }
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public void Show(KottuRecipe recipe)
    {
        SetUrgent(false); // fresh order, always start calm even if reused from the pool mid-blink

        // Force background fully opaque white.
        if (background != null)
        {
            background.color = Color.white;
            ResetImageRect(background);
        }

        bool hasAddOn = recipe?.addOnSprite != null;

        // Dish 1 — always shown.
        if (dish1Image != null)
        {
            ResetImageRect(dish1Image);
            dish1Image.sprite  = recipe?.orderSprite;
            dish1Image.enabled = recipe?.orderSprite != null;
            dish1Image.rectTransform.sizeDelta = Vector2.one * dishSize;
        }

        // Plus + Dish 2 — only for + recipes.
        if (plusLabel  != null) plusLabel.gameObject.SetActive(hasAddOn);
        if (dish2Image != null)
        {
            ResetImageRect(dish2Image);
            dish2Image.gameObject.SetActive(hasAddOn);
            if (hasAddOn)
            {
                dish2Image.sprite = recipe.addOnSprite;
                dish2Image.rectTransform.sizeDelta = Vector2.one * dishSize;
            }
        }

        LayoutAndScale(hasAddOn);

        isVisible = true;
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(PopIn());
    }

    public void Hide()
    {
        isVisible = false;
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        transform.localScale = Vector3.zero;
        SetUrgent(false); // stop any blinking and restore full opacity for next time
    }

    /// <summary>
    /// Called by CustomerController once remaining patience drops below its
    /// lowPatienceThreshold — makes the bubble blink (fade in/out) to warn
    /// the player this customer is about to leave. Call SetUrgent(false) to
    /// stop (Hide() already does this automatically).
    /// </summary>
    public void SetUrgent(bool urgent)
    {
        if (isUrgent == urgent) return;
        isUrgent = urgent;

        if (urgentCoroutine != null) StopCoroutine(urgentCoroutine);

        if (urgent)
        {
            urgentCoroutine = StartCoroutine(UrgentBlink());
        }
        else
        {
            urgentCoroutine = null;
            SetContentAlpha(1f);
        }
    }

    private IEnumerator UrgentBlink()
    {
        const float blinkInterval = 0.22f;
        while (true)
        {
            SetContentAlpha(0.35f);
            yield return new WaitForSeconds(blinkInterval);
            SetContentAlpha(1f);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SetContentAlpha(float a)
    {
        SetImageAlpha(background, a);
        SetImageAlpha(dish1Image, a);
        if (dish2Image != null && dish2Image.gameObject.activeSelf) SetImageAlpha(dish2Image, a);
        if (plusLabel != null && plusLabel.gameObject.activeSelf)
        {
            Color c = plusLabel.color;
            c.a = a;
            plusLabel.color = c;
        }
    }

    private static void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    // ── Private ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Forces an Image's RectTransform into a known-good state: centered
    /// anchors/pivot, no leftover scale or rotation. Without this, a bubble
    /// that was hand-tweaked in the editor (e.g. resized by dragging a scale
    /// handle instead of the Width/Height fields, or left with stretched
    /// anchors from copy/pasting between Pig and Dog prefabs) will silently
    /// ignore the sizeDelta the script sets below — which is exactly what
    /// makes one character's dish image look correctly sized and another's
    /// look wrong even though both run the same code.
    /// </summary>
    private static void ResetImageRect(Image img)
    {
        if (img == null) return;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    private void LayoutAndScale(bool hasAddOn)
    {
        float bgW, bgH, targetWorldW;

        if (hasAddOn)
        {
            float totalW = dishSize + spacing + plusWidth + spacing + dishSize;
            bgW = totalW + padding;
            bgH = dishSize + padding;
            targetWorldW = plusBubbleWidth;

            float half = totalW * 0.5f;
            SetPos(dish1Image, new Vector2(-half + dishSize * 0.5f, 0f));
            SetPlusLabel(plusWidth);
            SetPos(dish2Image, new Vector2( half - dishSize * 0.5f, 0f));
        }
        else
        {
            bgW = dishSize + padding;
            bgH = dishSize + padding;
            targetWorldW = singleBubbleWidth;
            SetPos(dish1Image, Vector2.zero);
        }

        if (background != null)
            background.rectTransform.sizeDelta = new Vector2(bgW, bgH);

        // Scale canvas so the background appears targetWorldW metres wide.
        float parentSc = transform.parent != null ? transform.parent.lossyScale.x : 1f;
        float s = parentSc > 0f ? targetWorldW / (bgW * parentSc) : 0.003f;
        shownScale = Vector3.one * s;

        // World height of the bubble body at this scale — independent of
        // parentSc since it cancels out (same ratio as the width scale above).
        // bgH is the same whether or not there's an add-on, but computing it
        // this way keeps it correct even if that ever changes.
        CurrentWorldHeight = bgW > 0f ? bgH * targetWorldW / bgW : 0f;
    }

    private void SetPos(Image img, Vector2 pos)
    {
        if (img != null) img.rectTransform.anchoredPosition = pos;
    }

    private void SetPlusLabel(float w)
    {
        if (plusLabel == null) return;
        RectTransform rt = plusLabel.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(w, dishSize);
        plusLabel.fontSize    = dishSize * 0.85f;
        plusLabel.alignment   = TextAlignmentOptions.Center;
        plusLabel.text        = "+";
    }

    private IEnumerator PopIn()
    {
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / popDuration);
            const float c = 1.70158f;
            float q  = p - 1f;
            float sc = q * q * ((c + 1f) * q + c) + 1f;
            transform.localScale = shownScale * Mathf.Max(0f, sc);
            yield return null;
        }
        transform.localScale = shownScale;
    }

#if UNITY_EDITOR
    // Draws the head anchor + resting position in the Scene view so you can
    // see at a glance whether it's actually sitting where you think it is.
    private void OnDrawGizmosSelected()
    {
        if (headAnchor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(headAnchor.position, 0.05f);
            Gizmos.DrawLine(headAnchor.position, headAnchor.position + Vector3.up * extraAboveHead);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(headAnchor.position + Vector3.up * extraAboveHead, 0.03f);
        }
    }
#endif
}