using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(20 * Time.deltaTime, 0, 0);

    }


    // private void onTriggerEnter(Collider other)
    // {
    //     Debug.Log("Coin: " + Running_challenge.numberOfCoins);
    //     if (other.tag == "Player")
    //     {
    //         Running_challenge.numberOfCoins += 1;
    //         Debug.Log("Coin: " + Running_challenge.numberOfCoins);
    //         Destroy(gameObject);
    //     }
    // }
}
