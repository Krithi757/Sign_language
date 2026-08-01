using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    // ⚠️ These constants haven't been kept in sync with your actual Build
    // Settings list for a while — your screenshot shows Scenes/LoadingScreen
    // at index 0, Home at 1, etc., which doesn't line up 1:1 with the values
    // below. I've only corrected challengeFeedback (confirmed = 5 in your
    // screenshot) since that's the one causing the current bug. If anything
    // in your project actually reads home/levelview/challenge1/etc. from
    // here, double check each value against Build Settings before trusting it.
    public static int home = 0;
    public static int levelview = 1;
    public static int challenge1 = 2;
    public static int challengeRunning = 3;
    public static int challengeMenu = 4;
    public static int challenge2 = 5;
    public static int challengeFeedback = 5;
    public static int progress = 7;
    public static int newLevel = 8;
}