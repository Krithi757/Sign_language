using UnityEngine;
using UnityEngine.UI;

// Lives on the row prefab used by OrderQueueUI — one instance per customer
// currently waiting/stationed. "Dumb" display component: shows who ordered
// (portrait) and what they ordered — either ONE dish (plain order) or TWO
// dishes joined by a "+" (combo order — KottuRecipe.orderSprite plus its
// optional addOnSprite, e.g. Vegetable Kottu + Egg/Cheese/Chicken).
//
// Prefab hierarchy to build:
//   OrderQueueRow (this script, on the row root)
//     Portrait      (Image)
//     FoodGroup                         <- Horizontal Layout Group lives HERE
//       OrderIcon   (Image + LayoutElement)
//       PlusSign    (a "+" — TextMeshProUGUI or a small Image, your choice)
//       AddOnIcon   (Image + LayoutElement)
//
// FoodGroup's Horizontal Layout Group settings:
//   Child Alignment      = Middle Center
//   Control Child Size   = Width ✓, Height ✓   (size comes from each icon's
//                          own LayoutElement below, not the group)
//   Spacing              = ~6-10px
//
// This is what gives you both behaviors for free: with only OrderIcon active
// (plain order), the group centers that single icon perfectly in FoodGroup's
// area. With OrderIcon + PlusSign + AddOnIcon all active (combo order), the
// group centers the whole 3-piece row as one unit — no manual positioning.
public class OrderQueueRow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Shows the customer's face (Pig/Dog) — CustomerController.portraitIcon.")]
    public Image portraitIcon;

    [Tooltip("Base dish image — KottuRecipe.orderSprite. Always shown.")]
    public Image orderImage;
    [Tooltip("LayoutElement on the SAME object as Order Image.")]
    public LayoutElement orderImageLayout;

    [Tooltip("The '+' shown only between the two icons for a combo order.")]
    public GameObject plusSign;

    [Tooltip("Add-on dish image — KottuRecipe.addOnSprite. Hidden entirely for plain orders.")]
    public Image addOnImage;
    [Tooltip("LayoutElement on the SAME object as Add-On Image.")]
    public LayoutElement addOnImageLayout;

    [Header("Sizing")]
    [Tooltip("Icon width/height when it's the ONLY dish shown (plain order, no add-on).")]
    public Vector2 singleIconSize = new Vector2(70f, 70f);
    [Tooltip("Icon width/height for EACH dish when a combo (2 dishes + '+') is shown — " +
             "a bit smaller so both fit comfortably without overflowing the row.")]
    public Vector2 comboIconSize = new Vector2(54f, 54f);

    public void Setup(Sprite portrait, Sprite order, Sprite addOn)
    {
        if (portraitIcon != null && portrait != null) portraitIcon.sprite = portrait;

        bool isCombo = addOn != null;

        if (orderImage != null)
        {
            orderImage.sprite  = order;
            orderImage.enabled = order != null;
        }
        ApplySize(orderImageLayout, isCombo ? comboIconSize : singleIconSize);

        if (addOnImage != null)
        {
            addOnImage.sprite = addOn;
            addOnImage.gameObject.SetActive(isCombo);
        }
        ApplySize(addOnImageLayout, comboIconSize);

        if (plusSign != null) plusSign.SetActive(isCombo);
    }

    private void ApplySize(LayoutElement layout, Vector2 size)
    {
        if (layout == null) return;
        layout.preferredWidth  = size.x;
        layout.preferredHeight = size.y;
    }
}