using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, BattleOver }
public enum BattleAction { Move , UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    BattlePlayer battlePlayer;
    Enemy wildEnemy;

    int escapeAttempts;

    public void StartBattle(BattlePlayer battlePlayer, Enemy wildEnemy)
    {
        this.battlePlayer = battlePlayer;
        this.wildEnemy = wildEnemy;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(battlePlayer.GetHealthyEnemy());
        enemyUnit.Setup(wildEnemy);

        dialogBox.SetMoveNames(playerUnit.Enemy.Moves);

        yield return dialogBox.TypeDialog($"You encountered a {enemyUnit.Enemy.Base.Name}!");
        yield return new WaitForSeconds(0.5f);

        escapeAttempts = 0;
        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Enemy.Speed >= enemyUnit.Enemy.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        battlePlayer.Enemys.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(dialogBox.TypeDialog("Choose an Action"));
        dialogBox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Enemy.CurrentMove = playerUnit.Enemy.Moves[currentMove];
            enemyUnit.Enemy.CurrentMove = enemyUnit.Enemy.GetRandomMove();

            // Check who goes first
            bool playerGoesFirst = playerUnit.Enemy.Speed >= enemyUnit.Enemy.Speed;
            
            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Enemy.CurrentMove);
            if (state == BattleState.BattleOver) yield break;

            // Second Turn
            yield return RunMove(secondUnit, firstUnit, secondUnit.Enemy.CurrentMove);
            if (state == BattleState.BattleOver) yield break;
        }
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.RunningTurn;

        var move = playerUnit.Enemy.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.RunningTurn)
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.RunningTurn;

        var move = enemyUnit.Enemy.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.RunningTurn)
            ActionSelection();
    }


    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Enemy.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.3f);
        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Enemy, targetUnit.Enemy);
        }
        else
        {
            var damageDetails = targetUnit.Enemy.TakeDamage(move, sourceUnit.Enemy);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (targetUnit.Enemy.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Enemy.Base.Name} fainted.");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }

        // Check if pokemon gets status effect like posion/burn
        sourceUnit.Enemy.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Enemy);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Enemy.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Enemy.Base.Name} fainted.");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Enemy source, Enemy target)
    {
        var effects = move.Base.Effects;

        // Stat boosting (up attack, defence etc)
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
        }

        // Status Condition (poisoned/burning)
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Enemy enemy)
    {
        while (enemy.StatusChanges.Count > 0)
        {
            var message = enemy.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            BattleOver(false);
        }
        else
            BattleOver(true);
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("It's a critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective.");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                StartCoroutine(TryToEscape());
            }
        }
    }
    
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Enemy.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Enemy.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Enemy.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        ++escapeAttempts;

        int playerSpeed = playerUnit.Enemy.Speed;
        int enemySpeed = enemyUnit.Enemy.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"You failed to flee the scene!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
