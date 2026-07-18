using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Attach to the PatienceStars World Space Canvas child of each customer.
//
// POSITIONING
// ───────────
// Preferred setup: assign `followBubble` to THIS character's own ThoughtBubble
// component (the same one wired into CustomerController.thoughtBubble). Every
// frame, PatienceStars then reads the bubble's actual current world position
// and actual current world height and sits `gapAboveBubble` metres above the
// TOP of the bubble — not a separately-guessed head offset. This is what
// guarantees "stars exactly above the bubble": they're mathematically locked
// to the bubble's own position/size, so they can't drift apart from it the
// way two independently-tuned offsets can.
//
// If followBubble is left empty, falls back to headAnchor (same idea as
// ThoughtBubble.headAnchor — an empty child Transform at the head), and if
// THAT is empty too, falls back further to the old SkinnedMeshRenderer
// bounds-guessing. Each fallback is progressively less reliable.
public class PatienceStars : MonoBehaviour
{
    [Tooltip("Drag 5 star Images here, left to right.")]
    public Image[] stars = new Image[5];

    public Color starFull  = new Color(1f, 0.84f, 0f, 1f);
    public Color starEmpty = new Color(0.35f, 0.35f, 0.35f, 0.4f);

    [Header("World Size")]
    [Tooltip("How wide the star row appears in world space (metres).")]
    public float desiredWorldWidth = 0.5f;

    [Header("Follow Bubble (recommended — locks stars exactly above it)")]
    [Tooltip("This character's own ThoughtBubble component. When set, the stars " +
             "are positioned using the bubble's real position + real height, so " +
             "they always sit directly above it regardless of bubble size or " +
             "which counter slot the character is in.")]
    public ThoughtBubble followBubble;
    [Tooltip("Extra world metres of gap between the top of the bubble and the stars.")]
    public float gapAboveBubble = 0.05f;

    [Header("Head Anchor (fallback if Follow Bubble is empty)")]
    [Tooltip("Same Transform you dragged into ThoughtBubble.headAnchor for this " +
             "character. Only used if Follow Bubble above is not set.")]
    public Transform headAnchor;

    [Header("Auto Head Position (legacy fallback)")]
    [Tooltip("Extra world metres above the head anchor / top of the mesh " +
             "(should be > ThoughtBubble's extraAboveHead so stars sit above the bubble). " +
             "Only used if neither Follow Bubble nor Head Anchor above is set.")]
    public float extraAboveHead = 0.45f;

    // ── internals ───────────────────────────────────────────────────────────
    private Coroutine           running;
    private Camera              mainCam;
    private SkinnedMeshRenderer smr;   // only used by the legacy fallback path

    void Awake()
    {
        mainCam = Camera.main;

        // Force the canvas pivot to dead-center so scaling (including a
        // wrong Desired World Width) only grows/shrinks the row in place
        // instead of skewing it sideways toward whichever edge the pivot
        // happened to be biased to.
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.pivot = new Vector2(0.5f, 0.5f);

        // Auto-scale canvas so stars appear desiredWorldWidth metres wide.
        float canvasW = (rt != null && rt.sizeDelta.x > 1f) ? rt.sizeDelta.x : 200f;
        float parentS = transform.parent != null ? transform.parent.lossyScale.x : 1f;
        float s       = parentS > 0f ? desiredWorldWidth / (canvasW * parentS) : 0.002f;
        transform.localScale = Vector3.one * s;

        // Start hidden via alpha.
        SetAllAlpha(0f);
    }

    private float logTimer; // debug position logging (see below)

