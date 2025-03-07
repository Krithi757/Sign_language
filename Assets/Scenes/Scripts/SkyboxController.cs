using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SkyboxController : MonoBehaviour
{
    public Material morningSkybox; // Assign your morning skybox
    public Material eveningSkybox; // Assign your evening skybox
    public Material nightSkybox;   // Assign your night skybox

    public PostProcessVolume postProcessVolume; // Reference to the PostProcess Volume
    private Bloom bloom; // Reference to the Bloom effect

    private float cycleTime;

    void Start()
    {
        // Get the Bloom effect from the PostProcessVolume
        postProcessVolume.profile.TryGetSettings(out bloom);
    }

    void Update()
    {
        cycleTime = Mathf.Repeat(Time.time, 15); // Resets every 15 seconds

        if (cycleTime < 5)
        {
            RenderSettings.skybox = morningSkybox; // Morning Skybox
            if (bloom != null) bloom.active = false; // Disable Bloom during morning
        }
        else if (cycleTime < 10)
        {
            RenderSettings.skybox = eveningSkybox; // Evening Skybox
            if (bloom != null) bloom.active = true; // Enable Bloom during evening
        }
        else
        {
            RenderSettings.skybox = nightSkybox; // Night Skybox
            if (bloom != null) bloom.active = true; // Enable Bloom during night
        }
    }
}
