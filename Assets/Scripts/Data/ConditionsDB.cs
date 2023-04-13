using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn, new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Enemy enemy) =>
                {
                    enemy.UpdateHP(enemy.MaxHp / 8);
                    enemy.StatusChanges.Enqueue($"{enemy.Base.Name} took poison damage!");
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
                    enemy.UpdateHP(enemy.MaxHp / 16);
                    enemy.StatusChanges.Enqueue($"{enemy.Base.Name} is burning away!");
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn
}
