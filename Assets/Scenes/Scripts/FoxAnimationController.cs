using System.Collections;
using UnityEngine;

public class FoxAnimationController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Sequence Durations")]
    [Tooltip("Sum of all correct sequence clip lengths (ThrustSlash + MeleeAttack + TorchBurn + ThrustSlash2).")]
    public float correctSequenceDuration = 4f;

    [Tooltip("Sum of wrong sequence clip lengths (SadKick + RightTurnDefeated).")]
    public float wrongSequenceDuration = 2.5f;

    [Header("VFX — leave empty if not set up yet")]
    public ParticleSystem chopEffect;
    public ParticleSystem steamEffect;

    private Coroutine sequenceCoroutine;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("❌ FoxAnimationController: No Animator found on " + gameObject.name + "!");

        // Make sure steam is off at start
        if (steamEffect != null) steamEffect.Stop();
    }

    // Called by VideoDropTarget when a correct word is dropped.
    public void PlayCorrectSequence(System.Action onComplete = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (steamEffect != null) steamEffect.Stop(); // stop any leftover steam
        animator.ResetTrigger("PlayWrong");
        animator.SetTrigger("PlayCorrect");
        if (chopEffect != null) chopEffect.Play();
        Debug.Log("🦊 Fox starts cooking!");
        sequenceCoroutine = StartCoroutine(CorrectSequenceRoutine(onComplete));
    }

    // Called by VideoDropTarget when a wrong word is dropped.
    public void PlayWrongSequence(System.Action onComplete = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (steamEffect != null) steamEffect.Stop(); // no steam on wrong answer
        animator.ResetTrigger("PlayCorrect");
        animator.SetTrigger("PlayWrong");
        Debug.Log("😔 Fox defeated.");
        sequenceCoroutine = StartCoroutine(WaitThenCallback(wrongSequenceDuration, onComplete));
    }

    // Correct sequence: wait 0.5s, play steam, wait rest of duration, stop steam, fire callback.
    private IEnumerator CorrectSequenceRoutine(System.Action onComplete)
    {
        // Small delay before steam appears
        yield return new WaitForSeconds(0.5f);

        if (steamEffect != null)
        {
            steamEffect.Play();
            Debug.Log("💨 Steam rising!");
        }

        // Wait the rest of the sequence (total - 0.5s already waited)
        yield return new WaitForSeconds(correctSequenceDuration - 0.5f);

        // Sequence done — stop steam and advance video
        if (steamEffect != null) steamEffect.Stop();
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

    public void OnChopFrame()
    {
        if (chopEffect != null) chopEffect.Play();
    }

    public void OnFoodReady()
    {
        Debug.Log("🍜 Food is ready!");
    }
}
