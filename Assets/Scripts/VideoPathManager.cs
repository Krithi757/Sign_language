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
        { "Level2/Again_005", "Again" },
        { "Level2/Also_008", "Also" },
        { "Level2/Can_008", "Can" },
        { "Level2/Cant_003", "Can't" },
        {"Level2/Clearly_001", "Clearly"},
        {"Sample/Happy_001", "Happy"},
        {"Sample/Fat_001", "Fat"},
        {"Sample/Different_004", "Different"}
    };

    public static Dictionary<string, string> level3 = new Dictionary<string, string>
    {
        { "Level3/Black_008", "Black" },
        { "Level3/Blue_002", "Blue" },
        { "Level3/Brown_002", "Brown" },
        { "Level3/Gold_002", "Gold" },
        {"Level3/Green_010", "Green"},
        {"Level3/Gray_001", "Gray"},
        {"Level3/Orange_003", "Orange"},
        {"Level3/Pink_004", "Pink"},
        {"Level3/Purple_028", "Purple"},
        {"Level3/Red_028", "Red"},
        {"Level3/White_048", "White"},
        {"Level3/Yellow_047", "Yellow"}
    };

    public static Dictionary<string, string> level4 = new Dictionary<string, string>
    {
        { "Level4/Friday_007", "Friday" },
        { "Level4/Monday_023", "Monday" },
        { "Level4/Thursday_001", "Thursday" },
        { "Level4/Today_036", "Today" },
        {"Level4/Tomorrow_008", "Tomorrow"},
        {"Level4/Tuesday_005", "Tuesday"},
        {"Level4/Wednesday_019", "Wednesday"},
        {"Level4/Yesterday_015", "Yesterday"},
        {"Level3/Yellow_047", "Yellow"}
    };

    public static Dictionary<string, string> level5 = new Dictionary<string, string>
    {
        { "Level5/Alright_008", "Alright" },
        { "Level5/August_008", "August" },
        { "Level5/Ayubowan_001", "Ayubowan" },
        { "Level5/Hello_003", "Hello" },
        {"Level5/How are you_001", "How are you"},
        {"Level5/Thank you_016", "Thank you"},
        {"Level4/Wednesday_019", "Wednesday"},
        {"Level4/Yesterday_015", "Yesterday"},
        {"Level3/Yellow_047", "Yellow"}
    };

    public static Dictionary<string, string> level6 = new Dictionary<string, string>
    {
        { "Level6/April_008", "April" },
        { "Level5/August_008", "August" },
        { "Level5/Ayubowan_001", "Ayubowan" },
        { "Level5/Hello_003", "Hello" },
        {"Level5/How are you_001", "How are you"},
        {"Level5/Thank you_016", "Thank you"},
        {"Level4/Wednesday_019", "Wednesday"},
        {"Level4/Yesterday_015", "Yesterday"},
        {"Level3/Yellow_047", "Yellow"}
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
            case 3:
                return level3;
                break;
            case 4:
                return level4;
                break;
            case 5:
                return level5;
                break;
            case 6:
                return level6;
                break;
            default:
                return level1;
                break;
        }
    }
}