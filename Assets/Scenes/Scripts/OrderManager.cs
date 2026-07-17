using System.Collections;
using UnityEngine;
using TMPro;

public class OrderManager : MonoBehaviour
{
    [Header("Recipes")]
    public KottuRecipe[] recipes;

    [Header("References")]
    public FoxAnimationController foxController;
    public WordRoundManager wordRoundManager;
    public CoinPopup coinPopup;
    public TextMeshProUGUI coinCounterLabel;

    [Header("Customer")]
    public CustomerController customer;
    [Tooltip("Seconds after pig disappears before next pig walks in.")]
    public float customerArriveDelay = 1f;

    [Header("Debug — read only")]
    public int    totalCoins         = 0;
    public int    currentRecipeIndex = 0;
    public string currentRecipeName  = "";

    private KottuRecipe currentRecipe;
    private float       orderStartTime;

    void Start()
    {
        if (recipes == null || recipes.Length == 0)
        {
            Debug.LogError("❌ OrderManager: No recipes assigned!");
            return;
        }
        PickRecipe(0);
        UpdateCoinLabel();
        SpawnCustomer();
    }

    public void OnCorrectAnswer()
    {
        if (currentRecipe == null) { Debug.LogError("❌ No current recipe!"); return; }
        if (foxController  == null) { Debug.LogError("❌ Fox not assigned!"); return; }

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

    public void OnWrongAnswer()
    {
        if (foxController == null) return;
        foxController.PlayWrongSequence(onComplete: () =>
            wordRoundManager.videoController.NextVideo());
    }

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

        // Meal is now on the counter → trigger pig happy.
        // Pig will hide the meal when it disappears.
        if (customer != null)
        {
            customer.onLeave = () => StartCoroutine(DelayedSpawn(customerArriveDelay));
            customer.Serve(recipe.mealObject);  // <-- passes the meal so pig hides it
        }
        else
        {
            // No customer assigned — just auto-hide the meal after 3 seconds
            if (recipe.mealObject != null)
                recipe.mealObject.autoHideDelay = 3f;
        }

        // Move to next recipe and next video immediately
        int next = (currentRecipeIndex + 1) % recipes.Length;
        PickRecipe(next);
        wordRoundManager.videoController.NextVideo();
    }

    private IEnumerator DelayedSpawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (customer == null || currentRecipe == null) return;
        customer.Arrive(currentRecipe);
        Debug.Log("🐷 Customer arriving for: " + currentRecipe.displayName);
    }

    private void PickRecipe(int index)
    {
        currentRecipeIndex = index;
        currentRecipe      = recipes[index];
        currentRecipeName  = currentRecipe.displayName;
        orderStartTime     = Time.time;
        Debug.Log("📋 Order: " + currentRecipe.displayName);
    }

    private void UpdateCoinLabel()
    {
        if (coinCounterLabel != null)
            coinCounterLabel.text = "💰 " + totalCoins;
    }

    public KottuRecipe CurrentRecipe => currentRecipe;
}
