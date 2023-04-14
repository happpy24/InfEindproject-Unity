using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, ConfirmWindow, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Text expText;
    [SerializeField] Text hpText;
    [SerializeField] Text attackText;
    [SerializeField] Text defenseText;
    [SerializeField] Text spAttackText;
    [SerializeField] Text spDefenseText;
    [SerializeField] Text speedText;

    Enemy _enemy;
    BattlePlayer battlePlayer;

    int selectedItem = 0;
    InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemList.GetComponent<RectTransform>();
        itemListRect = itemList.GetComponent<RectTransform>();
        battlePlayer = BattlePlayer.GetBattlePlayer();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;

        SetData();
    }

    public void SetData()
    {
        _enemy = battlePlayer.Enemys[0];
        UpdateData();

        _enemy.OnHPChanged += UpdateData;

    }

    public void UpdateData()
    {
        nameText.text = _enemy.Base.Name;
        levelText.text = "Lvl " + _enemy.Level;
        hpBar.SetHP((float)_enemy.HP / _enemy.MaxHp);
        hpText.text = _enemy.HP.ToString() + "/" + _enemy.MaxHp.ToString();
        SetExp();
        attackText.text = _enemy.Attack.ToString();
        defenseText.text = _enemy.Defense.ToString();
        spAttackText.text = _enemy.SpAttack.ToString();
        spDefenseText.text = _enemy.SpDefense.ToString();
        speedText.text = _enemy.Speed.ToString();
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
        expText.text = _enemy.Exp.ToString() + "/" + _enemy.Base.GetExpForLevel(_enemy.Level).ToString();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _enemy.Base.GetExpForLevel(_enemy.Level);
        int nextLevelExp = _enemy.Base.GetExpForLevel(_enemy.Level + 1);

        float normalizedExp = (float)(_enemy.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    void UpdateItemList()
    {
        // Clear all existing
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack)
    {
        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
                SetData();
            }
                
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
                SetData();
            }
                

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (prevSelection != selectedItem)
                UpdateItemSelection();

            if (Input.GetKeyDown(KeyCode.Z))
            {
                SetData();
                Debug.Log($"Used {inventory.Slots[selectedItem].Item.Name}");
                inventory.UseItem(selectedItem, _enemy);
            }
                
            else if (Input.GetKeyDown(KeyCode.X))
            {
                selectedItem = 0;
                UpdateItemSelection();
                onBack?.Invoke();
            }
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = new Color(0.19f, 0.19f, 0.19f);
        }

        var item = inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count < itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
    }
}
