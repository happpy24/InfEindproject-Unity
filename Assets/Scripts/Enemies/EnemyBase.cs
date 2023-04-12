using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/Create new Enemy")]
public class EnemyBase : ScriptableObject
{
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    [SerializeField] string name;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite sprite;

    [SerializeField] EnemyType type1;
    [SerializeField] EnemyType type2;

    // Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }

    public Sprite Sprite
    {
        get { return sprite; }
    }

    public EnemyType Type1
    {
        get { return type1; }
    }

    public EnemyType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpDefense
    {
        get { return defense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum EnemyType
{
    None,
    Normal,
    Fire,
    Water,
    Grass
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}

public class TypeChart
{
    static float[][] chart =
    {
        //                   NOR  FIR   WAT   GRA
        /*NOR*/ new float[] {1f,  1f,   1f,   1f},
        /*FIR*/ new float[] {1f,  0.5f, 0.5f, 2f},
        /*WAT*/ new float[] {1f,  2f,   0.5f, 0.5f},
        /*GRA*/ new float[] {1f,  0.5f, 2f,   0.5f}
    };

    public static float GetEffectiveness(EnemyType attackType, EnemyType defenseType)
    {
        if (attackType == EnemyType.None || defenseType == EnemyType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}