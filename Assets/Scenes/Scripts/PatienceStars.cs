using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Attach to the PatienceStarsCanvas (World Space Canvas, child of pig).
// Add 5 Image components in a row inside the canvas and drag them into the stars array.
//
// Hierarchy to build in Unity:
//
//   PatienceStarsCanvas  [Canvas – World Space]  ← this script here
//     └── StarsRow       [Horizontal Layout Group]
//           ├── Star1    [Image]  ← star sprite, gold colour
//           ├── Star2    [Image]
//           ├── Star3    [Image]
//           ├── Star4    [Image]
//           └── Star5    [Image]
public class PatienceStars : MonoBehaviour
{
    [Tooltip("Drag 5 star Image components here, left to right.")]
    public Image[] stars = new Image[5];

    public Color starFull  = new Color(1f, 0.84f, 0f, 1f);          // gold
    public Color starEmpty = new Color(0.35f, 0.35f, 0.35f, 0.4f);  // grey

    private Coroutine running;

    void Awake()
    {
        SetAll(full: true);
        gameObject.SetActive(false);
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public void StartCounting(float duration, System.Action onOut)
    {
        gameObject.SetActive(true);
        SetAll(full: true);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Countdown(duration, onOut));
    }

    public void Stop()
    {
        if (running != null) { StopCoroutine(running); running = null; }
        gameObject.SetActive(false);
    }

    // ── Private ─────────────────────────────────────────────────────────────

    private IEnumerator Countdown(float duration, System.Action onOut)
    {
        float elapsed = 0f;
        int prevCount = stars.Length;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float ratio    = 1f - (elapsed / duration);
            int   newCount = Mathf.CeilToInt(ratio * stars.Length);

            if (newCount != prevCount)
            {
                prevCount = newCount;
                ApplyVisuals(newCount);
                if (newCount >= 0 && newCount < stars.Length)
                    StartCoroutine(WobbleStar(newCount)); // wobble the star that just went grey
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

    private void ApplyVisuals(int activeCount)
    {
        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null)
                stars[i].color = i < activeCount ? starFull : starEmpty;
    }

    private void SetAll(bool full)
    {
        foreach (var s in stars)
            if (s != null) s.color = full ? starFull : starEmpty;
    }
}
