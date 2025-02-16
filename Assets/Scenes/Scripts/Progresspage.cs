using System.Collections;
using UnityEngine;
using TMPro;

public class ProgressTracker : MonoBehaviour
{
    public TextMeshProUGUI coinsText; // Assign this in the Inspector
    public TextMeshProUGUI diamondsText; // Assign in Inspector


    void Start()
    {
        int savedCoins = PlayerPrefs.GetInt("Coins", 0); // Get stored coins
        int savedDiamonds = PlayerPrefs.GetInt("Diamond", 0); // Retrieve stored diamonds

        coinsText.text = "Coins: " + savedCoins.ToString(); // Display on UI
        diamondsText.text = "Diamonds: " + savedDiamonds.ToString(); // Display diamonds
    }
}