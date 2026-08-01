using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ONE shared patience bar for the whole scene, built on your Slider_bg
// hierarchy (Slider_bg > FireIcon, Slider > Background/RawImage/Fill Area/Fill).
//
// Portrait icons OVERLAP inside a single round icon frame rather than sitting
// in a laid-out row: the front (tracked, queue[0]) customer's icon always
// animates to centerPosition (dead center of the frame); a second/waiting
// customer's icon sits offset at backPosition, peeking out from behind it.
// When the front customer leaves, whichever icon was showing the waiting
// customer SLIDES smoothly from backPosition into centerPosition and
// becomes the new tracked icon — no snapping, no layout-group reflow.
//
// The bar itself (fill/color/blink/fire escalation) only ever tracks the
// front-most stationed customer. A second/waiting customer's own countdown
// keeps running underneath for real — it's just not separately visualized,
// which is why Dog (or whichever character tends to end up waiting) should
// get a much higher patienceDuration than Pig.
//
// CustomerManager calls SetStationedCustomers(...) at the exact same moment
// it already syncs the active recipe / freezes-unfreezes the video, so the
// bar, the video, and the active order are always in lockstep automatically.
public class ActivePatienceUI : MonoBehaviour
{
    [Header("Bar (Unity UI Slider)")]
    [Tooltip("Drag the Slider component itself here.")]
    public Slider slider;
    [Tooltip("Slider > Fill Area > Fill — its Image gets recolored green->yellow->red " +
             "as patience drops. The Slider's own value drives how far it's filled.")]
    public Image fillImage;

    [Header("Customer face icons (overlapping — size this to exactly 2)")]
    [Tooltip("Two portrait Images, both children of the same round icon frame, both " +
             "anchored/pivoted the same way. Which one is showing which customer is " +
             "reassigned dynamically at runtime — don't rely on element order in the " +
             "Editor, the script tracks who owns which slot itself. Do NOT put a " +
             "Horizontal Layout Group on their parent — positions are driven directly " +
             "by this script (centerPosition / backPosition below), a layout group would " +
             "fight it every frame.")]
    public Image[] portraitSlots;

    [Tooltip("Local anchoredPosition (relative to the icon frame) the FRONT/tracked " +
             "customer's icon animates to. Set this to wherever dead-center of your " +
             "round icon frame artwork actually is — usually (0,0), but nudge it here " +
             "if the frame graphic itself isn't centered on its own RectTransform.")]
    public Vector2 centerPosition = Vector2.zero;

    [Tooltip("Local anchoredPosition a WAITING (second) customer's icon sits at — " +
             "offset to the left so it peeks out from behind the front icon.")]
    public Vector2 backPosition = new Vector2(-28f, 0f);

    [Tooltip("How long the slide from backPosition to centerPosition takes when the " +
             "front customer leaves and the waiting one is promoted.")]
    public float slideDuration = 0.25f;

    [Header("Fire icon (right side — your existing FireIcon object)")]
    [Tooltip("Add a PatienceFireIcon component to your FireIcon object and drag it here.")]
    public PatienceFireIcon fireIcon;

    [Tooltip("The whole bar's root GameObject (e.g. Slider_bg itself) — hidden " +
             "automatically when nobody is currently stationed.")]
    public GameObject rootPanel;

    [Header("Colors — green (calm) -> yellow (getting impatient) -> red (about to leave)")]
    public Color calmColor    = new Color(0.35f, 0.8f, 0.35f);
    public Color warningColor = new Color(0.95f, 0.75f, 0.15f);
    public Color angryColor   = new Color(0.85f, 0.2f, 0.2f);
    [Range(0f, 1f)] public float warningThreshold = 0.5f;
    [Range(0f, 1f)] public float angryThreshold   = 0.2f;

    private class SlotState
    {
        public Image image;
        public RectTransform rect;
        public CustomerController owner;
        public Coroutine moveCoroutine;
    }

    private SlotState[] slots;
    private CustomerController tracked;
    private float   lastFraction = 1f;
    private bool    portraitUrgent;
    private Coroutine portraitBlinkCoroutine;

