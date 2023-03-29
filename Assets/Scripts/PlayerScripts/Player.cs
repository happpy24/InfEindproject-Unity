using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private bool isMoving;
    private Vector2 origPos, targetPos;

    public float timeToMove = 0.15f;
    public float battleChance = 0.1f;

    public Animator transition;
    public float transitionTime = 1f;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!isMoving)
        {
            if (Input.GetKey(KeyCode.UpArrow))
                StartCoroutine(MovePlayer(Vector2.up));

            else if (Input.GetKey(KeyCode.LeftArrow))
                StartCoroutine(MovePlayer(Vector2.left));

            else if (Input.GetKey(KeyCode.DownArrow))
                StartCoroutine(MovePlayer(Vector2.down));

            else if (Input.GetKey(KeyCode.RightArrow))
                StartCoroutine(MovePlayer(Vector2.right));
        }
    }

    private IEnumerator MovePlayer(Vector2 direction)
    {
        isMoving = true;
        float elapsedTime = 0;
        origPos = transform.position;
        targetPos = origPos + Vector2.Scale(new Vector2((float)0.48, (float)0.48), direction);

        // Check if there is a blocking collider at the target position
        Collider2D hit = Physics2D.OverlapBox(targetPos, 0.5f * boxCollider.size, 0, LayerMask.GetMask("Blocking"));
        if (hit == null)
        {
            // Check if there is a battle trigger at the target position
            hit = Physics2D.OverlapBox(targetPos, 0.5f * boxCollider.size, 0, LayerMask.GetMask("BattleTrigger"));
            if (hit != null && Random.value <= battleChance)
            {
                transition.SetTrigger("StartBattle");

                yield return new WaitForSeconds(transitionTime);

                SceneManager.LoadScene("BattleScene");
            }
            else
            {
                while (elapsedTime < timeToMove)
                {
                    transform.position = Vector2.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                transform.position = targetPos;
            }
        }
        isMoving = false;
    }
}