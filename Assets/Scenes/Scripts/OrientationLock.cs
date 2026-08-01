using UnityEngine;

// Drop this on any GameObject in a scene (an empty "OrientationLock" object
// works fine) and pick the orientation THIS scene needs. Runs in Awake —
// before anything on screen renders — so the device rotates as early as
// possible, ideally while your LoadingScreen scene is still covering it,
// instead of the player seeing the already-loaded scene visibly flip.
//
// Put one of these in EVERY scene:
//   - The 14 portrait scenes (Home, Challenge1, ChallengeMenu, ChallengeFeedback,
//     Progresspage, NewLevel, LearningMode, PlayerId, Shop_Store1/2/3,
//     Alphabet, ChallengeFalppy, LoadingScreen) -> orientation = Portrait
//   - CookingGame -> orientation = LandscapeLeft (or LandscapeRight, whichever
//     matches how you built the landscape UI)
//
// Requires Project Settings -> Player -> Resolution and Presentation ->
// Default Orientation = "Auto Rotation", with Portrait + the Landscape
// direction(s) you use checked under Allowed Orientations. If Default
// Orientation is left on a single fixed value instead, some devices will
// ignore Screen.orientation changes from script.
public class OrientationLock : MonoBehaviour
{
    public enum LockMode { Portrait, LandscapeLeft, LandscapeRight }

    [Tooltip("The orientation THIS scene should be locked to.")]
    public LockMode orientation = LockMode.Portrait;

    void Awake()
    {
        switch (orientation)
        {
            case LockMode.Portrait:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case LockMode.LandscapeLeft:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
            case LockMode.LandscapeRight:
                Screen.orientation = ScreenOrientation.LandscapeRight;
                break;
        }
    }
}
