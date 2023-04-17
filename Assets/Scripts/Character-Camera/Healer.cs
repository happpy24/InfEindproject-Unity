using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public Vector3 savedLocation 
    { 
        get { return savedLocation; }
        set { savedLocation = value; }
    }

    public IEnumerator Heal(Transform player)
    {
        yield return Fader.i.FadeIn(0.5f);

        var battlePlayer = player.GetComponent<BattlePlayer>();
        battlePlayer.Enemys.ForEach(p => p.Heal());
        battlePlayer.Enemys.ForEach(p => p.Moves.ForEach(m => m.PPHEAL()));
        battlePlayer.BattlePlayerUpdated();
        savedLocation = player.transform.position;

        yield return Fader.i.FadeOut(0.5f);
    }
}
