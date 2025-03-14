using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject helpPanel;

    void Start()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    public void ShowHelpPanel()
    {
        if (helpPanel != null)
            helpPanel.SetActive(true);
    }

    public void CloseHelpPanel()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }
}
