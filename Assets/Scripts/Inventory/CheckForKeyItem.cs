using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForKeyItem : MonoBehaviour
{
    Player player;

    List<ItemSlot> Slots;
    Portal portal;

    public void Update()
    {
        foreach (ItemSlot slot in Slots)
        {
            if (slot.Item.Name == "Dusty Book")
            {
                if (portal.name == "Portal A")
                    gameObject.SetActive(false);
                else
                    gameObject.SetActive(true);
            }
            else
            {
                if (portal.name == "Portal A")
                    gameObject.SetActive(true);
                else
                    gameObject.SetActive(false);
            }
        }
    }
}
