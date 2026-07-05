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

    [Header("VFX — drag particle systems here, leave empty if not ready")]
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

    public void PlayCorrectSequence(System.Action onComplete = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        StopAllVFX();
        animator.ResetTrigger("PlayWrong");
        animator.SetTrigger("PlayCorrect");
        Debug.Log("🦊 Fox starts cooking!");
        sequenceCoroutine = StartCoroutine(CorrectSequenceRoutine(onComplete));
    }

    public void PlayWrongSequence(System.Action onComplete = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        StopAllVFX(); // no effects on wrong answer
        animator.ResetTrigger("PlayCorrect");
        animator.SetTrigger("PlayWrong");
        Debug.Log("😔 Fox defeated.");
        sequenceCoroutine = StartCoroutine(WaitThenCallback(wrongSequenceDuration, onComplete));
    }

    private IEnumerator CorrectSequenceRoutine(System.Action onComplete)
    {
        // Wait 0.5s — fox has started moving
        yield return new WaitForSeconds(0.5f);

        // Steam starts first
        if (steamEffect != null) { steamEffect.Play(); Debug.Log("💨 Steam!"); }

        // Fire starts 0.03s after steam
        yield return new WaitForSeconds(0.03f);
        if (fireEffect != null) { fireEffect.Play(); Debug.Log("🔥 Fire!"); }

        // Food debris burst immediately after fire
        if (foodDebrisEffect != null) { foodDebrisEffect.Play(); Debug.Log("🥕 Food flying!"); }

        // Wait out the rest of the sequence
        // (0.5 + 0.03 already waited, subtract from total)
        float remaining = correctSequenceDuration - 0.5f - 0.03f;
        yield return new WaitForSeconds(remaining);

        // Sequence done — stop everything, advance video
        StopAllVFX();
        animator.ResetTrigger("PlayCorrect");
        animator.ResetTrigger("PlayWrong");
        sequenceCoroutine = null;
        onComplete?.Invoke();
        Debug.Log("✅ Sequence done — video should now advance.");
    }

    private IEnumerator WaitThenCallback(float duration, System.Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        animator.ResetTrigger("PlayCorrect");
        animator.ResetTrigger("PlayWrong");
        sequenceCoroutine = null;
        onComplete?.Invoke();
        Debug.Log("✅ Sequence done — video should now advance.");
    }

    private void StopAllVFX()
    {
        if (steamEffect != null) steamEffect.Stop();
        if (fireEffect != null) fireEffect.Stop();
        if (foodDebrisEffect != null) foodDebrisEffect.Stop();
    }

    public void OnChopFrame()
    {
        // Optionally trigger an extra food burst on a specific animation frame
        if (foodDebrisEffect != null) foodDebrisEffect.Play();
    }

    public void OnFoodReady()
    {
        Debug.Log("🍜 Food is ready!");
    }
}