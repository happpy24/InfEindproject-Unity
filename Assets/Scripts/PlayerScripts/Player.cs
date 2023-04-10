using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Vector2 origPos, targetPos;

    public bool isMoving = false;
    public float timeToMove = 0.2f;
    public string playerDirection = "down";
    public float battleChance;

    public Animator animator;
    private float animationHandler = 0.02f;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (!isMoving)
        {
            animationHandler -= Time.deltaTime;
            if (animationHandler < 0)
            {
                animator.SetBool("isMoving", false);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                animator.SetFloat("Horizontal", 1);
                animator.SetFloat("Vertical", 0);
                playerDirection = "up";
                StartCoroutine(MovePlayer(Vector2.up));
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", -1);
                playerDirection = "left";
                StartCoroutine(MovePlayer(Vector2.left));
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                animator.SetFloat("Horizontal", -1);
                animator.SetFloat("Vertical", 0);
                playerDirection = "down";
                StartCoroutine(MovePlayer(Vector2.down));
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", 1);
                playerDirection = "right";
                StartCoroutine(MovePlayer(Vector2.right));
            }
        }
    }

    private IEnumerator MovePlayer(Vector2 direction)
    {
        isMoving = true;
        animationHandler = 0.02f;
        animator.SetBool("isMoving", true);
        float elapsedTime = 0;
        origPos = transform.position;
        targetPos = origPos + Vector2.Scale(new Vector2(0.5f, 0.5f), direction);

        // Check if there is a blocking collider at the target position
        Collider2D hit = Physics2D.OverlapBox(targetPos, 0.25f * boxCollider.size, 0, LayerMask.GetMask("Blocking"));
        if (hit == null)
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
        CheckForEncounters();
    }

    private void CheckForEncounters()
    {
        Collider2D hit = Physics2D.OverlapBox(transform.position, 0.25f * boxCollider.size, 0, LayerMask.GetMask("BattleTiles"));
        if (hit != null)
        {
            if (Random.Range(1, 100) <= battleChance)
            {
                Debug.Log("Encounter Enemy");
            }
        }
    }
}