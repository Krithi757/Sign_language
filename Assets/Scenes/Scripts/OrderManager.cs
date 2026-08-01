using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Place on an empty GameObject called "OrderManager".
// CustomerManager now controls spawning and recipe assignment.
public class OrderManager : MonoBehaviour
{
    [Header("All Kottu Recipes")]
    public KottuRecipe[] recipes;

    [Header("References")]
    public FoxAnimationController foxController;
    public WordRoundManager wordRoundManager;
    public CoinPopup coinPopup;
    public TextMeshProUGUI coinCounterLabel;
    [Tooltip("Optional — wire this up when you build the score UI. Safe to leave empty for now.")]
    public TextMeshProUGUI scoreCounterLabel;

    [Header("Audio")]
    [Tooltip("Add an AudioSource component to this GameObject (uncheck 'Play On Awake') and drag it here.")]
    public AudioSource uiAudioSource;
    [Tooltip("Plays once, right when the coin popup appears.")]
    public AudioClip coinSound;
    [Tooltip("Plays once, the instant the player drops the WRONG word onto the video.")]
    public AudioClip wrongSound;

    [Header("Customer System")]
    [Tooltip("Drag the CustomerManager GameObject here.")]
    public CustomerManager customerManager;

    [Header("Play Gate (optional)")]
    [Tooltip("Optional — gates the video + word buttons behind an explicit Play tap " +
             "instead of the video auto-starting the instant a customer is stationed. " +
             "Leave empty to keep the old auto-play behavior.")]
    public VideoPlayGate videoPlayGate;

    [Header("Level End -> ChallengeFeedback")]
    [Tooltip("No diamond-earning mechanic exists in this game yet — this stays 0 " +
             "until you add one.")]
    public int diamondsEarned = 0;
    [Tooltip("Scene build index for ChallengeFeedback.")]
    public int challengeFeedbackSceneIndex = 5;

    [Header("Debug — read only")]
    public int totalCoins = 0;
    public int totalScore = 0;
    public string currentRecipeName = "";

    // Set by CustomerManager whenever the active customer changes
    private KottuRecipe currentRecipe;
    private float orderStartTime;

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

    /// <summary>
    /// Saves this run's Coins/Score/Diamonds/IsCompleted to PlayerPrefs and loads
    /// ChallengeFeedback — called by PauseMenu's Give Up button (completed = false).
    /// Whatever was earned so far still gets recorded and shown, nothing is lost.
    /// </summary>
    public void EndLevelAndGoToFeedback(bool completed)
    {
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.SetInt("Score", totalScore);
        PlayerPrefs.SetInt("Diamonds", diamondsEarned);
        PlayerPrefs.SetInt("IsCompleted", completed ? 1 : 0);
        PlayerPrefs.Save();

        Time.timeScale = 1f; // in case we're leaving from a paused state
        Debug.Log("🎬 OrderManager.EndLevelAndGoToFeedback -> loading scene index: " + challengeFeedbackSceneIndex);
        SceneManager.LoadScene(challengeFeedbackSceneIndex);
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
        bool becameIdle = currentRecipe != null && recipe == null;

        currentRecipe = recipe;
        currentRecipeName = recipe != null ? recipe.displayName : "(none)";
        orderStartTime = Time.time;

        // ── Video freeze/blank hook ─────────────────────────────────────────
        // The sign-language video stays frozen/blank until a customer is
        // actually stationed at the counter, then plays; and goes back to
        // frozen/blank whenever there's no stationed customer (empty queue,
        // or the front customer only just started walking in). This is the
        // single place that transition happens — see
        // TapToChangeVideo.ResumeVideo()/PauseVideo() for the actual
        // VideoPlayer.Play()/Pause() calls (that script also starts frozen
        // by default on scene load, so the very first customer's arrival is
        // covered too, not just subsequent ones).
        // If a VideoPlayGate is wired up, it owns the actual Resume/Pause calls —
        // SetPlaying()/SetPaused() are pure automatic state mirrors (Play icon +
        // greyed words track whether the video is paused or playing, no click
        // required — see VideoPlayGate.cs). Without one wired up, falls back to
        // the old direct auto-play behavior.
        if (becameActive)
        {
            if (videoPlayGate != null) videoPlayGate.SetPlaying();
            else wordRoundManager.videoController.ResumeVideo();
        }
        if (becameIdle)
        {
            if (videoPlayGate != null) videoPlayGate.SetPaused();
            else wordRoundManager.videoController.PauseVideo();
        }

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

        KottuRecipe recipe = currentRecipe;
        float startTime = orderStartTime;

        foxController.PlayCorrectSequence(
            uniqueVFX: recipe.uniqueVFX,
            effectDelay: recipe.uniqueEffectDelay,
            mealToShow: recipe.mealObject,
            onComplete: () => OnOrderComplete(recipe, startTime)
        );
    }

    // ── Called by VideoDropTarget on wrong word drop ──────────────────────────
    public void OnWrongAnswer()
    {
        Debug.Log("❌ Wrong word — customer is getting angry and leaving.");

        // Error sound plays immediately, same instant the wrong drop happens.
        if (uiAudioSource != null && wrongSound != null) uiAudioSource.PlayOneShot(wrongSound);

        // The active (front-of-queue) customer gets angry and leaves unserved
        // right away — their own Angry animation + disappear timing is driven
        // by CustomerController.LeaveAngry(). Once they actually disappear,
        // CustomerManager shuffles the queue forward and re-syncs the active
        // recipe on its own (same path as a patience timeout), so
        // currentRecipe will correctly move on to whoever's next (or go idle
        // if the queue's now empty) without anything more needed here.
        if (customerManager != null) customerManager.CurrentCustomerGotWrongAnswer();

        if (foxController == null) return;
        // Fox still plays its "defeated" animation in parallel, then the
        // sign-language video advances so the player gets a fresh word.
        foxController.PlayWrongSequence(onComplete: () =>
            wordRoundManager.videoController.NextVideo());
    }

    // ── Fires when the fox finishes the full cooking sequence ─────────────────
    private void OnOrderComplete(KottuRecipe recipe, float startTime)
    {
        // Coins
        float timeTaken = Time.time - startTime;
        bool wasFast = timeTaken <= recipe.speedBonusSeconds;
        int earned = recipe.coinReward + (wasFast ? recipe.speedBonusCoins : 0);
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