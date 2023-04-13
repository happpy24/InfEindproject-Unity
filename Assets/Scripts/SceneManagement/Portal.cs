using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 cameraMaxPos;
    [SerializeField] Vector2 cameraMinPos;

    Player player;
    GameplayCamera cam;
    Fader fader;

    public void OnPlayerTriggered(Player player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }
    
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.2f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnaptoTile(destPortal.SpawnPoint.position);

        // Error on line down here
        cam = FindAnyObjectByType<GameplayCamera>();
        cam.MaxPos = cameraMaxPos;
        cam.MinPos = cameraMinPos;

        yield return new WaitForSeconds(1f);
        yield return fader.FadeOut(0.3f);
        GameController.Instance.PauseGame(false);

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier { A, B, C, D, E, F };
