using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with the collider attached to this game object!");
        }
    }
}