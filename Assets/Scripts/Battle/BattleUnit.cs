using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud
    {
        get { return hud; }
    }

    public Enemy Enemy { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Enemy enemy)
    {
        Enemy = enemy;
        if (isPlayerUnit)
            image.sprite = Enemy.Base.Sprite;
        else
            image.sprite = Enemy.Base.Sprite;

        hud.SetData(enemy);

        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if (!isPlayerUnit)
            image.transform.localPosition = new Vector3(1000f, originalPos.y);

        image.transform.DOLocalMoveX(originalPos.x+2, 0.6f);
        image.transform.DOLocalMoveX(originalPos.x, 1);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (!isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 50f, 0.2f));

        sequence.Append(image.transform.DOLocalMoveY(originalPos.y, 0.2f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.DOColor(new Color(0.48f, 0, 0, 0.5f), 0.1f));
        else
        {
            sequence.Append(image.DOColor(new Color(0.48f, 0, 0), 0.1f));
            sequence.Join(image.transform.DOLocalMoveX(originalPos.x - 10f, 0.1f));
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 10f, 0.1f));
        }
        sequence.Append(image.DOColor(originalColor, 0.1f));
        sequence.Join(image.transform.DOLocalMoveX(originalPos.x, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.DOColor(new Color(0.48f, 0, 0, 0.5f), 0.3f));
        else
        {
            sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
            sequence.Join(image.DOFade(0f, 0.3f));
        }
    }
}
