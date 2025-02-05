using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VideoPathManager
{

    public static Dictionary<string, string> level1 = new Dictionary<string, string>
    {
        { "Sample/Beautiful_002", "Beautiful" },
        { "Sample/Bad_004", "Bad" },
        { "Sample/Careful_001", "Careful" },
        { "Sample/Cold_001", "Cold" },
        {"Sample/Deaf_011", "Deaf"},
        {"Sample/Happy_001", "Happy"},
        {"Sample/Fat_001", "Fat"},
        {"Sample/Different_004", "Different"}
    };

    public static Dictionary<string, string> level2 = new Dictionary<string, string>
    {
        { "Sample/Beautiful_002", "Beautiful" },
        { "Sample/Bad_004", "Bad" },
        { "Sample/Careful_001", "Careful" },
        { "Sample/Cold_001", "Cold" },
        {"Sample/Deaf_011", "Deaf"},
        {"Sample/Happy_001", "Happy"},
        {"Sample/Fat_001", "Fat"},
        {"Sample/Different_004", "Different"}
    };
    public static Dictionary<string, string> GetVideoPaths()
    {
        int levelId = PlayerPrefs.GetInt("SelectedLevelId");
        Debug.Log(levelId);
        switch (levelId)
        {
            case 1:
                return level1;
                break;
            case 2:
                return level2;
                break;
            default:
                return level1;
                break;
        }
    }
}