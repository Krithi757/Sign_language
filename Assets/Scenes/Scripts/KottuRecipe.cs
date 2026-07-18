using UnityEngine;

[System.Serializable]
public class KottuRecipe
{
    [Tooltip("E.g. 'Egg Kottu'")]
    public string displayName;

    [Tooltip("Emoji shown in the coin popup and thought bubble. E.g. 🥚")]
    public string emoji;

    [Tooltip("Base coins earned for completing this order.")]
    public int coinReward = 10;

    [Tooltip("Extra coins if the player answers within speedBonusSeconds.")]
    public int speedBonusCoins = 5;

    [Tooltip("Time window (seconds) to qualify for the speed bonus.")]
    public float speedBonusSeconds = 5f;

    [Header("Finished Dish")]
    [Tooltip("Drag the meal's MealAppearEffect component here.")]
    public MealAppearEffect mealObject;

    [Header("Thought Bubble")]
    [Tooltip("Base kottu image shown in the thought bubble (left side, always shown).")]
    public Sprite orderSprite;

    [Tooltip("Add-on ingredient image (cheese / egg / chicken). " +
             "LEAVE EMPTY for plain Vegetable Kottu. " +
             "When set, the bubble shows: [orderSprite] + [addOnSprite].")]
    public Sprite addOnSprite;

    [Header("Optional Unique VFX (leave empty if not needed)")]
    [Tooltip("Seconds from cooking start to fire this effect. 0 = disabled.")]
    public float uniqueEffectDelay = 0f;

    [Tooltip("Particle system for a special mid-cook effect (e.g. cheese sparkle).")]
    public ParticleSystem uniqueVFX;
}
