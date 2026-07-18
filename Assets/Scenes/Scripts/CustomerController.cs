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
    public string animSpeedParam = "AnimSpeed";
    [Tooltip("How long the happy animation plays before pig + meal disappear.")]
    public float happyDuration = 2f;

    [Header("UI (leave empty while testing)")]
    public ThoughtBubble thoughtBubble;
    public PatienceStars patienceStars;

    [Header("Patience")]
    public float patienceDuration = 18f;

    // Fires when pig disappears so OrderManager can send the next customer
    public System.Action onLeave;

    public State CurrentState { get; private set; } = State.Hidden;

    // ── Public API ────────────────────────────────────────────────────────

    public void Arrive(KottuRecipe recipe)
    {
        ValidateUIWiring();

        StopAllCoroutines();
        thoughtBubble?.Hide();
        patienceStars?.Stop();

        gameObject.SetActive(true);
        if (spawnPoint != null) transform.position = spawnPoint.position;
        CurrentState = State.Walking;

        StartCoroutine(WalkTo(stationPoint.position, () =>
        {
            CurrentState = State.AtStation;
            SetWalking(false);
            transform.rotation = Quaternion.Euler(0f, counterFacingYaw, 0f);
            thoughtBubble?.Show(recipe);
            patienceStars?.StartCounting(patienceDuration, OnPatienceOut);
        }));
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
        patienceStars?.Stop();
        thoughtBubble?.Hide();
        SetWalking(false);

        if (animator != null)
            animator.SetTrigger(happyTrig);
        else
            Debug.LogWarning("🐷 Animator is null on CustomerController!");

        Debug.Log("🐷 Pig is happy! Disappearing in " + happyDuration + "s");
        StartCoroutine(DisappearAfter(happyDuration, mealToHide));
    }

    /// <summary>
    /// Called by CustomerManager when the queue shuffles forward.
    /// Customer walks to the new slot without resetting state or UI.
    /// </summary>
    public void ShuffleForward(Transform newSlot)
    {
        stationPoint = newSlot;
        StartCoroutine(ShuffleWalk(newSlot.position));
    }

    // ── Private ───────────────────────────────────────────────────────────

    /// <summary>
    /// Catches the exact bug that causes "both thought bubbles show up next
    /// to the wrong character": thoughtBubble / patienceStars dragged into
    /// the Inspector from a DIFFERENT customer's hierarchy instead of this
    /// one's own child. Both scripts position themselves relative to their
    /// own transform.parent, so if this reference points at someone else's
    /// bubble, it will faithfully render at THEIR position, not yours — with
    /// no error, just a visually confusing result. This check makes that
    /// mistake loud and immediate instead of silent.
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

        if (patienceStars != null && patienceStars.transform.parent != transform)
        {
            Debug.LogError(
                $"❌ {name}: 'Patience Stars' is wired to '{patienceStars.name}' " +
                $"which is a child of '{patienceStars.transform.parent?.name}', not '{name}'. " +
                $"Expand {name} in the Hierarchy and drag ITS OWN PatienceStars child into the " +
                $"Patience Stars field on this CustomerController.",
                this);
        }

        // Not an error — these fields are intentionally optional while testing —
        // but a nudge here saves you from silently missing a customer's meter
        // and having to spot it by eye in Play mode.
        if (thoughtBubble == null)
            Debug.LogWarning($"⚠️ {name}: 'Thought Bubble' is unassigned — no bubble will show for this customer.", this);
        if (patienceStars == null)
            Debug.LogWarning($"⚠️ {name}: 'Patience Stars' is unassigned — no patience meter will show for this customer.", this);
    }

    private void OnPatienceOut()
    {
        if (CurrentState != State.AtStation) return;
        CurrentState = State.Served; // reuse Served to prevent double-trigger
        thoughtBubble?.Hide();
        patienceStars?.Stop();
        StartCoroutine(DisappearAfter(0.3f, null));
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