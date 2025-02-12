using System.Collections;
using UnityEngine;
using TMPro;

public class ProgressTracker : MonoBehaviour
{
    public TextMeshProUGUI coinsText; // Assign this in the Inspector

    void Start()
    {
        int savedCoins = PlayerPrefs.GetInt("Coins", 0); // Get stored coins
        coinsText.text = "Coins: " + savedCoins.ToString(); // Display on UI
    }
}