    void Awake()
    {
        int count = portraitSlots != null ? portraitSlots.Length : 0;

        if (count < 2)
        {
            Debug.LogWarning("⚠️ ActivePatienceUI: Portrait Slots should be sized to 2 with both " +
                              "Image objects assigned in the Inspector — currently " + count + ". " +
                              "Icons will not appear until this is fixed.");
        }

        slots = new SlotState[count];
        for (int i = 0; i < count; i++)
        {
            if (portraitSlots[i] == null)
            {
                Debug.LogWarning($"⚠️ ActivePatienceUI: Portrait Slots element {i} is empty — drag a portrait Image into it.");
                slots[i] = new SlotState();
                continue;
            }

            // A leftover Horizontal/Vertical Layout Group on the icons' parent will
            // fight this script every frame (it manually drives anchoredPosition for
            // the overlap/slide effect) — commonly the reason icons end up invisible
            // or stuck at (0,0) after switching from the old laid-out design.
            Transform parent = portraitSlots[i].transform.parent;
            if (parent != null && parent.GetComponent<HorizontalOrVerticalLayoutGroup>() != null)
            {
                Debug.LogWarning("⚠️ ActivePatienceUI: " + parent.name + " still has a Layout Group " +
                                  "component on it — remove it. This script positions portrait icons " +
                                  "manually (centerPosition/backPosition), and a Layout Group will " +
                                  "override that every layout pass.");
            }

            slots[i] = new SlotState
            {
                image = portraitSlots[i],
                rect  = portraitSlots[i].rectTransform,
            };
            portraitSlots[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by CustomerManager whenever the set of stationed customers
    /// changes — pass ALL of them, front-to-back in queue order (empty list
    /// if nobody currently qualifies). Only stationed[0] (front) and
    /// stationed[1] (back/waiting) are shown; the bar/fire/blink always
    /// track stationed[0] only.
    /// </summary>
    public void SetStationedCustomers(List<CustomerController> stationed)
    {
        CustomerController front = stationed != null && stationed.Count > 0 ? stationed[0] : null;
        CustomerController back  = stationed != null && stationed.Count > 1 ? stationed[1] : null;

        SlotState frontSlot = AssignSlot(front, centerPosition);
        SlotState backSlot  = AssignSlot(back, backPosition);

        // Front renders on top of back so it visually overlaps in front.
        if (backSlot != null) backSlot.rect.SetAsFirstSibling();
        if (frontSlot != null) frontSlot.rect.SetAsLastSibling();

        // Free any slot whose owner isn't front or back anymore (served, left angry).
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                SlotState s = slots[i];
                if (s.owner != null && s.owner != front && s.owner != back)
                {
                    s.owner = null;
                    if (s.moveCoroutine != null) { StopCoroutine(s.moveCoroutine); s.moveCoroutine = null; }
                    if (s.image != null)
                    {
                        s.image.gameObject.SetActive(false);
                        SetSlotAlpha(s.image, 1f);
                    }
                }
            }
        }

        if (front != tracked)
        {
            tracked = front;
            SetPortraitUrgent(false);
            if (fireIcon != null) fireIcon.ResetFire();
        }

        if (tracked == null)
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            return;
        }

        if (rootPanel != null) rootPanel.SetActive(true);
        lastFraction = tracked.PatienceFraction;
        Apply(lastFraction);
    }

    /// <summary>
    /// Makes sure `customer` (if not null) owns a slot and that slot is
    /// sliding toward targetPos. Reuses the slot it already owns if it has
    /// one (so promotions SLIDE instead of snapping to a different icon);
    /// otherwise claims a free slot and snaps straight to targetPos (nothing
    /// to slide from — this customer wasn't shown a moment ago).
    /// </summary>
    private SlotState AssignSlot(CustomerController customer, Vector2 targetPos)
    {
        if (customer == null || slots == null) return null;

        SlotState existing = System.Array.Find(slots, s => s.owner == customer);
        if (existing == null)
        {
            existing = System.Array.Find(slots, s => s.owner == null)
                       ?? System.Array.Find(slots, s => s.owner != customer);
            if (existing == null) return null; // no spare slot — shouldn't happen with 2 slots max 2 customers

            existing.owner = customer;
            if (existing.image != null)
            {
                existing.image.gameObject.SetActive(true);
                if (customer.portraitIcon != null) existing.image.sprite = customer.portraitIcon;
                SetSlotAlpha(existing.image, 1f);
            }
            SnapTo(existing, targetPos);
        }
        else if (existing.image != null && customer.portraitIcon != null)
        {
            existing.image.sprite = customer.portraitIcon;
        }

        SlideTo(existing, targetPos);
        return existing;
    }

