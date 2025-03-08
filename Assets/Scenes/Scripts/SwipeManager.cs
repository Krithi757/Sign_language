using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{
    public static bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;
    private bool isDraging = false;
    private Vector2 startTouch, swipeDelta;

    // Reduce the threshold to make swipe detection more sensitive
    public float swipeThreshold = 5f;  // Lower threshold for more sensitive detection

    private void Update()
    {
        // Reset all swipe values to false
        tap = swipeDown = swipeUp = swipeLeft = swipeRight = false;

        #region Standalone Inputs
        if (Input.GetMouseButtonDown(0))
        {
            tap = true;
            isDraging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraging = false;
            Reset();  // Reset immediately after touch ends
        }
        #endregion

        #region Mobile Input
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                tap = true;
                isDraging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                isDraging = false;
                Reset();  // Reset immediately after touch ends
            }
        }
        #endregion

        // Calculate the swipe distance
        swipeDelta = Vector2.zero;
        if (isDraging)
        {
            if (Input.touches.Length > 0)
                swipeDelta = Input.touches[0].position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
        }

        // Respond immediately to any significant swipe
        if (swipeDelta.magnitude > swipeThreshold)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // Left or Right
                if (x < 0)
                    swipeLeft = true;
                else
                    swipeRight = true;
            }
            else
            {
                // Up or Down
                if (y < 0)
                    swipeDown = true;
                else
                    swipeUp = true;
            }

            Reset();  // Reset immediately after swipe is detected
        }
        else
        {
            // Handle smaller swipes immediately
            if (swipeDelta.x != 0)
            {
                if (swipeDelta.x > 0)
                    swipeRight = true;
                else if (swipeDelta.x < 0)
                    swipeLeft = true;
            }

            if (swipeDelta.y != 0)
            {
                if (swipeDelta.y > 0)
                    swipeUp = true;
                else if (swipeDelta.y < 0)
                    swipeDown = true;
            }
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDraging = false;
    }
}
