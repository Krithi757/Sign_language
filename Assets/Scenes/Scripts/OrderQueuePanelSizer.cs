using UnityEngine;

// Lives on OrderQueuePanel itself. Sets the panel's own height directly in
// code instead of relying on a Content Size Fitter + Layout Element stack —
// that combo kept producing edge-case bugs (an empty panel collapsing down
// to just the header with no box beneath it, or a single row losing all its
// padding) because a manually-added Layout Element and a Vertical Layout
// Group on the same GameObject don't reliably combine in Unity (their values
// can get averaged instead of the Layout Element acting as a hard floor).
//
// This replaces that with a plain formula: header height + however many rows
// (but never fewer than minEmptyRows, so the panel never collapses to
// nothing) + spacing between them + your padding. OrderQueueUI calls
// SetRowCount(...) once every time a row is added or removed, and that's it.
//
// Setup:
//   1. Remove the Content Size Fitter component from OrderQueuePanel (delete
//      it) — this script now owns the height entirely, and the Fitter would
//      just fight it every layout pass.
//   2. Remove any Layout Element you added directly to OrderQueuePanel too —
//      no longer needed.
//   3. KEEP OrderQueuePanel's Vertical Layout Group — it still auto-stacks
//      TextContainer then RowContainer inside whatever height this script
//      sets, you just don't need Control Child Size ticked for Height.
//   4. Add this script to OrderQueuePanel. Panel Rect will auto-grab the
//      RectTransform on the same object if you leave it empty.
//   5. Drag this component into OrderQueueUI's new "Panel Sizer" field.
//   6. Tune headerHeight / rowHeight / rowSpacing / verticalPadding below to
//      match your actual art — start with the defaults and nudge from there
//      by comparing against your best-looking screenshot (the 2-row one).
//   7. Make sure OrderQueuePanel's Pivot Y is 1 (top-anchored) so it grows
//      downward from a fixed top edge instead of shifting the header as it
//      resizes.
public class OrderQueuePanelSizer : MonoBehaviour
{
    [Tooltip("The panel's own RectTransform. Leave empty to auto-grab the one on this GameObject.")]
    public RectTransform panelRect;

    [Header("Sizing — tune these to match your art")]
    [Tooltip("Height reserved for the 'ORDER QUEUE' title bar (TextContainer).")]
    public float headerHeight = 50f;
    [Tooltip("Height of a single row.")]
    public float rowHeight = 90f;
    [Tooltip("Vertical gap between rows — match RowContainer's Vertical Layout Group Spacing.")]
    public float rowSpacing = 8f;
    [Tooltip("Extra top+bottom breathing room inside the panel, combined.")]
    public float verticalPadding = 24f;
    [Tooltip("How many rows' worth of space to always reserve, even at 0 customers — " +
             "this is what keeps the panel from collapsing to just the header.")]
    public int minEmptyRows = 1;

    void Awake()
    {
        if (panelRect == null) panelRect = GetComponent<RectTransform>();
        SetRowCount(0);
    }

    /// <summary>Call this every time a row is added to or removed from the queue.</summary>
    public void SetRowCount(int count)
    {
        if (panelRect == null) return;

        int rowsForHeight = Mathf.Max(count, minEmptyRows);
        float rowsHeight  = rowsForHeight * rowHeight + Mathf.Max(0, rowsForHeight - 1) * rowSpacing;
        float total = headerHeight + rowsHeight + verticalPadding;

        Vector2 size = panelRect.sizeDelta;
        size.y = total;
        panelRect.sizeDelta = size;
    }
}
