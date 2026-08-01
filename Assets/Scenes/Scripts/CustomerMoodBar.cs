using UnityEngine;
using UnityEngine.UI;

// A small world-space "patience" bar that sits LOW near the customer's base
// (deliberately NOT above the head — the thought bubble already lives there,
// and with two customers standing close together, anything else stacked
// above their heads gets hard to tell apart at a glance). Positioning this
// low, near ankle height, keeps it clearly associated with one specific
// customer no matter how crowded the counter gets.
//
// Setup (per character prefab, e.g. Pig and Dog each get their own):
//   1. Create a child GameObject under the customer root, name it e.g.
//      "MoodBar". Add a Canvas (Render Mode: World Space), Canvas Scaler,
//      and Graphic Raycaster is NOT needed (nothing to click).
//   2. Under it, add a background Image (a dark/empty bar) and a foreground
//      Image with Image Type = Filled, Fill Method = Horizontal, Fill Origin
//      = Left — this is the bar that actually depletes.
//   3. Position the whole MoodBar object low and slightly BEHIND the
//      character (away from the camera) — try local position around
//      (0, 0.1, -0.15), tune by eye per character.
//   4. Scale the Canvas down small (like 0.003-0.006) so it reads as a
//      reasonably-sized bar in the 3D scene, same idea as ThoughtBubble's
//      own canvas scaling.
//   5. Start the MoodBar GameObject DISABLED (unchecked) in the prefab —
//      Show()/Hide() below control its visibility at runtime.
//   6. Drag this component into the customer's CustomerController ->
//      Mood Bar field.
public class CustomerMoodBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("An Image with Image Type = Filled, Fill Method = Horizontal.")]
    public Image fillImage;

    [Header("Colors — green (calm) -> yellow (getting impatient) -> red (about to leave)")]
    public Color calmColor    = new Color(0.35f, 0.8f, 0.35f);
    public Color warningColor = new Color(0.95f, 0.75f, 0.15f);
    public Color angryColor   = new Color(0.85f, 0.2f, 0.2f);
    [Range(0f, 1f)] public float warningThreshold = 0.5f;
    [Range(0f, 1f)] public float angryThreshold   = 0.2f;

    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        // Billboard — always face the camera, same trick ThoughtBubble uses,
        // so the bar reads correctly no matter which way the character turns.
        if (mainCam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetFraction(1f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// fraction: 1 = full patience (just arrived), 0 = out of patience.
    /// Called every frame by CustomerController while a customer is AtStation.
    /// </summary>
    public void SetFraction(float fraction)
    {
        fraction = Mathf.Clamp01(fraction);
        if (fillImage == null) return;

        fillImage.fillAmount = fraction;

        if (fraction <= angryThreshold)
        {
            fillImage.color = angryColor;
        }
        else if (fraction <= warningThreshold)
        {
            float t = (fraction - angryThreshold) / Mathf.Max(0.0001f, warningThreshold - angryThreshold);
            fillImage.color = Color.Lerp(angryColor, warningColor, t);
        }
        else
        {
            float t = (fraction - warningThreshold) / Mathf.Max(0.0001f, 1f - warningThreshold);
            fillImage.color = Color.Lerp(warningColor, calmColor, t);
        }
    }
}
