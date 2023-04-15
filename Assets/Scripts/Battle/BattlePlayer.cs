using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlePlayer : MonoBehaviour
{
    [SerializeField] List<Enemy> enemies;

    public event Action OnUpdated;

    public List<Enemy> Enemys
    {
        get {
            return enemies;
        }
        set
        {
            enemies = value;
        }
    }

    private void Start()
    {
        foreach (var enemy in enemies)
        {
            enemy.Init();
        }
    }

    public void BattlePlayerUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static BattlePlayer GetBattlePlayer()
    {
        return FindObjectOfType<Player>().GetComponent<BattlePlayer>();
    }

    public Enemy GetHealthyEnemy()
    {
        return enemies.Where(x => x.HP > 0).FirstOrDefault();
    }
}
