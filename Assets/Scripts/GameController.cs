using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Menu, MainMenu, Info, Item, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] InventoryUI inventoryUI;

    [SerializeField] Animator battleAnimation;
    [SerializeField] Animator loadingAnimation;

    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioSource musicSaver;

    AudioClip prevMusic;

    GameState state;

    GameState prevState;

    public static GameController Instance { get; private set; }
    public static Camera WorldCamera { get; set; }

    Healer healer;
    MenuController menuController;

    private void Awake()
    {
        Instance = this;

        healer = new Healer();
        menuController = GetComponent<MenuController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EnemyDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = prevState;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
<<<<<<< HEAD

        mainMenu.onBack += () =>
        {
            state = GameState.FreeRoam;
            StartCoroutine(MainMenuHandler());
        };

        gameOver.onBack += () =>
        {
            var battlePlayer = player.GetComponent<BattlePlayer>();
            battlePlayer.Enemys.ForEach(p => p.Heal());
            battlePlayer.Enemys.ForEach(p => p.Moves.ForEach(m => m.PPHEAL()));
            battlePlayer.BattlePlayerUpdated();
            Vector3 newPlayerPos = healer.SavedLocation;
            StartCoroutine(GameOverBack());
            state = GameState.FreeRoam;
            if (newPlayerPos != null)
            {
                player.transform.position = newPlayerPos;

            }
            else
            {
                player.transform.position = new Vector3(-2, -1);
            }
        };
=======
>>>>>>> parent of 3b6bc7d5 (unity man. its over.)
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartBattle()
    {
        prevMusic = musicSaver.clip;
        battleAnimation.SetBool("StartBattle", false);
        battleAnimation.SetBool("EndBattle", false);
        loadingAnimation.SetBool("StartLoading", false);
        loadingAnimation.SetBool("EndLoading", false);
        state = GameState.Battle;
        AudioManager.i.PlayMusic(wildBattleMusic);
        StartCoroutine(OpeningAnimation());
    }

    void EndBattle(bool won)
    {
<<<<<<< HEAD
        if (won)
        {
            AudioManager.i.PlayMusic(prevMusic);
            state = GameState.FreeRoam;
            StartCoroutine(EndingAnimation());
        }
        else
        {
            state = GameState.GameOver;
            StartCoroutine(GameOverHandler());
        }

=======
        AudioManager.i.PlayMusic(prevMusic);
        state = GameState.FreeRoam;
        StartCoroutine(EndingAnimation());
>>>>>>> parent of 3b6bc7d5 (unity man. its over.)
    }

    private IEnumerator OpeningAnimation()
    {
        battleAnimation.SetBool("StartBattle", true);
        yield return new WaitForSeconds(1f);
        battleAnimation.SetBool("EndBattle", true);
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var battlePlayer = player.GetComponent<BattlePlayer>();
        var wildEnemy = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildEnemy();

        battleSystem.StartBattle(battlePlayer, wildEnemy);
    }

    private IEnumerator EndingAnimation()
    {
        loadingAnimation.SetBool("StartLoading", true);
        yield return new WaitForSeconds(0.16f);
        loadingAnimation.SetBool("EndLoading", true);
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }


    private IEnumerator GameOverBack()
    {
        loadingAnimation.SetBool("StartLoading", true);
        yield return new WaitForSeconds(0.4f);
        yield return SceneManager.LoadSceneAsync(healer.SavedScene);
        var cam = FindAnyObjectByType<GameplayCamera>();
        cam.MaxPos = healer.savedCamMax;
        cam.MinPos = healer.savedCamMin;
        gameOver.Menu.gameObject.SetActive(false);
        loadingAnimation.SetBool("EndLoading", true);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            player.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.C))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.Item)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
                menuController.OpenMenu();
                state = GameState.Menu;
            };

            inventoryUI.HandleUpdate(onBack);
        }
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // Item
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Item;
        }
        else if (selectedItem == 1)
        {
            // OPTIONS
        }
        else if (selectedItem == 2)
        {
            // Quit
            Debug.Log("QUIT GAME");
            Application.Quit();
        }
    }
}
