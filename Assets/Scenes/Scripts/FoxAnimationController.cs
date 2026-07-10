using System.Collections;
using UnityEngine;

public class FoxAnimationController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Sequence Durations")]
    [Tooltip("Sum of all correct sequence clip lengths.")]
    public float correctSequenceDuration = 4f;

    [Tooltip("Sum of wrong sequence clip lengths.")]
    public float wrongSequenceDuration = 2.5f;

    [Header("Base VFX — always play on any correct answer")]
    public ParticleSystem steamEffect;
    public ParticleSystem fireEffect;
    public ParticleSystem foodDebrisEffect;

    private Coroutine sequenceCoroutine;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("❌ FoxAnimationController: No Animator found on " + gameObject.name + "!");
        StopAllVFX();
    }

    // Called by OrderManager. Accepts an optional unique end VFX and meal per recipe.
    public void PlayCorrectSequence(
        System.Action onComplete   = null,
        ParticleSystem uniqueEndVFX = null,
        MealAppearEffect mealToShow = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        StopAllVFX();
        animator.ResetTrigger("PlayWrong");
        animator.SetTrigger("PlayCorrect");
        Debug.Log("🦊 Fox starts cooking!");
        sequenceCoroutine = StartCoroutine(CorrectSequenceRoutine(onComplete, uniqueEndVFX, mealToShow));
    }

    // Called by OrderManager on wrong answer.
    public void PlayWrongSequence(System.Action onComplete = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        StopAllVFX();
        animator.ResetTrigger("PlayCorrect");
        animator.SetTrigger("PlayWrong");
        Debug.Log("😔 Fox defeated.");
        sequenceCoroutine = StartCoroutine(WaitThenCallback(wrongSequenceDuration, onComplete));
    }

    private IEnumerator CorrectSequenceRoutine(
        System.Action onComplete,
        ParticleSystem uniqueEndVFX,
        MealAppearEffect mealToShow)
    {
        // 0.5s — fox has started moving
        yield return new WaitForSeconds(0.5f);

        // Base effects — always play
        if (steamEffect     != null) { steamEffect.Play();     Debug.Log("💨 Steam!"); }
        yield return new WaitForSeconds(0.03f);
        if (fireEffect      != null) { fireEffect.Play();      Debug.Log("🔥 Fire!"); }
        if (foodDebrisEffect != null)  foodDebrisEffect.Play();

        // Wait until 0.6s before the sequence ends, then play the recipe's unique effect
        float waitBeforeEnd = correctSequenceDuration - 0.5f - 0.03f - 0.6f;
        if (waitBeforeEnd > 0f) yield return new WaitForSeconds(waitBeforeEnd);

        // Unique per-recipe VFX (cheese sparkle, big flame, egg sizzle, etc.)
        if (uniqueEndVFX != null) { uniqueEndVFX.Play(); Debug.Log("✨ Unique recipe VFX!"); }

        // Final 0.6s
        yield return new WaitForSeconds(0.6f);

        // Sequence done
        StopAllVFX();
        if (uniqueEndVFX != null) uniqueEndVFX.Stop();

        // Show the recipe's finished meal with drop + ping effect
        if (mealToShow != null) mealToShow.Show();

        animator.ResetTrigger("PlayCorrect");
        animator.ResetTrigger("PlayWrong");
        sequenceCoroutine = null;
        onComplete?.Invoke();
        Debug.Log("✅ Sequence done — order complete.");
    }

    private IEnumerator WaitThenCallback(float duration, System.Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        animator.ResetTrigger("PlayCorrect");
        animator.ResetTrigger("PlayWrong");
        sequenceCoroutine = null;
        onComplete?.Invoke();
        Debug.Log("✅ Wrong sequence done.");
    }

    private void StopAllVFX()
    {
        StopPS(steamEffect);
        StopPS(fireEffect);
        StopPS(foodDebrisEffect);
    }

    private void StopPS(ParticleSystem ps)
    {
        // StopEmittingAndClear removes all existing particles instantly
        // so cooking steam vanishes immediately when the meal appears
        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void OnChopFrame()
    {
        if (foodDebrisEffect != null) foodDebrisEffect.Play();
    }

    public void OnFoodReady()
    {
        Debug.Log("🍜 Food is ready!");
    }
}
