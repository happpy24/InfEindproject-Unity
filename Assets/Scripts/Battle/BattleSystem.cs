using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Item, MoveToForget, BattleOver }
public enum BattleAction { Move, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState prevState;

    BattleAction action;

    int currentAction;
    int currentMove;
    bool failedRunning;

    BattlePlayer battlePlayer;
    Enemy wildEnemy;

    int escapeAttempts;
    MoveBase moveToLearn;

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

        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        battlePlayer.Enemys.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void OpenInventory()
    {
        state = BattleState.Item;
        inventoryUI.gameObject.SetActive(true);
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
        action = BattleAction.Move;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator ChooseMoveToForget(Enemy enemy, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(enemy.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (failedRunning)
        {
            failedRunning = false;
            dialogBox.EnableActionSelector(false);
            enemyUnit.Enemy.CurrentMove = enemyUnit.Enemy.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyUnit.Enemy.CurrentMove);
            yield return RunAfterTurn(playerUnit);
            yield return RunAfterTurn(enemyUnit);
        }
        else
        {
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
                yield return RunAfterTurn(firstUnit);
                if (state == BattleState.BattleOver) yield break;

                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Enemy.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
            else
            {
                if (playerAction == BattleAction.UseItem)
                {
                    dialogBox.EnableActionSelector(false);
                    failedRunning = true;
                    state = BattleState.RunningTurn;
                }
                else if (playerAction == BattleAction.Run)
                {
                    yield return TryToEscape();
                }
            }
        }
        

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Enemy.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Enemy);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Enemy);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Enemy.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Enemy, targetUnit.Enemy))
        {

            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(0.3f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Enemy, targetUnit.Enemy, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Enemy.TakeDamage(move, sourceUnit.Enemy);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Enemy.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Enemy, targetUnit.Enemy, secondary.Target);
                }
            }

            if (targetUnit.Enemy.HP <= 0)
            {
                yield return HandleEnemyFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"...but it missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Enemy source, Enemy target, MoveTarget moveTarget)
    {
        // Stat boosting (up attack, defence etc)
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
        }

        // Status Condition (poisoned/burning)
        if (effects.Status != ConditionID.none)
        {
            if (moveTarget == MoveTarget.Self)
                source.SetStatus(effects.Status);
            else
                target.SetStatus(effects.Status);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Check if pokemon gets status effect like posion/burn
        sourceUnit.Enemy.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Enemy);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Enemy.HP <= 0)
        {
            yield return HandleEnemyFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Enemy source, Enemy target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f};

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Enemy enemy)
    {
        if (enemy.StatusChanges != null)
        {
            while (enemy.StatusChanges.Count > 0)
            {
                var message = enemy.StatusChanges.Dequeue();
                yield return dialogBox.TypeDialog(message);
            }
        }
    }

    IEnumerator HandleEnemyFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Enemy.Base.Name} fainted.");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // EXP GAIN
            int expYield = faintedUnit.Enemy.Base.ExpYield;
            int enemyLevel = faintedUnit.Enemy.Level;
            // float bossBonus = (isBossBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * 1f) / 7);
            playerUnit.Enemy.Exp += expGain;
            yield return dialogBox.TypeDialog($"You won! {playerUnit.Enemy.Base.Name} gained {expGain} exp!");
            yield return playerUnit.Hud.SetExpSmooth();

            // CHECK LEVEL
            while (playerUnit.Enemy.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Enemy.Base.Name} leveled up to level {playerUnit.Enemy.Level}");

                var newMove = playerUnit.Enemy.GetLearnableMovesAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Enemy.Moves.Count < EnemyBase.MaxNumOfMoves)
                    {
                        playerUnit.Enemy.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Enemy.Base.Name} learned {newMove.Base.Name}!");
                        dialogBox.SetMoveNames(playerUnit.Enemy.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Enemy.Base.Name} is trying to learn {newMove.Base.Name}...");
                        yield return dialogBox.TypeDialog($"... but it cannot learn more than {EnemyBase.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Enemy, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }


            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
        else if (state == BattleState.Item)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };
            Action onItemUsed = () =>
            {
                state = BattleState.Busy;
                inventoryUI.gameObject.SetActive(false);
                failedRunning = true;
                StartCoroutine(RunTurns(BattleAction.UseItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == EnemyBase.MaxNumOfMoves)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Enemy.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    var selectedMove = playerUnit.Enemy.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Enemy.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));

                    playerUnit.Enemy.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2)
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
                prevState = state;
                action = BattleAction.Move;
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                prevState = state;
                OpenInventory();
            }
            else if (currentAction == 2)
            {
                state = BattleState.Busy;
                prevState = state;
                StartCoroutine(RunTurns(BattleAction.Run));
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
            var move = playerUnit.Enemy.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
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
                failedRunning = true;
                state = BattleState.RunningTurn;
            }
        }
    }
}
