using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color parColor;

    Enemy _enemy;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Enemy enemy)
    {
        _enemy = enemy;

        nameText.text = enemy.Base.Name;
        SetLevel();
        hpBar.SetHP((float)enemy.HP / enemy.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.par, parColor},
        };

        SetStatusText();
        _enemy.OnStatusChanged += SetStatusText;
        _enemy.OnHPChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if (_enemy.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _enemy.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_enemy.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _enemy.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _enemy.Base.GetExpForLevel(_enemy.Level);
        int nextLevelExp = _enemy.Base.GetExpForLevel(_enemy.Level + 1);

        float normalizedExp = (float)(_enemy.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);

    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_enemy.HP / _enemy.MaxHp);  
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }
}