    private void SnapTo(SlotState slot, Vector2 pos)
    {
        if (slot.rect == null) return;
        if (slot.moveCoroutine != null) { StopCoroutine(slot.moveCoroutine); slot.moveCoroutine = null; }
        slot.rect.anchoredPosition = pos;
    }

    private void SlideTo(SlotState slot, Vector2 pos)
    {
        if (slot.rect == null) return;
        if (slot.rect.anchoredPosition == pos) return; // already there
        if (slot.moveCoroutine != null) StopCoroutine(slot.moveCoroutine);
        slot.moveCoroutine = StartCoroutine(SlideRoutine(slot, pos));
    }

    private IEnumerator SlideRoutine(SlotState slot, Vector2 targetPos)
    {
        Vector2 start = slot.rect.anchoredPosition;
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = slideDuration > 0f ? Mathf.Clamp01(t / slideDuration) : 1f;
            p = 1f - (1f - p) * (1f - p); // ease-out
            slot.rect.anchoredPosition = Vector2.Lerp(start, targetPos, p);
            yield return null;
        }
        slot.rect.anchoredPosition = targetPos;
        slot.moveCoroutine = null;
    }

    void Update()
    {
        if (tracked == null) return;

        float fraction = tracked.PatienceFraction;
        Apply(fraction);

        bool urgent = fraction <= tracked.lowPatienceThreshold;
        if (urgent != portraitUrgent) SetPortraitUrgent(urgent);

        if (fireIcon != null) fireIcon.UpdateFraction(fraction, tracked.lowPatienceThreshold);

        // Patience just fully ran out this frame (as opposed to being served,
        // where the countdown gets cancelled early at some non-zero value) —
        // this is the one clean, decoupled signal for "customer just yelled
        // and started leaving from a timeout," so fire it here.
        if (lastFraction > 0f && fraction <= 0f && fireIcon != null)
            fireIcon.Blast();

        lastFraction = fraction;
    }

    private void Apply(float fraction)
    {
        fraction = Mathf.Clamp01(fraction);
        if (slider != null) slider.value = fraction;
        if (fillImage == null) return;

        if (fraction <= angryThreshold)
        {
            fillImage.color = angryColor;
        }
        else if (fraction <= warningThreshold)
        {
            float t = (fraction - angryThreshold) / Mathf.Max(0.0001f, warningThreshold - angryThreshold);
            fillImage.color = Color.Lerp(angryColor, warningColor, t);
        }
        else
        {
            float t = (fraction - warningThreshold) / Mathf.Max(0.0001f, 1f - warningThreshold);
            fillImage.color = Color.Lerp(warningColor, calmColor, t);
        }
    }

    private void SetPortraitUrgent(bool urgent)
    {
        if (portraitUrgent == urgent) return;
        portraitUrgent = urgent;

        if (portraitBlinkCoroutine != null) StopCoroutine(portraitBlinkCoroutine);

        if (urgent)
        {
            portraitBlinkCoroutine = StartCoroutine(PortraitBlink());
        }
        else
        {
            portraitBlinkCoroutine = null;
            SetTrackedSlotAlpha(1f);
        }
    }

    private IEnumerator PortraitBlink()
    {
        const float interval = 0.22f;
        while (true)
        {
            SetTrackedSlotAlpha(0.35f);
            yield return new WaitForSeconds(interval);
            SetTrackedSlotAlpha(1f);
            yield return new WaitForSeconds(interval);
        }
    }

    // Blink only ever touches whichever slot is CURRENTLY showing the
    // tracked (front) customer — found dynamically, since a promoted
    // customer may now occupy what used to be the "back" slot.
    private void SetTrackedSlotAlpha(float a)
    {
        if (tracked == null || slots == null) return;
        SlotState s = System.Array.Find(slots, x => x.owner == tracked);
        if (s?.image == null) return;
        SetSlotAlpha(s.image, a);
    }

    private void SetSlotAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}