using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn, new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnStart = (Enemy enemy) =>
                {
                    enemy.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be poisoned for {enemy.StatusTime} moves");
                },
                OnBeforeMove = (Enemy enemy) =>
                {
                    if (enemy.StatusTime <= 0)
                    {
                        enemy.CureStatus();
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} found an antidote in the grass and is no longer poisoned");
                        return true;
                    }
                    enemy.StatusTime--;
                    enemy.DecreaseHP(enemy.MaxHp / 8);
                    enemy.StatusChanges.Enqueue($"{enemy.Base.Name} took poison damage!");
                    return false;
                }
            }
        },
        {
            ConditionID.brn, new Condition()
            {
                Name = "Burn",
                StartMessage = "started burning!",
                OnBeforeMove = (Enemy enemy) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        enemy.CureStatus();
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} is no longer burning");
                        return true;
                    }
                    else
                    {
                        enemy.DecreaseHP(enemy.MaxHp / 16);
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} is burning away!");
                        return false;
                    }
                }
            }
        },
        {
            ConditionID.par, new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been Paralyzed!",
                OnBeforeMove = (Enemy enemy) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} is paralyzed and can't move!");
                        return false; 
                    }

                    return true;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, par
}
