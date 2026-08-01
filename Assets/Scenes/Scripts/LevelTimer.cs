using UnityEngine;
using TMPro;

// Place on an empty GameObject called "LevelTimer" anywhere in the scene —
// only one should exist at a time (see the singleton Instance below, which
// is how CustomerController and CustomerManager reach it without needing a
// direct Inspector reference each).
//
// Two jobs:
//   1. Displays a "01:45"-style countdown (drag a TextMeshProUGUI into
//      Timer Label — put it next to a clock icon Image in your UI, like the
//      reference screenshot's top-right clock).
//   2. Drives the difficulty ramp: PatienceMultiplier and
//      SpawnIntervalMultiplier both change over the course of the level
//      according to the two AnimationCurves below. Start them ABOVE 1 (more
//      patience / slower spawns early on) and end BELOW 1 (less patience /
//      faster spawns later) for the "starts slower, gets faster and more
//      frustrated" pacing you described. Tune the curve shape directly in
//      the Inspector's curve editor — drag the end keys, add a middle key
//      if you want an S-curve instead of a straight ramp.
public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance { get; private set; }

    [Header("Timer")]
    [Tooltip("Total length of this level in seconds. 120 = 2 minutes.")]
    public float levelDuration = 120f;
    [Tooltip("Optional — a TextMeshProUGUI showing the countdown as mm:ss.")]
    public TextMeshProUGUI timerLabel;

    [Header("Difficulty ramp")]
    [Tooltip("X axis = fraction of the level elapsed (0-1). Y axis = multiplier applied " +
             "to every customer's patienceDuration. Start around 1.2-1.3 (more patient, " +
             "slower start) and end around 0.5-0.6 (less patient, tense finish).")]
    public AnimationCurve patienceMultiplierCurve = AnimationCurve.Linear(0f, 1.3f, 1f, 0.6f);
    [Tooltip("X axis = fraction of the level elapsed (0-1). Y axis = multiplier applied " +
             "to the random spawn interval range in CustomerManager. Start higher (slower " +
             "spawns) and end lower (faster, more frequent customers).")]
    public AnimationCurve spawnIntervalMultiplierCurve = AnimationCurve.Linear(0f, 1.3f, 1f, 0.6f);

    /// <summary>Fires once, the instant the countdown reaches 0.</summary>
    public System.Action onLevelEnd;

    public float Elapsed { get; private set; }
    public float ElapsedFraction => levelDuration > 0f ? Mathf.Clamp01(Elapsed / levelDuration) : 0f;
    public float PatienceMultiplier      => patienceMultiplierCurve.Evaluate(ElapsedFraction);
    public float SpawnIntervalMultiplier => spawnIntervalMultiplierCurve.Evaluate(ElapsedFraction);

    private bool ended;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("⚠️ More than one LevelTimer in the scene — keeping the first, destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (ended) return;

        Elapsed += Time.deltaTime;
        float remaining = Mathf.Max(0f, levelDuration - Elapsed);

        if (timerLabel != null)
        {
            int mm = Mathf.FloorToInt(remaining / 60f);
            int ss = Mathf.FloorToInt(remaining % 60f);
            timerLabel.text = $"{mm:00}:{ss:00}";
        }

        if (remaining <= 0f)
        {
            ended = true;
            Debug.Log("⏰ Level time's up!");
            onLevelEnd?.Invoke();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
