using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { MainMenu, FreeRoam, Battle, Dialog, Menu, GameOver, Info, Item, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] InventoryUI inventoryUI;

    [SerializeField] Animator battleAnimation;
    [SerializeField] Animator loadingAnimation;

    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip gameOverMusic;
    [SerializeField] AudioSource musicSaver;

    Healer healer;

    AudioClip prevMusic;

    GameState state;

    GameState prevState;

    public static GameController Instance { get; private set; }
    public static Camera WorldCamera { get; set; }

    MenuController menuController;
    GameOver gameOver;
    MainMenu mainMenu;

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();
        gameOver = GetComponent<GameOver>();
        mainMenu = GetComponent<MainMenu>();

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

        mainMenu.onBack += () =>
        {
            state = GameState.FreeRoam;
            StartCoroutine(MainMenuHandler());
        };

        gameOver.onBack += () =>
        {
            if (healer.savedLocation != null)
            {
                player.transform.position = healer.savedLocation;
            }
            else
            {
                player.transform.position = 
            }
        };
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

    private IEnumerator GameOverHandler()
    {
        loadingAnimation.SetBool("StartLoading", true);
        yield return new WaitForSeconds(0.16f);
        battleSystem.gameObject.SetActive(false);
        gameOver.Menu.SetActive(true);
        worldCamera.gameObject.SetActive(true);
        AudioManager.i.PlayMusic(gameOverMusic, false, true);
        loadingAnimation.SetBool("EndLoading", true);
    }

    private IEnumerator MainMenuHandler()
    {
        loadingAnimation.SetBool("StartLoading", true);
        yield return new WaitForSeconds(0.16f);
        mainMenu.Menu.gameObject.SetActive(false);
        loadingAnimation.SetBool("EndLoading", true);
    }

    private void Update()
    {
        if (state == GameState.MainMenu)
        {
            mainMenu.HandleUpdate();
        }
        else if (state == GameState.FreeRoam)
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
        else if (state == GameState.GameOver)
        {
            gameOver.HandleUpdate();
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
            // Saving
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 2)
        {
            // Loading
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            // OPTIONS
        }
        else if (selectedItem == 4)
        {
            // Mainmenu
            state = GameState.MainMenu;
        }
        else if (selectedItem == 5)
        {
            // Quit
            Debug.Log("QUIT GAME");
            Application.Quit();
        }
    }
}
