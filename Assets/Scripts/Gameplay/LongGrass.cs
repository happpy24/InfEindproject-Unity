using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggable
{
    public int cooldown = 0;
    public void OnPlayerTriggered(Player player)
    {
        if (cooldown < 5)
        {
            cooldown++;
        }
        else
        {
            if (UnityEngine.Random.Range(1, 201) <= 15)
            {
                player.Character.Animator.IsMoving = false;
                GameController.Instance.StartBattle();
                cooldown = 0;
            }
        }
    }
}
