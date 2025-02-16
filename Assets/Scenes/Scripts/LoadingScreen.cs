using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{

    public float rotationSpeed = 90f; // Speed of rotation

    void Update()
    {
        // Rotate the fox smoothly around the Y-axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

}
