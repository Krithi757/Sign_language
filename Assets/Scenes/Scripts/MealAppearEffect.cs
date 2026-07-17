using System.Collections;
using UnityEngine;

// Attach this to your prepared meal GameObject.
// Drag the MealSteam child Particle System into the mealSteam slot.
public class MealAppearEffect : MonoBehaviour
{
    [Tooltip("How far above the final position it starts.")]
    public float dropHeight = 0.5f;

    [Tooltip("How long the drop takes in seconds.")]
    public float dropDuration = 0.35f;

    [Tooltip("Overshoot amount on the bounce ping.")]
    public float punchScale = 1.25f;

    [Tooltip("How long the meal stays visible before auto-hiding. Set to 0 to never auto-hide.")]
    public float autoHideDelay = 2f;

    [Header("Meal Steam")]
    [Tooltip("Drag the MealSteam Particle System (child of this meal) here.")]
    public ParticleSystem mealSteam;

    private Vector3 finalPosition;
    private Vector3 finalScale;
    private Coroutine autoHideCoroutine;

    void Awake()
    {
        finalPosition = transform.position;
        finalScale    = transform.localScale;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        // Cancel any pending auto-hide from a previous show
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        gameObject.SetActive(true);
        if (mealSteam != null) mealSteam.Play();
        StartCoroutine(AppearRoutine());
    }

    public void Hide()
    {
        // Cancel auto-hide if it was waiting
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        if (mealSteam != null)
            mealSteam.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        gameObject.SetActive(false);
        transform.position   = finalPosition;
        transform.localScale = finalScale;
    }

    private IEnumerator AppearRoutine()
    {
        transform.position   = finalPosition + Vector3.up * dropHeight;
        transform.localScale = Vector3.zero;

        // Phase 1 — drop and scale up
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / dropDuration), 3f);
            transform.position   = Vector3.Lerp(finalPosition + Vector3.up * dropHeight, finalPosition, eased);
            transform.localScale = finalScale * eased;
            yield return null;
        }

        transform.position   = finalPosition;
        transform.localScale = finalScale;

        // Phase 2 — ping (overshoot then settle)
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

        // Phase 3 — auto-hide after delay
        if (autoHideDelay > 0f)
            autoHideCoroutine = StartCoroutine(AutoHideAfterDelay());
    }

    private IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        Debug.Log("🍽 Meal cleared after " + autoHideDelay + "s.");
        Hide();
    }
}
