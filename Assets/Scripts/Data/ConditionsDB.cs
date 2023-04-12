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
                OnAfterTurn = (Enemy enemy) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        enemy.CureStatus();
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} found an antidote in the grass and is no longer poisoned");
                    }
                    else
                    {
                        enemy.UpdateHP(enemy.MaxHp / 8);
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} took poison damage!");
                    }
                }
            }
        },
        {
            ConditionID.brn, new Condition()
            {
                Name = "Burn",
                StartMessage = "started burning!",
                OnAfterTurn = (Enemy enemy) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        enemy.CureStatus();
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} is no longer burning");
                    }
                    else
                    {
                        enemy.UpdateHP(enemy.MaxHp / 16);
                        enemy.StatusChanges.Enqueue($"{enemy.Base.Name} is burning away!");
                    }
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn
}
