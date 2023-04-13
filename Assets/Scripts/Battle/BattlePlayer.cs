using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlePlayer : MonoBehaviour
{
    [SerializeField] List<Enemy> enemies;

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

    public Enemy GetHealthyEnemy()
    {
        return enemies.Where(x => x.HP > 0).FirstOrDefault();
    }
}
