using UnityEngine;

// One entry per kottu type. Configure all 5 recipes in the OrderManager Inspector.
[System.Serializable]
public class KottuRecipe
{
    [Tooltip("What shows in the coin popup. E.g. 'Egg Kottu'")]
    public string displayName;

    [Tooltip("Emoji shown in the popup. E.g. 🥚")]
    public string emoji;

    [Tooltip("Base coins awarded for completing this order.")]
    public int coinReward = 10;

    [Tooltip("Extra coins if the player answers within speedBonusSeconds.")]
    public int speedBonusCoins = 5;

    [Tooltip("How many seconds counts as 'fast'. Default 5.")]
    public float speedBonusSeconds = 5f;

    [Header("Visuals")]
    [Tooltip("The finished meal GameObject (with MealAppearEffect). One per recipe.")]
    public MealAppearEffect mealObject;

    [Tooltip("Optional unique VFX that plays near the END of the cooking sequence. " +
             "E.g. cheese sparkle, big flame burst, egg sizzle.")]
    public ParticleSystem uniqueEndVFX;
}
