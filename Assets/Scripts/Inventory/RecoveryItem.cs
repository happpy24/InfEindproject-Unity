using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("CONDITIONS")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    public override bool Use(Enemy enemy)
    {
        // Restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (enemy.HP == enemy.MaxHp)
                return false;

            // Fully restore if MaxHP
            if (restoreMaxHP)
                enemy.IncreaseHP(enemy.MaxHp);
            else
                enemy.IncreaseHP(hpAmount);
        }

        // Recover Status
        if (recoverAllStatus || status != ConditionID.none)
        {
            if (enemy.Status == null)
                return false;

            if (recoverAllStatus)
            {
                enemy.CureStatus();
            }
            else
            {
                if (enemy.Status.Id == status)
                    enemy.CureStatus();
                else
                    return false;
            }
        }

        // Recover PP
        if (restoreMaxPP)
        {
            enemy.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount > 0)
        {
            enemy.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
