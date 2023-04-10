using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Enemy> wildEnemys;

    public Enemy GetRandomWildEnemy()
    {
        var wildEnemy = wildEnemys[Random.Range(0, wildEnemys.Count)];
        wildEnemy.Init();
        return wildEnemy;
    }
}
 