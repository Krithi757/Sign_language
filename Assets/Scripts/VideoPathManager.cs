using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VideoPathManager
{

    public static string select = "level1";
    public static Dictionary<string, string> level1 = new Dictionary<string, string>
    {
        { "Sample/Beautiful_002", "Beautiful" },
        { "Sample/Bad_004", "Bad" },
        { "Sample/Careful_001", "Careful" },
        { "Sample/Cold_001", "Cold" }
    };

    public static Dictionary<string, string> GetVideoPaths()
    {
        switch (select)
        {
            case "level1":
                return level1;
            default:
                return new Dictionary<string, string>(); // Return an empty dictionary if level is not found
        }
    }
}