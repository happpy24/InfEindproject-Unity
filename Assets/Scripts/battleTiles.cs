using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class battleTiles : MonoBehaviour
{
    public Player player;

    private void FixedUpdate()
    {
        // Check for collisions with all TilemapCollider2D objects
        foreach (var tilemapCollider in FindObjectsOfType<TilemapCollider2D>())
        {
            Collider2D hit = Physics2D.OverlapBox(player.transform.position, 0.5f * player.boxCollider.size, 0, LayerMask.GetMask("Player"), tilemapCollider.gameObject.layer);
            if (hit != null)
            {
                Debug.Log("Player collided with battle tile");
                break; // Exit the loop if a collision was found
            }
        }
    }
}