using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggable
{
    public void OnPlayerTriggered(Player player)
    {
        if (UnityEngine.Random.Range(1, 100) <= 15)
        {
            player.Character.Animator.IsMoving = false;
            GameController.Instance.StartBattle();
        }
    }
}
