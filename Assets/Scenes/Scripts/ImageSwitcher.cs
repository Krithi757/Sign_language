using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Image[] images; // Array to hold images
    private int currentIndex = 0; // Keep track of current image

    public Button nextButton;
    public Button prevButton;

    void Start()
    {
        ShowImage(currentIndex); // Show the first image at start

        // Add button click event listeners
        nextButton.onClick.AddListener(NextImage);
        prevButton.onClick.AddListener(PreviousImage);
    }

    public void NextImage()
    {
        if (currentIndex < images.Length - 1) // Check if not the last image
        {
            currentIndex++; // Move to next image
            ShowImage(currentIndex);
        }
    }

    public void PreviousImage()
    {
        if (currentIndex > 0) // Check if not the first image
        {
            currentIndex--; // Move to previous image
            ShowImage(currentIndex);
        }
    }

    void ShowImage(int index)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(i == index); // Only show current image
        }
    }
}
