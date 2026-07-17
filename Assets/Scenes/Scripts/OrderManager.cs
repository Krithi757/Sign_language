using UnityEngine;
using TMPro;

// Place on an empty GameObject called "OrderManager".
// CustomerManager now controls spawning and recipe assignment.
public class OrderManager : MonoBehaviour
{
    [Header("All Kottu Recipes")]
    public KottuRecipe[] recipes;

    [Header("References")]
    public FoxAnimationController foxController;
    public WordRoundManager       wordRoundManager;
    public CoinPopup              coinPopup;
    public TextMeshProUGUI        coinCounterLabel;

    [Header("Customer System")]
    [Tooltip("Drag the CustomerManager GameObject here.")]
    public CustomerManager customerManager;

    [Header("Debug — read only")]
    public int    totalCoins        = 0;
    public string currentRecipeName = "";

    // Set by CustomerManager whenever the active customer changes
    private KottuRecipe currentRecipe;
    private float       orderStartTime;

    // ══════════════════════════════════════════════════════════════════════════

    void Start()
    {
        if (recipes == null || recipes.Length == 0)
            Debug.LogError("❌ OrderManager: No recipes assigned!");

        UpdateCoinLabel();
        // CustomerManager handles spawning and calls SetCurrentRecipe when first customer arrives
    }

    // ── Called by CustomerManager when the active customer changes ────────────
    public void SetCurrentRecipe(KottuRecipe recipe)
    {
        currentRecipe    = recipe;
        currentRecipeName = recipe != null ? recipe.displayName : "(none)";
        orderStartTime   = Time.time;
        Debug.Log("📋 Active order: " + currentRecipeName);
    }

    // ── Called by VideoDropTarget on correct word drop ────────────────────────
    public void OnCorrectAnswer()
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning("⚠️ Correct answer but no active recipe — waiting for a customer.");
            return;
        }
        if (foxController == null) { Debug.LogError("❌ Fox not assigned!"); return; }

        Debug.Log("✅ Correct! Cooking: " + currentRecipe.displayName);

        KottuRecipe recipe    = currentRecipe;
        float       startTime = orderStartTime;

        foxController.PlayCorrectSequence(
            uniqueVFX:   recipe.uniqueVFX,
            effectDelay: recipe.uniqueEffectDelay,
            mealToShow:  recipe.mealObject,
            onComplete:  () => OnOrderComplete(recipe, startTime)
        );
    }

    // ── Called by VideoDropTarget on wrong word drop ──────────────────────────
    public void OnWrongAnswer()
    {
        if (foxController == null) return;
        foxController.PlayWrongSequence(onComplete: () =>
            wordRoundManager.videoController.NextVideo());
    }

    // ── Fires when the fox finishes the full cooking sequence ─────────────────
    private void OnOrderComplete(KottuRecipe recipe, float startTime)
    {
        // Coins
        float timeTaken = Time.time - startTime;
        bool  wasFast   = timeTaken <= recipe.speedBonusSeconds;
        int   earned    = recipe.coinReward + (wasFast ? recipe.speedBonusCoins : 0);
        totalCoins += earned;

        string popup = recipe.emoji + " " + recipe.displayName + "!\n+" + earned + " coins";
        if (wasFast) popup += " ⚡ Speed bonus!";
        if (coinPopup != null) coinPopup.Show(popup);
        UpdateCoinLabel();

        // CustomerManager serves the front customer and pushes the next recipe to us
        if (customerManager != null)
            customerManager.ServeCurrentCustomer(recipe.mealObject);

        // Advance to next sign language video
        wordRoundManager.videoController.NextVideo();

        Debug.Log($"💰 +{earned} coins | Total: {totalCoins}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateCoinLabel()
    {
        if (coinCounterLabel != null)
            coinCounterLabel.text = "💰 " + totalCoins;
    }

    public KottuRecipe CurrentRecipe => currentRecipe;
}
