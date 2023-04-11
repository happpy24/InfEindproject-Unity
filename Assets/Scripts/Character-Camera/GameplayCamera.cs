using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float smoothing;

    [SerializeField] Vector2 maxPos;
    [SerializeField] Vector2 minPos;

    private Scene scene;

    private void Start()
    {
        scene = SceneManager.GetActiveScene();
        Debug.Log("Current Scene: " + scene.name);
    }

    private void FixedUpdate()
    {
        if (transform.position != player.position)
        {
            Vector3 targetPos = new Vector3 (player.position.x, player.position.y, transform.position.z);

            targetPos.x = Mathf.Clamp(targetPos.x, minPos.x, maxPos.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minPos.y, maxPos.y);

            transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
        }
    }
}
