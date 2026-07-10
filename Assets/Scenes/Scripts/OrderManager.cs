using UnityEngine;
using TMPro;

// The brain of the restaurant system.
// Place this on an empty GameObject in the scene called "OrderManager".
// Wire everything up in the Inspector.
public class OrderManager : MonoBehaviour
{
    [Header("All Kottu Recipes — add 5 entries here")]
    public KottuRecipe[] recipes;

    [Header("References")]
    [Tooltip("Drag the Fox GameObject here.")]
    public FoxAnimationController foxController;

    [Tooltip("Drag the WordRoundManager here.")]
    public WordRoundManager wordRoundManager;

    [Tooltip("Drag the CoinPopup TextMeshProUGUI here.")]
    public CoinPopup coinPopup;

    [Tooltip("Optional: a TextMeshProUGUI that always shows total coins on screen.")]
    public TextMeshProUGUI coinCounterLabel;

    [Header("State — read-only, visible for debugging")]
    public int totalCoins = 0;
    public int currentRecipeIndex = 0;
    public string currentRecipeName = "";

    private KottuRecipe currentRecipe;
    private float orderStartTime;

    void Start()
    {
        if (recipes == null || recipes.Length == 0)
        {
            Debug.LogError("❌ OrderManager: No recipes assigned! Add them in the Inspector.");
            return;
        }
        PickRecipe(0);
        UpdateCoinLabel();
    }

    // ── Called by VideoDropTarget when the correct word is dropped ─────────
    public void OnCorrectAnswer()
    {
        if (currentRecipe == null) { Debug.LogError("❌ OrderManager: No current recipe!"); return; }
        if (foxController == null) { Debug.LogError("❌ OrderManager: Fox Controller not assigned!"); return; }

        Debug.Log("✅ Correct! Cooking: " + currentRecipe.displayName);

        // Capture these NOW before the coroutine runs (avoids closure issues)
        KottuRecipe recipe = currentRecipe;
        float startTime = orderStartTime;

        foxController.PlayCorrectSequence(
            uniqueEndVFX:  recipe.uniqueEndVFX,
            mealToShow:    recipe.mealObject,
            onComplete:    () => OnOrderComplete(recipe, startTime)
        );
    }

    // ── Called by VideoDropTarget when the wrong word is dropped ───────────
    public void OnWrongAnswer()
    {
        if (foxController == null) return;

        // Wrong answer — fox plays sad animation, then video advances. No coins, no meal.
        foxController.PlayWrongSequence(onComplete: () =>
        {
            wordRoundManager.videoController.NextVideo();
        });
    }

    // ── Fired after the fox finishes the full cooking sequence ─────────────
    private void OnOrderComplete(KottuRecipe recipe, float startTime)
    {
        float timeTaken = Time.time - startTime;
        bool wasFast = timeTaken <= recipe.speedBonusSeconds;

        int earned = recipe.coinReward + (wasFast ? recipe.speedBonusCoins : 0);
        totalCoins += earned;

        string popup = recipe.emoji + " " + recipe.displayName + "!\n+" + earned + " coins";
        if (wasFast) popup += " ⚡ Speed bonus!";

        if (coinPopup != null) coinPopup.Show(popup);
        UpdateCoinLabel();

        Debug.Log("💰 Earned " + earned + " coins. Total: " + totalCoins);

        // Move to the next recipe and the next video
        int next = (currentRecipeIndex + 1) % recipes.Length;
        PickRecipe(next);
        wordRoundManager.videoController.NextVideo();
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private void PickRecipe(int index)
    {
        currentRecipeIndex = index;
        currentRecipe = recipes[index];
        currentRecipeName = currentRecipe.displayName;
        orderStartTime = Time.time;
        Debug.Log("📋 Next order: " + currentRecipe.displayName + " (" + currentRecipe.coinReward + " coins)");
    }

    private void UpdateCoinLabel()
    {
        if (coinCounterLabel != null)
            coinCounterLabel.text = "💰 " + totalCoins;
    }

    public KottuRecipe CurrentRecipe => currentRecipe;
}
