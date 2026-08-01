using System.Collections.Generic;
using UnityEngine;

// Lives on the 2D "Order Queue" panel GameObject — purely a Canvas UI
// element, nothing in the 3D scene. Adds one row per customer, but ONLY
// once they've actually reached the counter — CustomerManager calls
// AddEntry from the same onArrivedAtStation callback it already uses to
// sync the active recipe, so a customer still walking in off-screen never
// shows up here.
//
// Setup:
//   1. Build the row prefab: an OrderQueueRow component + a portrait Image
//      (customer face) + an order Image (the dish they ordered), laid out
//      side by side however you like. Save it as a prefab.
//   2. Under your Canvas, create the panel with a Vertical Layout Group on
//      a container Transform (so rows auto-stack without manual positioning).
//   3. Add this component to the panel, drag the row prefab into Row Prefab
//      and the Vertical-Layout-Group object into Row Container.
//   4. Drag this OrderQueueUI onto CustomerManager's Order Queue UI field.
public class OrderQueueUI : MonoBehaviour
{
    [Tooltip("The row prefab — must have an OrderQueueRow component.")]
    public OrderQueueRow rowPrefab;
    [Tooltip("The Transform rows get instantiated under. Should have a Vertical Layout " +
             "Group so rows auto-stack without any manual positioning.")]
    public Transform rowContainer;
    [Tooltip("The OrderQueuePanelSizer on the panel background — drives the panel's own " +
             "height directly, so it grows/shrinks correctly at 0, 1, or many rows. " +
             "Leave empty to skip (panel keeps whatever fixed size it has in the Editor).")]
    public OrderQueuePanelSizer panelSizer;

    private readonly Dictionary<CustomerController, OrderQueueRow> activeRows =
        new Dictionary<CustomerController, OrderQueueRow>();

    /// <summary>Call this the instant a customer reaches the counter — not before.
    /// addOnSprite is null for a plain order, or KottuRecipe.addOnSprite for a combo
    /// order — the row shows just one dish, or both joined by a "+", accordingly.</summary>
    public void AddEntry(CustomerController customer, Sprite portrait, Sprite orderSprite, Sprite addOnSprite)
    {
        if (customer == null || activeRows.ContainsKey(customer)) return;
        if (rowPrefab == null || rowContainer == null)
        {
            Debug.LogWarning("⚠️ OrderQueueUI: Row Prefab / Row Container not assigned.");
            return;
        }

        OrderQueueRow row = Instantiate(rowPrefab, rowContainer);
        row.Setup(portrait, orderSprite, addOnSprite);
        activeRows[customer] = row;

        if (panelSizer != null) panelSizer.SetRowCount(activeRows.Count);
    }

    /// <summary>Call this when a customer leaves the queue (served or angry-left).</summary>
    public void RemoveEntry(CustomerController customer)
    {
        if (customer == null) return;
        if (activeRows.TryGetValue(customer, out OrderQueueRow row))
        {
            if (row != null) Destroy(row.gameObject);
            activeRows.Remove(customer);
        }

        if (panelSizer != null) panelSizer.SetRowCount(activeRows.Count);
    }
}