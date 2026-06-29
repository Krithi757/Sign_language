using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

// Lives on its own empty GameObject (e.g. "WordRoundManager") under Canvas.
//
// Whenever the video changes (including the very first one), this grabs every
// video's word from TapToChangeVideo, picks one correct word + a few wrong
// ones, shuffles them, and spawns a draggable word chip for each one inside
// wordContainer. VideoDropTarget reads currentCorrectWord to check a drop.
public class WordRoundManager : MonoBehaviour
{
    [Tooltip("Drag the VideoScreen GameObject (the one with TapToChangeVideo on it) here.")]
    public TapToChangeVideo videoController;

    [Tooltip("Drag the empty container that has a Horizontal Layout Group on it (e.g. WordContainer) - the word chips get spawned inside this.")]
    public RectTransform wordContainer;

    [Tooltip("Drag the WordOption prefab here (the draggable word chip).")]
    public GameObject wordOptionPrefab;

    [Range(3, 5)]
    [Tooltip("How many word options to show each round - one of these will always be the correct word.")]
    public int wordsPerRound = 4;

    // VideoDropTarget reads this to check whether a dropped word was right or wrong.
    [HideInInspector] public string currentCorrectWord;

    void Awake()
    {
        // Subscribed here (not Start) so we never miss the very first video,
        // no matter which GameObject's Start() happens to run first.
        if (videoController != null)
        {
            videoController.OnVideoChanged += HandleVideoChanged;
        }
        else
        {
            Debug.LogError("❌ WordRoundManager: Video Controller is not assigned in the Inspector.");
        }
    }

    void OnDestroy()
    {
        if (videoController != null)
            videoController.OnVideoChanged -= HandleVideoChanged;
    }

    private void HandleVideoChanged(VideoClip clip)
    {
        SpawnRoundFor(clip);
    }

    private void SpawnRoundFor(VideoClip clip)
    {
        if (wordContainer == null || wordOptionPrefab == null)
        {
            Debug.LogError("❌ WordRoundManager: Word Container or Word Option Prefab is not assigned in the Inspector.");
            return;
        }

        // Clear out last round's word chips.
        for (int i = wordContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(wordContainer.GetChild(i).gameObject);
        }

        currentCorrectWord = TapToChangeVideo.GetWordFromClipName(clip.name);

        // Build the pool of possible wrong words from every video in this level.
        List<string> pool = videoController.VideoClips
            .Select(c => TapToChangeVideo.GetWordFromClipName(c.name))
            .Where(w => w != currentCorrectWord)
            .Distinct()
            .ToList();

        Shuffle(pool);

        int wrongCount = Mathf.Min(wordsPerRound - 1, pool.Count);
        List<string> roundWords = pool.Take(wrongCount).ToList();
        roundWords.Add(currentCorrectWord);
        Shuffle(roundWords); // so the correct word isn't always last

        Debug.Log("🔤 Word options this round: " + string.Join(", ", roundWords) + "  (correct: " + currentCorrectWord + ")");

        foreach (string word in roundWords)
        {
            GameObject chip = Instantiate(wordOptionPrefab, wordContainer);
            DraggableWord dw = chip.GetComponent<DraggableWord>();
            if (dw != null)
            {
                dw.SetWord(word);
            }
            else
            {
                Debug.LogError("❌ WordOption prefab is missing the DraggableWord component.");
            }
        }
    }

    private void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            string temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
}
