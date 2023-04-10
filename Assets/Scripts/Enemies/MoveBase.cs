using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Enemy/Create new move")]
public class MoveBase : ScriptableObject
{
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    [SerializeField] string name;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    [TextArea]
    [SerializeField] string description;

    [SerializeField] EnemyType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public EnemyType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int PP
    {
        get { return pp; }
    }

    public bool IsSpecial
    {
        get
        {
            if (type == EnemyType.Fire || type == EnemyType.Water || type == EnemyType.Grass)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
