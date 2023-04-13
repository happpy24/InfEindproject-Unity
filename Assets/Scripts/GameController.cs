using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Menu, Item, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] InventoryUI inventoryUI;

    [SerializeField] Animator battleAnimation;
    [SerializeField] Animator loadingAnimation;

    GameState state;

    GameState stateBeforePause;

    public static GameController Instance { get; private set; }
    public static Camera WorldCamera { get; set; }

    MenuController menuController;

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EnemyDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        battleAnimation.SetBool("StartBattle", false);
        battleAnimation.SetBool("EndBattle", false);
        loadingAnimation.SetBool("StartLoading", false);
        loadingAnimation.SetBool("EndLoading", false);
        state = GameState.Battle;
        StartCoroutine(OpeningAnimation());
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        StartCoroutine(EndingAnimation());
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
            // Info

        }
        else if (selectedItem == 1)
        {
            // Item
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Item;
        }
        else if (selectedItem == 2)
        {
            // Saving
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            // Loading
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 4)
        {
            // Options
        }
        else if (selectedItem == 5)
        {
            // Exit to Menu
        }
        else if (selectedItem == 6)
        {
            // Exit Game
        }
    }
}
