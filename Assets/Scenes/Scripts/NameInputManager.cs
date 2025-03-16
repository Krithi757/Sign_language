using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;  // For scene management

public class NameInputManager : MonoBehaviour
{
    public TMP_InputField nameInputField;  // Reference to the TMP_InputField
    public Button submitButton;            // Reference to the Submit Button
    public TextMeshProUGUI greetingText;          // Reference to the TMP_Text for greeting

    void Start()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("LoadingMusic"); // Play sound only once
        }
        greetingText.gameObject.SetActive(false);
        // Check if a name is already saved
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            // If a name is already stored, we can use it later or load it as needed
            string savedName = PlayerPrefs.GetString("PlayerName");
            Debug.Log("Player Name Loaded: " + savedName);
        }

        // Add listener to submit button
        submitButton.onClick.AddListener(SavePlayerName);
    }

    // Save player name when the button is clicked
    void SavePlayerName()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            string playerName = nameInputField.text;
            // Save the player name using PlayerPrefs
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();  // Make sure to save the data

            Debug.Log("Player Name Saved: " + playerName);

            // Display greeting text
            greetingText.text = "Hello, " + playerName + "!";
            greetingText.gameObject.SetActive(true);

            // Optionally, load the next scene here after a delay or immediately
            Invoke("ChangeScene", 0.3f);  // Change scene after 2 seconds (optional delay)
        }
        else
        {
            Debug.LogWarning("Name cannot be empty!");
        }
    }

    // Method to change the scene
    void ChangeScene()
    {
        SceneManager.LoadScene(7);  // Change "Scene2" to your actual scene name
    }
}
