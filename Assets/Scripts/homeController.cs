using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class homeCont : MonoBehaviour
{
    public GameObject music;
    public GameObject musicDisabled;
    public GameObject soundEffectDisabled;
    public GameObject soundEffects;
    // Start is called before the first frame update
    void Start()
    {
        music.SetActive(false);
        soundEffects.SetActive(false);
        musicDisabled.SetActive(false);
        soundEffectDisabled.SetActive(false);
        Debug.Log("Script has strated");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void clickedSettings()
    {
        music.SetActive(true);
        soundEffects.SetActive(true);
        musicDisabled.SetActive(false);
        soundEffectDisabled.SetActive(false);
    }

    public void gotoLevel()
    {
        SceneManager.LoadScene(SceneData.levelview);
    }
    public void gotoProgress()
    {
        SceneManager.LoadScene(SceneData.progress);
    }
    public void gotoLearning()
    {
        //SceneManager.LoadScene(SceneData);
    }
    public void gotoChallenge()
    {
        SceneManager.LoadScene(SceneData.challengeMenu);
    }
    public void gotoPractice()
    {
        //SceneManager.LoadScene(SceneData);
    }
}
