using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Vector2 origPos, targetPos;

    public bool isMoving = false;

    public float timeToMove = 0.15f;
    public float battleChance = 0.1f;

    public Animator transition;
    public float transitionTime = 1f;

    public Animator animator;
    private float animationHandler = 0.02f;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
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
                StartCoroutine(MovePlayer(Vector2.up));
            } 
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", -1);
                StartCoroutine(MovePlayer(Vector2.left));
            } 
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                animator.SetFloat("Horizontal", -1);
                animator.SetFloat("Vertical", 0);
                StartCoroutine(MovePlayer(Vector2.down));
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", 1);
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
        targetPos = origPos + Vector2.Scale(new Vector2(1, 1), direction);

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