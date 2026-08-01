using System.Collections;
using UnityEngine;

// Lives on the FireIcon object (the flame at the right end of the Patience
// bar). Purely reactive — ActivePatienceUI tells it the current patience
// fraction every frame, and it escalates on its own: past the low-patience
// threshold it starts shaking and growing, the closer to 0 the bigger/more
// violent the shake gets, then Blast() plays a quick burst pop the instant
// patience actually hits 0 (customer starts yelling / leaving angry).
public class PatienceFireIcon : MonoBehaviour
{
    [Tooltip("Leave empty to auto-grab the RectTransform on this same GameObject.")]
    public RectTransform fireRect;

    [Header("Escalation (threshold -> 0 patience)")]
    [Tooltip("Scale at the instant patience crosses the low threshold.")]
    public float baseScale = 1f;
    [Tooltip("Scale reached right as patience hits 0 (just before the blast).")]
    public float maxScale = 1.4f;
    [Tooltip("Max random shake offset, in UI pixels, reached right as patience hits 0.")]
    public float maxShakeMagnitude = 6f;

    [Header("Blast (plays once, the instant patience hits 0)")]
    public float blastScale = 2.1f;
    public float blastDuration = 0.35f;

    private Vector3 baseLocalScale;
    private Vector2 baseAnchoredPos;
    private bool initialized;
    private Coroutine blastCoroutine;

    void Awake()
    {
        if (fireRect == null) fireRect = GetComponent<RectTransform>();
        Init();
    }

    private void Init()
    {
        if (initialized || fireRect == null) return;
        baseLocalScale = fireRect.localScale;
        baseAnchoredPos = fireRect.anchoredPosition;
        initialized = true;
    }

    /// <summary>
    /// Called every frame while a customer is active. fraction = remaining
    /// patience (1 = full, 0 = none). lowThreshold = the same value driving
    /// the thought bubble's blink, so everything escalates together.
    /// </summary>
    public void UpdateFraction(float fraction, float lowThreshold)
    {
        Init();
        if (blastCoroutine != null) return; // mid-blast — let it finish undisturbed

        if (fraction > lowThreshold || lowThreshold <= 0f)
        {
            ResetFire();
            return;
        }

        // t: 0 right at the threshold, 1 right at zero patience
        float t = Mathf.Clamp01(1f - fraction / lowThreshold);
        float scale = Mathf.Lerp(baseScale, maxScale, t);
        float shakeMag = Mathf.Lerp(0f, maxShakeMagnitude, t);
        Vector2 offset = new Vector2(Random.Range(-shakeMag, shakeMag), Random.Range(-shakeMag, shakeMag));

        fireRect.localScale = baseLocalScale * scale;
        fireRect.anchoredPosition = baseAnchoredPos + offset;
    }

    public void ResetFire()
    {
        Init();
        if (blastCoroutine != null) return;
        fireRect.localScale = baseLocalScale;
        fireRect.anchoredPosition = baseAnchoredPos;
    }

    /// <summary>Call the instant patience actually reaches 0.</summary>
    public void Blast()
    {
        Init();
        if (blastCoroutine != null) StopCoroutine(blastCoroutine);
        blastCoroutine = StartCoroutine(BlastRoutine());
    }

    private IEnumerator BlastRoutine()
    {
        float t = 0f;
        while (t < blastDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / blastDuration);
            float scale = Mathf.Lerp(maxScale, blastScale, EaseOutBack(p));
            fireRect.localScale = baseLocalScale * scale;
            fireRect.anchoredPosition = baseAnchoredPos; // hold still for the pop itself
            yield return null;
        }
        fireRect.localScale = baseLocalScale;
        fireRect.anchoredPosition = baseAnchoredPos;
        blastCoroutine = null;
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float xm1 = x - 1f;
        return 1f + c3 * xm1 * xm1 * xm1 + c1 * xm1 * xm1;
    }
}
