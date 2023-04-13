using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDB : MonoBehaviour
{
    static Dictionary<string, EnemyBase> enemys;

    public static void Init()
    {
        enemys = new Dictionary<string, EnemyBase>();

        var enemyArray = Resources.LoadAll<EnemyBase>("");
        foreach (var enemy in enemyArray)
        {
            if (enemys.ContainsKey(enemy.Name))
            {
                Debug.LogError($"There are two enemies with the name {enemy.Name}");
                continue;
            }

            enemys[enemy.Name] = enemy;
        }
    }

    public static EnemyBase GetEnemyByName(string name)
    {
        if (!enemys.ContainsKey(name))
        {
            Debug.LogError($"Enemy with name {name} was not found in database");
            return null;
        }

        return enemys[name];
    }
}
