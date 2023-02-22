using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    private RaycastHit2D hit;

    private bool isMoving;

    private Vector2 origPos, targetPos;

    private float timeToMove = 0.1f;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W) && !isMoving)
            StartCoroutine(MovePlayer(Vector2.up));

        if (Input.GetKey(KeyCode.A) && !isMoving)
            StartCoroutine(MovePlayer(Vector2.left));

        if (Input.GetKey(KeyCode.S) && !isMoving)
            StartCoroutine(MovePlayer(Vector2.down));

        if (Input.GetKey(KeyCode.D) && !isMoving)
            StartCoroutine(MovePlayer(Vector2.right));
    }

    private IEnumerator MovePlayer(Vector2 direction)
    {
        isMoving = true;

        float elapsedTime = 0;

        origPos = transform.position;

        targetPos = origPos + Vector2.Scale(new Vector2((float)0.48, (float)0.48), direction);

        // if (targetPos != boxCollider.size LayerMask.GetMask("Actor", "Blocking")
            
        hit = Physics2D.BoxCast(origPos, boxCollider.size, 0, targetPos, 1, LayerMask.GetMask("Actor", "Blocking"));

        // Debug.Log(hit.collider);

        if (hit.collider == null)
        {
            while (elapsedTime < timeToMove)
            {
                transform.position = Vector2.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
        }

        isMoving = false;
    }
}