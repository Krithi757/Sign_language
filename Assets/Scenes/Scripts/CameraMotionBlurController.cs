using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraMotionBlurController : MonoBehaviour
{
    public PostProcessVolume postProcessVolume; // Reference to the Post-Processing Volume
    private MotionBlur motionBlurEffect;
    private CharacterController characterController;

    public float speedThreshold = 5f; // Speed threshold for activating motion blur
    public float maxMotionBlurIntensity = 0.3f; // Max motion blur intensity

    void Start()
    {
        // Retrieve the motion blur effect from the post-processing profile
        postProcessVolume.profile.TryGetSettings(out motionBlurEffect);
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Calculate the character speed
        float speed = characterController.velocity.magnitude;

        // If the character is moving fast, increase motion blur intensity
        if (speed > speedThreshold)
        {
            motionBlurEffect.enabled.value = true;
            motionBlurEffect.shutterAngle.value = Mathf.Lerp(0, 360, speed / maxMotionBlurIntensity);
        }
        else
        {
            motionBlurEffect.enabled.value = false;
        }
    }
}
