using System.Collections;
using UnityEngine;

// Attach to an egg-shaped GameObject (a sphere scaled to 0.8, 1.0, 0.8).
// Call Crack() from FoxAnimationController mid-sequence for Egg Kottu.
// The egg drops in, squishes (cracks), yolk bursts out, then disappears.
public class EggCrackEffect : MonoBehaviour
{
    [Tooltip("Drag the yolk burst Particle System (child of this egg) here.")]
    public ParticleSystem yolkBurst;

    [Tooltip("How far above the grill the egg starts its drop.")]
    public float dropHeight = 0.6f;

    [Tooltip("How long the egg takes to drop onto the grill.")]
    public float dropDuration = 0.2f;

    private Vector3 homePosition;
    private Vector3 homeScale;

    void Awake()
    {
        homePosition = transform.position;
        homeScale    = transform.localScale;
        gameObject.SetActive(false); // hidden until Crack() is called
    }

    // Called by FoxAnimationController at the mid-point of the Egg Kottu sequence
    public void Crack()
    {
        StartCoroutine(CrackRoutine());
    }

    private IEnumerator CrackRoutine()
    {
        // Reset and show
        transform.position   = homePosition + Vector3.up * dropHeight;
        transform.localScale = homeScale * 0.3f;
        gameObject.SetActive(true);

        // --- Drop and grow ---
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 2f); // ease-out
            transform.position   = Vector3.Lerp(homePosition + Vector3.up * dropHeight, homePosition, eased);
            transform.localScale = homeScale * Mathf.Lerp(0.3f, 1f, eased);
            yield return null;
        }

        // Snap to position
        transform.position   = homePosition;
        transform.localScale = homeScale;

        // Short pause — egg just landed
        yield return new WaitForSeconds(0.05f);

        // --- Crack: squish outward ---
        transform.localScale = new Vector3(homeScale.x * 1.5f, homeScale.y * 0.3f, homeScale.z * 1.5f);

        // Yolk burst!
        if (yolkBurst != null) yolkBurst.Play();

        // Hold the squish shape briefly
        yield return new WaitForSeconds(0.12f);

        // --- Disappear ---
        gameObject.SetActive(false);
        transform.localScale = homeScale; // reset for next time

        Debug.Log("🥚 Egg cracked!");
    }
}
