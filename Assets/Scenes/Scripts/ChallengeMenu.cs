using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChallengeMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void goToChallenge1()
    {
        SceneManager.LoadScene(2);
    }
    public void goToChallenge2()
    {
        SceneManager.LoadScene(3);
    }
    public void goToChallenge3()
    {
        //SceneManager.LoadScene(2); 
    }
    public void goToChallenge4()
    {
        //SceneManager.LoadScene(2); 
    }
}
