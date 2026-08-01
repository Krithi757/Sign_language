using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Place on an empty GameObject called "CustomerManager".
// Drag Pig and Dog GameObjects (disabled in scene) into customerPool.
// Create empty GameObjects at counter positions and drag into counterSlots.
public class CustomerManager : MonoBehaviour
{
    [Header("Customer pool — drag Pig, Dog, etc. here (must be DISABLED in scene)")]
    public CustomerController[] customerPool;

    [Header("Counter slots — left to right positions at the counter")]
    [Tooltip("Slot 0 = active order position. Slot 1, 2 = waiting positions behind/beside.")]
    public Transform[] counterSlots;

    [Header("Shared off-screen spawn point")]
    public Transform spawnPoint;

    [Header("Wiring")]
    public OrderManager orderManager;
    [Tooltip("Optional — the 2D 'Order Queue' side panel. A row is added for a customer " +
             "the instant they reach the counter (never while still walking in), and " +
             "removed the instant they leave (served or angry). Leave empty to skip.")]
    public OrderQueueUI orderQueueUI;
    [Tooltip("ONE shared Patience Meter for the whole scene. When multiple customers are " +
             "stationed at once, it shows their portraits side-by-side (auto-centered via " +
             "a Horizontal Layout Group on the icon slots) but the bar/fill/color/blink/" +
             "fire escalation only ever track the front-most (queue[0]) customer. The " +
             "background customer's own countdown still runs for real underneath — it's " +
             "just not separately visualized. To keep that from feeling unfair, give Dog " +
             "(or whichever character tends to end up waiting) a much higher " +
             "patienceDuration than Pig.")]
    public ActivePatienceUI patienceMeter;

    [Header("Spawn timing")]
    [Tooltip("Base random range for how long between customer spawns. If a LevelTimer " +
             "exists in the scene, its SpawnIntervalMultiplier scales this each time — " +
             "starts slower (bigger multiplier), ramps down to faster later in the level.")]
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 12f;

    // ── Internal ──────────────────────────────────────────────────────────────

    private class QueueEntry
    {
        public CustomerController customer;
        public KottuRecipe        recipe;
    }

