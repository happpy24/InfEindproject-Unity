using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Healer : MonoBehaviour
{
    public int savedScene = 0;
    public Vector3 savedLocation;
    public Vector2 savedCamMin;
    public Vector2 savedCamMax;

    public int SavedScene
    {
        get { return savedScene = 0; }
    } 

    public Vector3 SavedLocation 
    { 
        get { return savedLocation; }
    }

    public Vector2 SavedCamMin
    {
        get { return savedCamMin = new Vector2(-2, -2); }
    }

    public Vector2 SavedCamMax
    {
        get { return savedCamMax = new Vector2(-2, 1); }
    }


    public IEnumerator Heal(Transform player)
    {
        yield return Fader.i.FadeIn(0.5f);

        var battlePlayer = player.GetComponent<BattlePlayer>();
        battlePlayer.Enemys.ForEach(p => p.Heal());
        battlePlayer.Enemys.ForEach(p => p.Moves.ForEach(m => m.PPHEAL()));
        battlePlayer.BattlePlayerUpdated();
        savedLocation = player.transform.position;
        var cam = FindAnyObjectByType<GameplayCamera>();
        savedCamMax = cam.MaxPos;
        savedCamMin = cam.MinPos;

        savedScene = SceneManager.GetActiveScene().buildIndex;

        yield return Fader.i.FadeOut(0.5f);
    }
}
