using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player)
    {
        yield return Fader.i.FadeIn(0.5f);

        var battlePlayer = player.GetComponent<BattlePlayer>();
        battlePlayer.Enemys.ForEach(p => p.Heal());
        battlePlayer.BattlePlayerUpdated();

        yield return Fader.i.FadeOut(0.5f);
    }
}
