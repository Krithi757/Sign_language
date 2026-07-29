using UnityEngine;

// Attach to an empty GameObject positioned where you want the road to
// start (e.g. right at the edge of your existing counter floor). Rotate
// that empty so its LOCAL Z axis (the blue arrow on its Move gizmo when
// selected) points the same direction the camera looks — away from the
// player, down the road into the distance.
//
// Assign your road segment / lamp post / tree prefabs below, tune the
// spacing fields, then press Play — this builds the entire strip
// automatically instead of you manually duplicating and positioning
// dozens of objects by hand. It rebuilds identically every time (same
// random seed), so it won't look different each time you playtest.
public class EnvironmentRowBuilder : MonoBehaviour
{
    [Header("Road — repeating segments going straight back")]
    public GameObject roadSegmentPrefab;
    [Tooltip("Length of ONE road segment along Z (world units). Select the segment " +
             "prefab and check its Mesh Renderer bounds (Size.z) if you're not sure.")]
    public float roadSegmentLength = 10f;
    public int   roadSegmentCount  = 10;

    [Header("Lamp posts — mirrored left/right pairs")]
    public GameObject lampPrefab;
    public float lampStartZ     = 5f;
    public float lampSpacing    = 15f;
    [Tooltip("How far out from the road center each lamp sits.")]
    public float lampSideOffset = 6f;
    public int   lampPairCount  = 8;

    [Header("Trees — mirrored left/right pairs, further out than the lamps")]
    public GameObject treePrefab;
    [Tooltip("Offset from the lamp spacing so trees interleave with lamps instead of lining up exactly.")]
    public float treeStartZ      = 12f;
    public float treeSpacing     = 15f;
    public float treeSideOffset  = 9f;
    public int   treePairCount   = 8;
    [Tooltip("Random Y rotation + slight scale variance so repeated trees don't look copy-pasted.")]
    public bool  randomizeTrees   = true;
    public Vector2 treeScaleRange = new Vector2(0.9f, 1.15f);
    [Tooltip("Same seed every time = the same-looking arrangement every time you press Play.")]
    public int   randomSeed       = 12345;

    void Awake()
    {
        Random.InitState(randomSeed);
        BuildRoad();
        BuildLamps();
        BuildTrees();
    }

    private void BuildRoad()
    {
        if (roadSegmentPrefab == null) return;
        for (int i = 0; i < roadSegmentCount; i++)
        {
            Vector3 pos = transform.position + transform.forward * (roadSegmentLength * i);
            Instantiate(roadSegmentPrefab, pos, transform.rotation, transform);
        }
    }

    private void BuildLamps()
    {
        if (lampPrefab == null) return;
        for (int i = 0; i < lampPairCount; i++)
        {
            float z = lampStartZ + lampSpacing * i;
            PlacePair(lampPrefab, z, lampSideOffset, randomize: false);
        }
    }

    private void BuildTrees()
    {
        if (treePrefab == null) return;
        for (int i = 0; i < treePairCount; i++)
        {
            float z = treeStartZ + treeSpacing * i;
            PlacePair(treePrefab, z, treeSideOffset, randomize: randomizeTrees);
        }
    }

    private void PlacePair(GameObject prefab, float z, float sideOffset, bool randomize)
    {
        foreach (int side in new[] { -1, 1 })
        {
            Vector3 pos = transform.position
                + transform.forward * z
                + transform.right * (side * sideOffset);
            Quaternion rot = transform.rotation;
            if (randomize) rot *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject go = Instantiate(prefab, pos, rot, transform);
            if (randomize)
                go.transform.localScale *= Random.Range(treeScaleRange.x, treeScaleRange.y);
        }
    }
}
