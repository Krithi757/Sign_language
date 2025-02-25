using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttonscript : MonoBehaviour
{
    public void goToLearn()
    {

    }
    public void goToChallenge()
    {
        SceneManager.LoadScene(4);
    }
    public void goToPractice()
    {
        //SceneManager.LoadScene();
    }
}
