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

    [Header("Spawn timing")]
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

        // After that, new customers arrive at random intervals
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
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

        c.spawnPoint    = spawnPoint;
        c.stationPoint  = counterSlots[queue.Count];
        c.onLeave       = () => OnCustomerLeft(entry);

        queue.Add(entry);
        c.Arrive(recipe);

        // If this is the first customer, tell OrderManager their recipe is now active
        if (queue.Count == 1)
            orderManager.SetCurrentRecipe(recipe);

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

        // Immediately push the NEXT customer's recipe to OrderManager
        // (don't wait for the animation — the video advances right away)
        KottuRecipe nextRecipe = queue.Count > 1 ? queue[1].recipe : null;
        orderManager.SetCurrentRecipe(nextRecipe);
    }

    // ── Queue management ──────────────────────────────────────────────────────

    private void OnCustomerLeft(QueueEntry entry)
    {
        queue.Remove(entry);

        // Shuffle remaining customers one slot forward
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].customer.stationPoint != counterSlots[i])
                queue[i].customer.ShuffleForward(counterSlots[i]);
        }

        Debug.Log($"Queue after leave: {queue.Count} customer(s).");
    }
}
