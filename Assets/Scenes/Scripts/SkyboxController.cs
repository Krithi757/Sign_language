using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SkyboxController : MonoBehaviour
{
    public Material morningSkybox;
    public Material eveningSkybox;
    public Material nightSkybox;

    public PostProcessProfile morningProfile;
    public PostProcessProfile eveningProfile;
    public PostProcessProfile nightProfile;

    private float cycleDuration = 180f; // Each cycle lasts 3 minutes (180 seconds)
    private float cycleTime;
    private PostProcessVolume postProcessVolume;

    void Start()
    {
        postProcessVolume = FindObjectOfType<PostProcessVolume>();

        // Ensure the morning skybox is set by default
        RenderSettings.skybox = morningSkybox;
        if (postProcessVolume != null)
        {
            postProcessVolume.profile = morningProfile;
        }
    }

    void Update()
    {
        cycleTime = Mathf.Repeat(Time.time, cycleDuration * 3); // Full cycle of Morning -> Evening -> Night

        if (cycleTime < cycleDuration)
        {
            SetSkybox(morningSkybox, morningProfile);
        }
        else if (cycleTime < cycleDuration * 2)
        {
            SetSkybox(eveningSkybox, eveningProfile);
        }
        else
        {
            SetSkybox(nightSkybox, nightProfile);
        }
    }

    void SetSkybox(Material skyboxMaterial, PostProcessProfile profile)
    {
        if (RenderSettings.skybox != skyboxMaterial)
        {
            RenderSettings.skybox = skyboxMaterial;
        }

        if (postProcessVolume != null && postProcessVolume.profile != profile)
        {
            postProcessVolume.profile = profile;
        }
    }
}