    private List<QueueEntry> queue = new List<QueueEntry>();
    private int recipeAssignIndex  = 0;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Force all pool customers to start disabled regardless of scene setup
        foreach (var c in customerPool)
            if (c != null) c.gameObject.SetActive(false);
    }

    void Start()
    {
        // No active order yet — makes sure the video starts frozen/blank
        // rather than whatever OrderManager's default state happens to be.
        SyncActiveRecipe();
        StartCoroutine(SpawnLoop());
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    private IEnumerator SpawnLoop()
    {
        // First customer walks in almost immediately
        yield return new WaitForSeconds(0.5f);
        TrySpawn();

        // Second customer walks in 1-2 seconds later
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        if (queue.Count < counterSlots.Length)
            TrySpawn();

        // After that, new customers arrive at random intervals — scaled by
        // LevelTimer's SpawnIntervalMultiplier if one exists in the scene, so
        // spawns start slower and ramp up to faster as the level goes on. No
        // LevelTimer at all just means a multiplier of 1 (unchanged pacing).
        while (true)
        {
            float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
            if (LevelTimer.Instance != null) interval *= LevelTimer.Instance.SpawnIntervalMultiplier;
            yield return new WaitForSeconds(interval);
            if (queue.Count < counterSlots.Length)
                TrySpawn();
        }
    }

    private void TrySpawn()
    {
        // Pick a random customer that is currently inactive
        CustomerController c = customerPool
            .Where(x => !x.gameObject.activeSelf)
            .OrderBy(_ => Random.value)
            .FirstOrDefault();

        if (c == null) { Debug.Log("⚠️ No inactive customers available."); return; }

        // Assign next recipe in the cycle
        KottuRecipe recipe = orderManager.recipes[recipeAssignIndex % orderManager.recipes.Length];
        recipeAssignIndex++;

        var entry = new QueueEntry { customer = c, recipe = recipe };

        c.spawnPoint         = spawnPoint;
        c.stationPoint       = counterSlots[queue.Count];
        c.onLeave            = () => OnCustomerLeft(entry);
        // Don't push the recipe to OrderManager here — that would start the
        // video while this customer is still walking in from off-screen.
        // Instead, wait until they've actually reached the counter and their
        // bubble is showing (CustomerController fires onArrivedAtStation the
        // moment State becomes AtStation), then re-sync. This keeps the video
        // frozen/blank until a customer is genuinely stationed, and only acts
        // on it if that customer is the one actually at the front of the queue.
        // Same timing is used for the 2D Order Queue panel — a row only
        // appears once the customer has actually reached the counter, never
        // while they're still walking in from off-screen.
        c.onArrivedAtStation = (_) =>
        {
            SyncActiveRecipe();
            if (orderQueueUI != null)
                orderQueueUI.AddEntry(
                    c,
                    c.portraitIcon,
                    entry.recipe != null ? entry.recipe.orderSprite : null,
                    entry.recipe != null ? entry.recipe.addOnSprite : null);
        };

        queue.Add(entry);
        c.Arrive(recipe);

        Debug.Log($"🐾 {c.name} → {recipe.displayName} (slot {queue.Count - 1})");
    }

    // ── Serve ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by OrderManager when cooking finishes.
    /// Serves whoever is at the front of the queue.
    /// </summary>
    public void ServeCurrentCustomer(MealAppearEffect meal)
    {
        if (queue.Count == 0)
        {
            Debug.LogWarning("⚠️ ServeCurrentCustomer: nobody in queue!");
            return;
        }

        // Serve the front customer
        queue[0].customer.Serve(meal);

        // Immediately push the NEXT customer's recipe to OrderManager — don't
        // wait for the leave/disappear animation, the video should advance
        // right away. But only if that next customer is actually stationed
        // (bubble showing) already; if they're still mid-walk (e.g. they were
        // *just* spawned a moment ago), freeze/blank instead and let their own
        // onArrivedAtStation callback pick it up the instant they arrive.
        if (queue.Count > 1 && queue[1].customer.CurrentState == CustomerController.State.AtStation)
            orderManager.SetCurrentRecipe(queue[1].recipe);
        else
            orderManager.SetCurrentRecipe(null);
    }

    /// <summary>
    /// Called by OrderManager the instant the player drops the WRONG word.
    /// The front-of-queue (active) customer gets angry and leaves unserved —
    /// their own LeaveAngry() plays the Angry animation, waits angryDuration,
    /// then fires onLeave, which routes back into OnCustomerLeft below to
    /// shuffle the queue forward and re-sync the active recipe automatically.
    /// </summary>
    public void CurrentCustomerGotWrongAnswer()
    {
        if (queue.Count == 0)
        {
            Debug.LogWarning("⚠️ CurrentCustomerGotWrongAnswer: nobody in queue!");
            return;
        }
        queue[0].customer.LeaveAngry();
    }

    // ── Queue management ──────────────────────────────────────────────────────

    private void OnCustomerLeft(QueueEntry entry)
    {
        queue.Remove(entry);
        if (orderQueueUI != null) orderQueueUI.RemoveEntry(entry.customer);

        // Shuffle remaining customers one slot forward
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].customer.stationPoint != counterSlots[i])
                queue[i].customer.ShuffleForward(counterSlots[i]);
        }

        // Re-sync in case the customer who just left was the active order
        // (this is what happens when a customer leaves from a patience
        // timeout rather than being served) — without this, OrderManager
        // would keep pointing at the departed customer's recipe, and the
        // next correct answer would serve the wrong meal to whoever is now
        // actually at the front.
        SyncActiveRecipe();

        Debug.Log($"Queue after leave: {queue.Count} customer(s).");
    }

    /// <summary>
    /// Makes OrderManager's active recipe match reality: whoever is actually
    /// at the front of the queue AND fully stationed (bubble shown). If
    /// nobody qualifies right now — queue empty, or the front customer is
    /// still walking in — the active recipe is cleared so the video can
    /// freeze/blank instead of showing a stale or premature order.
    /// </summary>
    private void SyncActiveRecipe()
    {
        bool active = queue.Count > 0 && queue[0].customer.CurrentState == CustomerController.State.AtStation;

        if (orderManager != null)
            orderManager.SetCurrentRecipe(active ? queue[0].recipe : null);

        SyncPatienceMeters();
    }

    /// <summary>
    /// Builds the ordered (front-to-back) list of every currently-stationed
    /// customer and hands it to the single shared Patience Meter in one call.
    /// The meter itself decides how to display it: one icon per stationed
    /// customer side-by-side, bar/fire/blink tracking only the front one.
    /// </summary>
    private void SyncPatienceMeters()
    {
        if (patienceMeter == null) return;

        var stationed = new List<CustomerController>();
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].customer.CurrentState == CustomerController.State.AtStation)
                stationed.Add(queue[i].customer);
        }

        patienceMeter.SetStationedCustomers(stationed);
    }
}