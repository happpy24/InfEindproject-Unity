using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, IPlayerTriggable
{
    public void OnPlayerTriggered(Player player)
    {
        Debug.Log("Player entered the portal!");
    }
}