    void LateUpdate()
    {
        if (transform.parent == null) return;

        Vector3 p;
        string  source;

        if (followBubble != null)
        {
            // Preferred path: lock to the bubble's own position + real height.
            // Note: relies on ThoughtBubble.LateUpdate having already run this
            // frame. Both scripts only move while the character is stationary
            // (patience counting starts after the walk finishes), so the usual
            // one-frame script-order lag between LateUpdate calls isn't visible
            // in practice. If you ever DO see a one-frame jitter, set Project
            // Settings → Script Execution Order so ThoughtBubble runs before
            // PatienceStars.
            p = followBubble.transform.position;
            p.y += followBubble.CurrentWorldHeight + gapAboveBubble;
            source = $"followBubble='{followBubble.name}' (bubble parent='{followBubble.transform.parent?.name}', bubble pos={followBubble.transform.position:F2}, bubbleHeight={followBubble.CurrentWorldHeight:F3})";
        }
        else if (headAnchor != null)
        {
            p = headAnchor.position;
            p.y += extraAboveHead;
            source = $"headAnchor='{headAnchor.name}'";
        }
        else
        {
            // Legacy fallback: guess the head height from mesh bounds.
            if (smr == null)
                smr = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();

            if (smr == null)
            {
                Debug.LogWarning($"⭐ PatienceStars on parent='{transform.parent?.name}': no followBubble/headAnchor set AND no SkinnedMeshRenderer found — stars are not being positioned at all this frame.");
                return; // nothing to position against yet
            }

            p = transform.parent.position;      // correct X and Z
            p.y = smr.bounds.max.y + extraAboveHead;
            source = $"SMR-fallback smr='{smr.name}'";
        }

        transform.position = p;

        if (mainCam != null && IsVisible())
            transform.rotation = Quaternion.LookRotation(
                transform.position - mainCam.transform.position);

        // Debug: print resolved position once a second so you can see exactly
        // which character's PatienceStars is being triggered and where it
        // thinks it should be. Compare the "parent=" here against which
        // customer is actually counting down. Remove this block once fixed.
        logTimer += Time.deltaTime;
        if (logTimer > 1f)
        {
            logTimer = 0f;
            Debug.Log($"⭐ PatienceStars GO='{gameObject.name}' parent='{transform.parent?.name}' visible={IsVisible()} using {source} -> world pos={transform.position:F2}");
        }
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public void StartCounting(float duration, System.Action onOut)
    {
        SetAll(full: true);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Countdown(duration, onOut));
    }

    public void Stop()
    {
        if (running != null) { StopCoroutine(running); running = null; }
        SetAllAlpha(0f);
    }

    // ── Private ─────────────────────────────────────────────────────────────

    private IEnumerator Countdown(float duration, System.Action onOut)
    {
        float elapsed   = 0f;
        int   prevCount = stars.Length;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int newCount = Mathf.CeilToInt((1f - elapsed / duration) * stars.Length);
            if (newCount != prevCount)
            {
                prevCount = newCount;
                ApplyVisuals(newCount);
                if (newCount >= 0 && newCount < stars.Length)
                    StartCoroutine(WobbleStar(newCount));
            }
            yield return null;
        }
        ApplyVisuals(0);
        yield return new WaitForSeconds(0.25f);
        onOut?.Invoke();
    }

    private IEnumerator WobbleStar(int index)
    {
        if (index < 0 || index >= stars.Length || stars[index] == null) yield break;
        Transform t = stars[index].transform;
        Vector3 orig = t.localScale;
        float half = 0.07f, e = 0f;
        while (e < half) { e += Time.deltaTime; t.localScale = orig * Mathf.Lerp(1f, 1.35f, e / half); yield return null; }
        e = 0f;
        while (e < half) { e += Time.deltaTime; t.localScale = orig * Mathf.Lerp(1.35f, 1f, e / half); yield return null; }
        t.localScale = orig;
    }

    private void ApplyVisuals(int n)
    {
        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null)
                stars[i].color = i < n ? starFull : starEmpty;
    }

    private void SetAll(bool full)
    {
        foreach (var s in stars)
            if (s != null) s.color = full ? starFull : starEmpty;
    }

    private void SetAllAlpha(float a)
    {
        foreach (var s in stars)
        {
            if (s == null) continue;
            Color c = s.color; c.a = a; s.color = c;
        }
    }

    private bool IsVisible() =>
        stars.Length > 0 && stars[0] != null && stars[0].color.a > 0.01f;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (followBubble != null)
        {
            Gizmos.color = Color.green;
            Vector3 top = followBubble.transform.position + Vector3.up * followBubble.CurrentWorldHeight;
            Gizmos.DrawLine(followBubble.transform.position, top);
            Gizmos.DrawWireSphere(top + Vector3.up * gapAboveBubble, 0.03f);
        }
        else if (headAnchor != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(headAnchor.position + Vector3.up * extraAboveHead, 0.03f);
        }
    }
#endif
}