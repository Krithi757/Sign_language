using UnityEngine;

public class UnlockFeature : MonoBehaviour
{
    public GameObject popupPanel; // Assign the Popup Panel in Inspector

    public void ShowPopup()
    {
        popupPanel.SetActive(true); // Show the pop-up message
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false); // Hide the pop-up message
    }
}
