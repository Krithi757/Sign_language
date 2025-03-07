using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SkyboxController : MonoBehaviour
{
    public Material morningSkybox; // Assign your morning skybox material
    public Material eveningSkybox; // Assign your evening skybox material
    public Material nightSkybox;   // Assign your night skybox material

    public PostProcessProfile morningProfile; // Assign morning profile
    public PostProcessProfile eveningProfile; // Assign evening profile
    public PostProcessProfile nightProfile;   // Assign night profile

    private float cycleTime;

    private PostProcessVolume postProcessVolume;

    void Start()
    {
        // Find the PostProcessVolume in the scene
        postProcessVolume = FindObjectOfType<PostProcessVolume>();
    }

    void Update()
    {
        cycleTime = Mathf.Repeat(Time.time, 15); // Resets every 15 seconds

        if (cycleTime < 5)
        {
            // Morning
            RenderSettings.skybox = morningSkybox;
            postProcessVolume.profile = morningProfile;
        }
        else if (cycleTime < 10)
        {
            // Evening
            RenderSettings.skybox = eveningSkybox;
            postProcessVolume.profile = eveningProfile;
        }
        else
        {
            // Night
            RenderSettings.skybox = nightSkybox;
            postProcessVolume.profile = nightProfile;
        }
    }
}
