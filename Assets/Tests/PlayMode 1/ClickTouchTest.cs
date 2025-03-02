using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickTouchTest
{
    private GameObject buttonObject;
    private Button testButton;
    private bool buttonClicked;

    [SetUp]
    public void Setup()
    {
        // Create a new GameObject for the button
        buttonObject = new GameObject("TestButton");
        buttonObject.AddComponent<RectTransform>(); // Needed for UI elements
        testButton = buttonObject.AddComponent<Button>();

        // Add listener to track button click
        testButton.onClick.AddListener(() => buttonClicked = true);

        buttonClicked = false; // Reset click status
    }

    [UnityTest]
    public IEnumerator ButtonClickRespondsProperly()
    {
        // Simulate a click event
        testButton.onClick.Invoke();

        // Wait a frame to process UI event
        yield return null;

        // Check if button click was registered
        Assert.IsTrue(buttonClicked, "Button click was not registered.");
    }

    [UnityTest]
    public IEnumerator TouchInputRespondsProperly()
    {
        // Simulate a touch event
        PointerEventData pointerEvent = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width / 2, Screen.height / 2) // Simulating a touch at screen center
        };

        ExecuteEvents.Execute(testButton.gameObject, pointerEvent, ExecuteEvents.pointerClickHandler);

        // Wait a frame to process event
        yield return null;

        // Check if touch input triggered the button click
        Assert.IsTrue(buttonClicked, "Touch input did not trigger the button.");
    }
}
