using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the ThoughtBubbleCanvas (World Space Canvas, child of pig).
//
// Hierarchy to build in Unity:
//
//   ThoughtBubbleCanvas  [Canvas – World Space]  ← this script here
//     └── BubbleRoot     [Empty GameObject]       ← drag into bubbleRoot
//           ├── Background  [Image]               ← white bubble sprite
//           ├── DishImage   [Image]               ← recipe photo sprite
//           └── DishLabel   [TextMeshProUGUI]     ← emoji + dish name
public class ThoughtBubble : MonoBehaviour
{
    [Tooltip("The parent that gets shown/hidden and scaled for the pop-in animation.")]
    public GameObject bubbleRoot;

    [Tooltip("Shows the dish photo (KottuRecipe.orderSprite).")]
    public Image dishImage;

    [Tooltip("Shows the emoji and dish name, e.g. '🧀\\nCheese Kottu'.")]
    public TextMeshProUGUI dishLabel;

    void Awake()
    {
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
    }

    public void Show(KottuRecipe recipe)
    {
        if (bubbleRoot == null) return;

        if (dishImage != null)
        {
            dishImage.enabled = recipe.orderSprite != null;
            if (recipe.orderSprite != null)
                dishImage.sprite = recipe.orderSprite;
        }

        if (dishLabel != null)
            dishLabel.text = recipe.emoji + "\n" + recipe.displayName;

        bubbleRoot.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PopIn());
    }

    public void Hide()
    {
        StopAllCoroutines();
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
    }

    // Bouncy scale-up animation
    private IEnumerator PopIn()
    {
        Transform t = bubbleRoot.transform;
        t.localScale = Vector3.zero;
        float duration = 0.28f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s = EaseOutBack(Mathf.Clamp01(elapsed / duration));
            t.localScale = Vector3.one * s;
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
