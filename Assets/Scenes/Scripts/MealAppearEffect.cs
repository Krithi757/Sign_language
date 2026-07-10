using System.Collections;
using UnityEngine;

// Attach this to your prepared meal GameObject.
// Drag the MealSteam child Particle System into the mealSteam slot.
public class MealAppearEffect : MonoBehaviour
{
    [Tooltip("How far above the final position it starts. 0.5 = subtle drop.")]
    public float dropHeight = 0.5f;

    [Tooltip("How long the drop takes in seconds.")]
    public float dropDuration = 0.35f;

    [Tooltip("How much it overshoots on the bounce ping. 1.25 = 25% bigger then settles.")]
    public float punchScale = 1.25f;

    [Header("Meal Steam")]
    [Tooltip("Drag the MealSteam Particle System (child of this meal) here.")]
    public ParticleSystem mealSteam;

    private Vector3 finalPosition;
    private Vector3 finalScale;

    void Awake()
    {
        finalPosition = transform.position;
        finalScale    = transform.localScale;

        // Hide meal and make sure steam is off at start
        gameObject.SetActive(false);
    }

    // Called by FoxAnimationController when cooking sequence ends
    public void Show()
    {
        gameObject.SetActive(true);

        // Start steam immediately as the meal drops in
        if (mealSteam != null) mealSteam.Play();

        StartCoroutine(AppearRoutine());
    }

    // Called when the next round starts
    public void Hide()
    {
        if (mealSteam != null)
            mealSteam.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        gameObject.SetActive(false);
        transform.position   = finalPosition;
        transform.localScale = finalScale;
    }

    private IEnumerator AppearRoutine()
    {
        // Start above final position, scaled to zero
        transform.position   = finalPosition + Vector3.up * dropHeight;
        transform.localScale = Vector3.zero;

        // --- Phase 1: Drop down while scaling up ---
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t     = elapsed / dropDuration;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease-out cubic

            transform.position   = Vector3.Lerp(finalPosition + Vector3.up * dropHeight, finalPosition, eased);
            transform.localScale = finalScale * eased;

            yield return null;
        }

        // Snap to final position
        transform.position   = finalPosition;
        transform.localScale = finalScale;

        // --- Phase 2: Punch scale ping ---
        float punchUp = 0.12f;
        elapsed = 0f;
        while (elapsed < punchUp)
        {
            elapsed += Time.deltaTime;
            transform.localScale = finalScale * Mathf.Lerp(1f, punchScale, elapsed / punchUp);
            yield return null;
        }

        float punchDown = 0.1f;
        elapsed = 0f;
        while (elapsed < punchDown)
        {
            elapsed += Time.deltaTime;
            transform.localScale = finalScale * Mathf.Lerp(punchScale, 1f, elapsed / punchDown);
            yield return null;
        }

        transform.localScale = finalScale;
        Debug.Log("🍜 Meal appeared!");
    }
}
