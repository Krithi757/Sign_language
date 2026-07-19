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
    [Tooltip("Optional — wire this up when you build the score UI. Safe to leave empty for now.")]
    public TextMeshProUGUI        scoreCounterLabel;

    [Header("Audio")]
    [Tooltip("Add an AudioSource component to this GameObject (uncheck 'Play On Awake') and drag it here.")]
    public AudioSource uiAudioSource;
    [Tooltip("Plays once, right when the coin popup appears.")]
    public AudioClip   coinSound;

    [Header("Customer System")]
    [Tooltip("Drag the CustomerManager GameObject here.")]
    public CustomerManager customerManager;

    [Header("Debug — read only")]
    public int    totalCoins        = 0;
    public int    totalScore        = 0;
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
        UpdateScoreLabel();
        // CustomerManager handles spawning and calls SetCurrentRecipe when
        // a customer is actually stationed at the counter.
    }

    // ── Called by CustomerManager whenever the active (front-of-queue,
    //    fully-stationed) customer changes — including to null when nobody
    //    currently qualifies (queue empty, or front customer still walking in,
    //    or the previous active customer just left from a patience timeout) ──
    public void SetCurrentRecipe(KottuRecipe recipe)
    {
        // Guard against redundant re-syncs setting the SAME recipe again —
        // CustomerManager calls this from a few different places (spawn
        // arrival, serve, queue shuffle) and can legitimately land on the
        // same recipe more than once in a row. Without this guard,
        // orderStartTime would keep resetting and quietly extend/break the
        // speed-bonus window every time that happens.
        if (recipe == currentRecipe) return;

        bool becameActive = currentRecipe == null && recipe != null;
        bool becameIdle    = currentRecipe != null && recipe == null;

        currentRecipe      = recipe;
        currentRecipeName  = recipe != null ? recipe.displayName : "(none)";
        orderStartTime     = Time.time;

        // ── Video freeze/blank hook ─────────────────────────────────────────
        // Requirement: the sign-language video should stay frozen/blank until
        // a customer is actually stationed at the counter, then play; and it
        // should go back to frozen/blank whenever there's no stationed
        // customer to serve (empty queue, or the front customer only just
        // started walking in). This is the single place that transition
        // happens, but I don't have WordRoundManager/VideoController's actual
        // API yet, so this is left as a clearly-marked TODO rather than a
        // guess that might not compile. Share that script and I'll wire the
        // real calls in on the next pass. Likely shape:
        //
        //   if (becameActive) wordRoundManager.videoController.Play();   // unfreeze
        //   if (becameIdle)   wordRoundManager.videoController.Pause();  // freeze/blank
        //
        if (becameActive) Debug.Log("▶️ TODO: unfreeze/play video — customer now stationed.");
        if (becameIdle)   Debug.Log("⏸️ TODO: freeze/blank video — no stationed customer.");

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
        // Wrong answer doesn't fail the order or touch currentRecipe/orderStartTime —
        // the player just tries again with a new video for the same active order.
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

        // Score — one point per completed order. Tune the value/formula once
        // you know how you want scoring to feel (e.g. weight by speed too).
        totalScore += 1;

        string popup = recipe.emoji + " " + recipe.displayName + "!\n+" + earned + " coins";
        if (wasFast) popup += " ⚡ Speed bonus!";
        if (coinPopup != null) coinPopup.Show(popup);
        if (uiAudioSource != null && coinSound != null) uiAudioSource.PlayOneShot(coinSound);
        UpdateCoinLabel();
        UpdateScoreLabel();

        // CustomerManager serves the front customer and pushes the next recipe to us
        if (customerManager != null)
            customerManager.ServeCurrentCustomer(recipe.mealObject);

        // Advance to next sign language video
        wordRoundManager.videoController.NextVideo();

        Debug.Log($"💰 +{earned} coins | Total: {totalCoins} | Score: {totalScore}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateCoinLabel()
    {
        // No emoji here on purpose — LiberationSans SDF (TMP's default font)
        // has no emoji glyphs, so 💰/⭐ render as blank "missing glyph" boxes
        // (□) instead of the actual icon. You've already got a real coin
        // icon graphic next to this label doing that job visually, so the
        // text itself only needs the number.
        if (coinCounterLabel != null)
            coinCounterLabel.text = totalCoins.ToString();
    }

    private void UpdateScoreLabel()
    {
        if (scoreCounterLabel != null)
            scoreCounterLabel.text = totalScore.ToString();
    }

    public KottuRecipe CurrentRecipe => currentRecipe;
}