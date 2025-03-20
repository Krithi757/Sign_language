using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{
    public static bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;
    private bool isDraging = false;
    private Vector2 startTouch, swipeDelta;
    private const float SWIPE_THRESHOLD = 1f; // Reduced threshold for faster response

    private void Update()
    {
        tap = swipeDown = swipeUp = swipeLeft = swipeRight = false;

        // Standalone Inputs
        if (Input.GetMouseButtonDown(0))
        {
            tap = true;
            isDraging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraging = false;
            Reset();
        }

        // Mobile Inputs
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                tap = true;
                isDraging = true;
                startTouch = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDraging = false;
                Reset();
            }
        }

        // Calculate swipe distance
        swipeDelta = Vector2.zero;
        if (isDraging)
        {
            if (Input.touchCount > 0)
                swipeDelta = Input.GetTouch(0).position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
        }

        // Detect swipe direction if it crosses the threshold
        if (swipeDelta.magnitude > SWIPE_THRESHOLD)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                swipeLeft = x < 0;
                swipeRight = x > 0;
            }
            else
            {
                swipeDown = y < 0;
                swipeUp = y > 0;
            }

            Reset();
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDraging = false;
    }
}
