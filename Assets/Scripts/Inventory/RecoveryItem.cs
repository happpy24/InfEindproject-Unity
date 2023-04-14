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
        if (hpAmount > 0)
        {
            if (enemy.HP == enemy.MaxHp)
                return false;
            
            enemy.IncreaseHP(hpAmount);
        }

        return true;
    }
}
