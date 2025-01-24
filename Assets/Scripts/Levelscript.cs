using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levelscript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Get all LevelData components in children (only the level images)
        LevelData[] levelObjects = GetComponentsInChildren<LevelData>();

        int id = 1; // Starting level ID

        // Loop through each level object and assign levelId
        foreach (LevelData levelData in levelObjects)
            if (levelData.levelId == 0)
            {
                {
                    levelData.levelId = id; // Manually assign the level ID
                    id++;  // Increment level ID for the next level
                }
            }
    }
}
