using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class homeCont : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Script has strated");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void gotoLevel()
    {
        SceneManager.LoadScene(SceneData.levelview);
    }
}
