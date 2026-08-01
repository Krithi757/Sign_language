using System.Collections;
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum State { Hidden, Walking, AtStation, Served }

    [Header("Positions")]
    public Transform spawnPoint;
    public Transform stationPoint;

    [Header("Movement")]
    [Tooltip("How fast the pig physically slides toward the station. Tune this in Play mode.")]
    public float walkSpeed = 0.4f;
    [Tooltip("How fast the walk animation plays. 1 = normal, 0.5 = half speed. Tune separately from walkSpeed.")]
    public float walkAnimSpeed = 0.5f;
    [Tooltip("Y rotation so the pig faces the counter when it arrives.")]
    public float counterFacingYaw = 180f;

    [Header("Animator")]
    public Animator animator;
    public string walkBool       = "Walk";
    public string happyTrig      = "Happy";
    [Tooltip("Must match the Trigger parameter name you add in the Animator Controller " +
             "for the angry animation (played when a customer leaves from a patience timeout " +
             "or a wrong answer).")]
    public string angryTrig      = "Angry";
    public string animSpeedParam = "AnimSpeed";
    [Tooltip("How long the happy animation plays before pig + meal disappear.")]
    public float happyDuration = 2f;
    [Tooltip("How long the angry animation plays before the customer disappears unserved. " +
             "IMPORTANT: this must be at least as long as your Angry animation clip's " +
             "actual length, or the customer will vanish mid-animation. Select the Angry " +
             "clip asset and check its length in the Inspector preview, then set this a " +
             "little longer (add ~0.3-0.5s buffer). This is a per-prefab value — bumping " +
             "the default here in code does NOT change it on Pig/Dog prefabs that were " +
             "already set up; update the Inspector field on both directly.")]
    public float angryDuration = 2f;

    [Header("UI (leave empty while testing)")]
    public ThoughtBubble thoughtBubble;
    [Tooltip("Face icon used by the single shared Patience Meter (and later the Order " +
             "Queue panel, if you add it) to represent this customer — e.g. a small Pig " +
             "or Dog portrait. Optional, but needed for those to show the right icon.")]
    public Sprite portraitIcon;

    [Header("Angry Shake")]
    [Tooltip("If true, the whole customer briefly jitters side to side the instant " +
             "they get angry (wrong answer or patience timeout).")]
    public bool enableAngryShake = true;
    public float shakeDuration   = 0.35f;
    public float shakeMagnitude  = 0.04f;

    [Header("Audio")]
    [Tooltip("Add an AudioSource component to this customer (uncheck 'Play On Awake') " +
             "and drag it here, or just leave this empty — Awake() will auto-grab an " +
             "AudioSource on this same GameObject if one exists.")]
    public AudioSource audioSource;
    [Tooltip("Short angry yell/grunt — plays once, the instant this customer gets " +
             "angry (wrong answer or patience timeout), at the same moment as the " +
             "Angry animation trigger and the shake.")]
    public AudioClip angryYellSound;
    [Tooltip("Short happy sound — plays once, the instant this customer gets served " +
             "(correct answer), at the same moment as the Happy animation trigger.")]
    public AudioClip happySound;
    [Tooltip("Playback speed for Happy Sound — 1 = normal, higher = faster/higher-pitched. " +
             "Per-prefab, so e.g. Dog's slow-sounding clip can be sped up here without " +
             "re-editing the audio file. Does not affect Angry Yell Sound, which always " +
             "plays at normal speed regardless of this value.")]
    public float happySoundSpeed = 1f;

    [Header("Patience")]
    [Tooltip("How long (seconds) this customer waits at the station before leaving " +
             "unserved. Set a DIFFERENT value per character prefab — e.g. Pig should " +
             "be the least patient (lower, ~12–15s), Dog the most patient (higher, " +
             "~30–35s). Timer starts the moment this customer actually reaches the " +
             "counter (State becomes AtStation), whether that's their first arrival " +
             "or after shuffling forward.")]
    public float patienceDuration = 25f;
    [Tooltip("When remaining patience drops to this fraction (0-1) of the total, the " +
             "thought bubble starts blinking to warn the player this customer is " +
             "running low. 0.35 = starts blinking with 35% of their patience left.")]
    [Range(0f, 1f)] public float lowPatienceThreshold = 0.35f;

    /// <summary>
    /// Remaining patience as a 0-1 fraction, updated every frame while the
    /// countdown is running (1 = just arrived, 0 = about to leave). Read by
    /// OrderQueueUI to drive its own mood icons without needing to know
    /// anything about how the countdown itself works.
    /// </summary>
    public float PatienceFraction { get; private set; } = 1f;

    // Fires when this customer physically arrives at the counter and their
    // thought bubble is shown — i.e. the moment they're actually "stationed."
    // CustomerManager listens for this to know when it's safe to treat this
    // customer as the active order (see SyncActiveRecipe in CustomerManager).
    public System.Action<KottuRecipe> onArrivedAtStation;

    // Fires when pig disappears so OrderManager can send the next customer
    public System.Action onLeave;

    public State CurrentState { get; private set; } = State.Hidden;

    private Coroutine   patienceCoroutine;
    private Coroutine   moveCoroutine;   // the ONE movement coroutine currently driving transform
    private KottuRecipe pendingRecipe;   // recipe this customer is walking toward — needed if ShuffleForward redirects them mid-walk

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void Arrive(KottuRecipe recipe)
    {
        ValidateUIWiring();

        StopAllCoroutines();
        moveCoroutine = null;
        thoughtBubble?.Hide();

        if (animator != null)
        {
            // This script drives position/rotation entirely by hand (WalkTo /
            // ShuffleWalk below). If the Animator's "Apply Root Motion" is
            // enabled, it fights that every frame. Force it off so the
            // Animator only ever plays clips in place and never touches
            // transform itself.
            animator.applyRootMotion = false;
            // Leftover "Happy"/"Angry" triggers from a previous life of this
            // pooled object shouldn't carry over into the new walk-in.
            animator.ResetTrigger(happyTrig);
            animator.ResetTrigger(angryTrig);
        }

        gameObject.SetActive(true);
        if (spawnPoint != null) transform.position = spawnPoint.position;
        CurrentState = State.Walking;
        pendingRecipe = recipe;

        Debug.Log($"🚶 {name}: Arrive() called for '{recipe?.displayName}' — walking from {transform.position:F2} to {stationPoint.position:F2}");

        moveCoroutine = StartCoroutine(WalkTo(stationPoint.position, HandleArrivedAtStation));
    }

    /// <summary>
    /// The actual "I've reached the counter" logic — shared by a normal
    /// Arrive() walk-in and by ShuffleForward when it has to redirect a
    /// customer who was still mid-walk-in (see ShuffleForward below).
    /// </summary>
    private void HandleArrivedAtStation()
    {
        CurrentState = State.AtStation;
        SetWalking(false);
        transform.rotation = Quaternion.Euler(0f, counterFacingYaw, 0f);
        thoughtBubble?.Show(pendingRecipe);
        StartPatienceTimer();
        onArrivedAtStation?.Invoke(pendingRecipe);
        Debug.Log($"✅ {name}: reached station, bubble shown, order active.");
    }

    /// <summary>
    /// Call this when the finished meal appears on the counter.
    /// Pig cheers, then pig + meal both disappear after happyDuration seconds.
    /// </summary>
    public void Serve(MealAppearEffect mealToHide)
    {
        // Only serve if actually at the station
        if (CurrentState != State.AtStation)
        {
            Debug.LogWarning("🐷 Serve() called but pig is not at station. State: " + CurrentState);
            return;
        }

        CurrentState = State.Served;
        StopPatienceTimer();
        thoughtBubble?.Hide();
        SetWalking(false);

        if (animator != null)
            animator.SetTrigger(happyTrig);
        else
            Debug.LogWarning("🐷 Animator is null on CustomerController!");

        if (audioSource != null && happySound != null)
        {
            audioSource.pitch = happySoundSpeed;
            audioSource.PlayOneShot(happySound);
        }

        Debug.Log("🐷 Pig is happy! Disappearing in " + happyDuration + "s");
        StartCoroutine(DisappearAfter(happyDuration, mealToHide));
    }

    /// <summary>
    /// Called by CustomerManager when the queue shuffles forward.
    /// </summary>
    public void ShuffleForward(Transform newSlot)
    {
        stationPoint = newSlot;

        // Always stop whatever movement coroutine is currently running before
        // starting a new one. THIS was the actual bug you hit: if a customer
        // gets shuffled while still walking in from their original spawn (an
        // earlier customer left before this one finished arriving), the old
        // code started a SECOND movement coroutine without stopping the
        // first — two coroutines fighting over transform.position toward two
        // different targets, forever. That's exactly what your Console log
        // showed: distance stuck at 0.24, not shrinking, 5+ seconds in.
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        if (CurrentState == State.Walking)
        {
            // Still on the original walk-in (hasn't reached AtStation yet) —
            // redirect toward the updated slot instead of losing the arrival
            // behaviour. If we switched to ShuffleWalk here instead, the
            // bubble/patience-timer/active-order logic in
            // HandleArrivedAtStation would simply never run for this
            // customer, since that logic only lives on WalkTo's callback.
            Debug.Log($"↪️ {name}: shuffled to a new slot mid-walk-in, redirecting.");
            moveCoroutine = StartCoroutine(WalkTo(newSlot.position, HandleArrivedAtStation));
        }
        else
        {
            // Already stationed — this is just a cosmetic reposition, don't
            // re-show the bubble or restart the patience timer.
            moveCoroutine = StartCoroutine(ShuffleWalk(newSlot.position));
        }
    }

    // ── Private ───────────────────────────────────────────────────────────

    /// <summary>
    /// Catches the exact bug that causes "the thought bubble shows up next to
    /// the wrong character": thoughtBubble dragged into the Inspector from a
    /// DIFFERENT customer's hierarchy instead of this one's own child.
    /// ThoughtBubble positions itself relative to its own transform.parent, so
    /// if this reference points at someone else's bubble, it will faithfully
    /// render at THEIR position, not yours — with no error, just a visually
    /// confusing result. This check makes that mistake loud and immediate
    /// instead of silent.
    /// </summary>
    private void ValidateUIWiring()
    {
        if (thoughtBubble != null && thoughtBubble.transform.parent != transform)
        {
            Debug.LogError(
                $"❌ {name}: 'Thought Bubble' is wired to '{thoughtBubble.name}' " +
                $"which is a child of '{thoughtBubble.transform.parent?.name}', not '{name}'. " +
                $"Expand {name} in the Hierarchy, drag ITS OWN ThoughtBubble child into the " +
                $"Thought Bubble field on this CustomerController, and fix the other " +
                $"customer's field the same way if it was pointing here instead.",
                this);
        }

        // Not an error — this field is intentionally optional while testing —
        // but a nudge here saves you from silently missing a customer's bubble
        // and having to spot it by eye in Play mode.
        if (thoughtBubble == null)
            Debug.LogWarning($"⚠️ {name}: 'Thought Bubble' is unassigned — no bubble will show for this customer.", this);
    }

    /// <summary>
    /// The actual game-affecting patience countdown, entirely independent of
    /// any visual meter — customers correctly leave when impatient with
    /// nothing more than this coroutine running.
    /// </summary>
    private void StartPatienceTimer()
    {
        StopPatienceTimer();

        // If a LevelTimer exists in the scene, its PatienceMultiplier (which
        // ramps down over the course of the level — see LevelTimer.cs) scales
        // this customer's actual countdown. No LevelTimer in the scene at all
        // just means a multiplier of 1 (unchanged behavior).
        float duration = patienceDuration;
        if (LevelTimer.Instance != null) duration *= LevelTimer.Instance.PatienceMultiplier;

        PatienceFraction = 1f;
        patienceCoroutine = StartCoroutine(PatienceCountdown(duration));
    }

    private void StopPatienceTimer()
    {
        if (patienceCoroutine != null)
        {
            StopCoroutine(patienceCoroutine);
            patienceCoroutine = null;
        }
    }

    /// <summary>
    /// Runs frame-by-frame (instead of a single WaitForSeconds) so it can
    /// continuously report PatienceFraction — the shared Patience Meter UI
    /// (see ActivePatienceUI.cs, driven from CustomerManager) polls this
    /// every frame for whichever customer is currently active, and the
    /// thought bubble's low-patience blink also keys off it, in addition to
    /// still being what makes the customer actually leave when it hits 0.
    /// </summary>
    private IEnumerator PatienceCountdown(float duration)
    {
        float elapsed = 0f;
        bool  urgentTriggered = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            PatienceFraction = duration > 0f ? Mathf.Clamp01(1f - elapsed / duration) : 0f;

            if (!urgentTriggered && PatienceFraction <= lowPatienceThreshold)
            {
                urgentTriggered = true;
                thoughtBubble?.SetUrgent(true);
            }
            yield return null;
        }

        PatienceFraction = 0f;
        OnPatienceOut();
    }

    private void OnPatienceOut()
    {
        if (CurrentState != State.AtStation) return;
        patienceCoroutine = null;
        Debug.Log("😠 " + name + ": ran out of patience!");
        TriggerAngryLeave();
    }

    /// <summary>
    /// Call this when the player drops the WRONG word for this customer's
    /// order. Plays the exact same angry/leave sequence as a patience
    /// timeout (Angry trigger, hide bubble, disappear after angryDuration,
    /// then fire onLeave so CustomerManager shuffles the queue forward and
    /// re-syncs the active recipe to whoever's next). Safe to call even if
    /// this customer isn't currently the active one — it no-ops unless
    /// they're actually AtStation.
    /// </summary>
    public void LeaveAngry()
    {
        if (CurrentState != State.AtStation) return;
        StopPatienceTimer(); // cancel the running patience countdown — they're leaving early, not from a timeout
        Debug.Log("😠 " + name + ": got the wrong answer!");
        TriggerAngryLeave();
    }

    /// <summary>
    /// Shared "customer gets angry and leaves unserved" sequence — used by
    /// both a patience timeout (OnPatienceOut) and a wrong word drop
    /// (LeaveAngry). Kept deliberately simple: animation + yell + shake.
    /// </summary>
    private void TriggerAngryLeave()
    {
        CurrentState = State.Served; // reuse Served to prevent double-trigger
        thoughtBubble?.Hide();

        if (animator != null)
            animator.SetTrigger(angryTrig);
        else
            Debug.LogWarning("🐷 Animator is null on CustomerController!");

        if (audioSource != null && angryYellSound != null)
        {
            // Reset pitch first — Happy Sound Speed above changes audioSource.pitch,
            // and pitch persists on the AudioSource until something else changes it.
            // Without this, a customer served with a sped-up happy sound and later
            // reused from the pool for an angry sequence would yell at that same
            // sped-up pitch instead of normal speed.
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(angryYellSound);
        }

        if (enableAngryShake) StartCoroutine(ShakeRoutine());

        Debug.Log("😠 Disappearing in " + angryDuration + "s");
        StartCoroutine(DisappearAfter(angryDuration, null));
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 originalLocalPos = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
            float offsetZ = Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.localPosition = originalLocalPos + new Vector3(offsetX, 0f, offsetZ);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalLocalPos;
    }

    // Walks to new slot without changing state — used for queue shuffle
    private IEnumerator ShuffleWalk(Vector3 target)
    {
        SetWalking(true);
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir.normalized), 12f * Time.deltaTime);
            transform.position = Vector3.MoveTowards(
                transform.position, target, walkSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        SetWalking(false);
        transform.rotation = Quaternion.Euler(0f, counterFacingYaw, 0f);
    }

    private IEnumerator WalkTo(Vector3 target, System.Action onArrived)
    {
        SetWalking(true);
        float elapsed = 0f;
        bool  warned  = false;
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir.normalized), 12f * Time.deltaTime);
            transform.position = Vector3.MoveTowards(
                transform.position, target, walkSpeed * Time.deltaTime);

            // Watchdog: a normal walk should take a couple of seconds at most.
            // If it's still going after 5s, something (most likely Animator
            // root motion fighting this manual movement) is preventing the
            // distance from ever closing. Log it once so it's obvious in the
            // Console instead of just looking like a silent hang.
            elapsed += Time.deltaTime;
            if (!warned && elapsed > 5f)
            {
                warned = true;
                Debug.LogWarning($"⚠️ {name}: WalkTo has been running for {elapsed:F1}s without reaching target. " +
                    $"pos={transform.position:F2} target={target:F2} dist={Vector3.Distance(transform.position, target):F2}. " +
                    $"If this keeps climbing instead of shrinking, Animator root motion (or something else) is fighting the manual movement.");
            }
            yield return null;
        }
        transform.position = target;
        onArrived?.Invoke();
    }

    private IEnumerator DisappearAfter(float delay, MealAppearEffect meal)
    {
        yield return new WaitForSeconds(delay);

        // Hide the meal at the same time as the pig
        if (meal != null) meal.Hide();

        onLeave?.Invoke();
        onLeave = null;
        gameObject.SetActive(false);
        CurrentState = State.Hidden;
        Debug.Log("🐷 Pig (and meal) disappeared.");
    }

    private void SetWalking(bool walking)
    {
        if (animator == null) return;
        animator.SetBool(walkBool, walking);
        animator.SetFloat(animSpeedParam, walking ? walkAnimSpeed : 1f);
    }
}