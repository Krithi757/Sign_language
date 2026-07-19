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

    [Header("Audio")]
    [Tooltip("Add an AudioSource component to this GameObject (uncheck 'Play On Awake') and drag it here.")]
    public AudioSource audioSource;
    [Tooltip("Looping cooking/sizzle sound — starts the instant cooking begins, stops the instant the meal appears.")]
    public AudioClip cookingSound;
    [Tooltip("Short 'ding' — plays once, right when the finished meal appears on the counter.")]
    public AudioClip orderCompleteSound;

    private Coroutine sequenceCoroutine;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("❌ FoxAnimationController: No Animator found on " + gameObject.name + "!");
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        StopAllVFX();
    }

    public void PlayCorrectSequence(
        System.Action    onComplete  = null,
        ParticleSystem   uniqueVFX   = null,
        float            effectDelay = 0f,
        MealAppearEffect mealToShow  = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        StopAllVFX();
        animator.ResetTrigger("PlayWrong");
        animator.SetTrigger("PlayCorrect");
        Debug.Log("🦊 Fox starts cooking!");

        // Start the cooking sound loop now — it runs for as long as cooking
        // takes and is stopped the instant the meal appears (see below), so
        // it doesn't matter how long the actual clip is relative to
        // correctSequenceDuration.
        if (audioSource != null && cookingSound != null)
        {
            audioSource.Stop();
            audioSource.clip = cookingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        sequenceCoroutine = StartCoroutine(CorrectSequenceRoutine(onComplete, uniqueVFX, effectDelay, mealToShow));
    }

    public void PlayWrongSequence(System.Action onComplete = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        StopAllVFX();
        // Defensive — cooking sound shouldn't normally be playing here (wrong
        // answers happen before cooking starts), but stop it just in case.
        if (audioSource != null) { audioSource.loop = false; audioSource.Stop(); }
        animator.ResetTrigger("PlayCorrect");
        animator.SetTrigger("PlayWrong");
        Debug.Log("😔 Fox defeated.");
        sequenceCoroutine = StartCoroutine(WaitThenCallback(wrongSequenceDuration, onComplete));
    }

    private IEnumerator CorrectSequenceRoutine(
        System.Action    onComplete,
        ParticleSystem   uniqueVFX,
        float            effectDelay,
        MealAppearEffect mealToShow)
    {
        float elapsed = 0f;

        // 0.5s — fox has started moving, base VFX kick in
        yield return new WaitForSeconds(0.5f);
        elapsed += 0.5f;

        if (steamEffect      != null) steamEffect.Play();
        yield return new WaitForSeconds(0.03f);
        elapsed += 0.03f;
        if (fireEffect       != null) fireEffect.Play();
        if (foodDebrisEffect != null) foodDebrisEffect.Play();

        // Fire the recipe's unique VFX at the configured delay (optional)
        if (effectDelay > 0f && uniqueVFX != null)
        {
            float waitForEffect = effectDelay - elapsed;
            if (waitForEffect > 0f) yield return new WaitForSeconds(waitForEffect);
            elapsed = effectDelay;
            uniqueVFX.Play();
            Debug.Log("✨ Unique VFX!");
        }

        // Wait out the rest of the sequence
        float remaining = correctSequenceDuration - elapsed;
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        // Done — clear cooking VFX and show finished meal
        StopAllVFX();
        if (uniqueVFX != null) StopPS(uniqueVFX);
        if (mealToShow != null) mealToShow.Show();

        // Cooking sound stops, order-complete ding plays — right as the meal appears.
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            if (orderCompleteSound != null) audioSource.PlayOneShot(orderCompleteSound);
        }

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