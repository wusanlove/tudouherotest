using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 商店面板（View + 局部 Mediator）：
/// 展示当前角色属性/武器/道具，并提供购买、刷新商品功能。
/// 场景跳转委托 ShopController.ProceedToNextWave()，不直接调用 SceneManager。
/// 购买逻辑委托 ShopController.TryBuy()。
/// </summary>
public class ShopPanel : BaseMgrMono<ShopPanel>
{
    public Button   _startButton;
    public Button   _refreshButton;
    public TMP_Text _shopText;
    public TMP_Text _moneyText;
    public TMP_Text _weaponTitle;

    public Transform _attrLayout;
    public Transform _propsLayout;
    public Transform _weaponsLayout;
    public Transform _itemLayout;

    public List<ItemData> props = new List<ItemData>();

    public override void Awake()
    {
        base.Awake();
        _refreshButton.onClick.AddListener(RefreshItem);
    }

    private void Start()
    {
        _shopText.text = "商店 (第" + (GameManager.Instance.currentWave - 1) + "波)";
        _startButton.transform.GetChild(0).GetComponent<TMP_Text>().text
            = "出发 (第" + GameManager.Instance.currentWave + "波)";

        // 委托 ShopController 处理波次递进和场景跳转
        _startButton.onClick.AddListener(() => ShopController.Instance.ProceedToNextWave());

        _moneyText.text = GameManager.Instance.money.ToString();

        SetAttrUI();
        ShowCurrentProp();
        ShowCurrentWeapon();
        RandomProps();
        SetWeaponSlotIndex();
    }

    private void SetWeaponSlotIndex()
    {
        for (int i = 0; i < _weaponsLayout.childCount; i++)
            _weaponsLayout.GetChild(i).GetComponent<WeaponSlot>().slotCount = i;
    }

    private void RefreshItem()
    {
        if (GameManager.Instance.money < 3) return;
        GameManager.Instance.money -= 3;
        _moneyText.text = GameManager.Instance.money.ToString();
        RandomProps();
    }

    private void SetAttrUI()
    {
        TMP_Text Get(int row) => _attrLayout.GetChild(row).GetChild(2).GetComponent<TMP_Text>();
        PropData p = GameManager.Instance.propData;
        Get(0).text  = ((int)(GameManager.Instance.exp / 12)).ToString();
        Get(1).text  = p.maxHp.ToString();
        Get(2).text  = p.revive.ToString();
        Get(3).text  = p.short_damage.ToString();
        Get(4).text  = p.long_damage.ToString();
        Get(5).text  = p.critical_strikes_probability.ToString();
        Get(6).text  = p.pickRange.ToString();
        Get(7).text  = p.speedPer.ToString();
        Get(8).text  = p.harvest.ToString();
        Get(9).text  = p.expMuti.ToString();
        Get(10).text = p.shopDiscount.ToString();
    }

    private void ShowCurrentProp()
    {
        for (int i = 0; i < _propsLayout.childCount; i++)
        {
            var icon = _propsLayout.GetChild(i).GetChild(0).GetComponent<Image>();
            if (i < GameManager.Instance.currentProps.Count)
            {
                icon.enabled = true;
                icon.sprite  = GameManager.Instance.propsAtlas
                    .GetSprite(GameManager.Instance.currentProps[i].name);
            }
            else
            {
                icon.enabled = false;
            }
        }
    }

    public void ShowCurrentWeapon()
    {
        for (int i = 0; i < _weaponsLayout.childCount; i++)
        {
            WeaponSlot slot = _weaponsLayout.GetChild(i).GetComponent<WeaponSlot>();
            if (i < GameManager.Instance.currentWeapons.Count)
            {
                slot._weaponIcon.enabled = true;
                slot.weaponData          = GameManager.Instance.currentWeapons[i];
                slot._weaponIcon.sprite  = Resources.Load<Sprite>(slot.weaponData.avatar);
                switch (slot.weaponData.grade)
                {
                    case 1: slot._weaponBG.color = new Color(29/255f, 29/255f, 29/255f); break;
                    case 2: slot._weaponBG.color = new Color(58/255f, 83/255f, 99/255f); break;
                    case 3: slot._weaponBG.color = new Color(79/255f, 58/255f, 99/255f); break;
                    case 4: slot._weaponBG.color = new Color(99/255f, 50/255f, 50/255f); break;
                }
            }
            else
            {
                slot._weaponIcon.enabled = false;
                slot._weaponBG.color     = new Color(29/255f, 29/255f, 29/255f);
                slot.weaponData          = null;
            }
        }
        _weaponTitle.text = $"武器({GameManager.Instance.currentWeapons.Count}"
                          + $"/{GameManager.Instance.propData.slot})";
    }

    private void RandomProps()
    {
        props.Clear();
        int propCount  = Random.Range(2, 4);
        var allProps   = ConfigService.Instance.Props;
        var allWeapons = ConfigService.Instance.Weapons;

        for (int i = 0; i < propCount; i++)
            props.Add(allProps[Random.Range(0, allProps.Count)]);
        for (int i = 0; i < 4 - propCount; i++)
            props.Add(allWeapons[Random.Range(0, allWeapons.Count)]);

        ShowPropUI();
    }

    private void ShowPropUI()
    {
        for (int i = 0; i < props.Count && i < _itemLayout.childCount; i++)
        {
            var cg = _itemLayout.GetChild(i).GetComponent<ItemCardUI>()._canvasGroup;
            cg.alpha = 1; cg.interactable = true;
            _itemLayout.GetChild(i).GetComponent<ItemCardUI>().SetItemData(props[i]);
        }
    }

    /// <summary>由 ItemCardUI 调用，委托 ShopController 处理购买逻辑</summary>
    public bool Shopping(ItemData itemData)
    {
        bool success = ShopController.Instance.TryBuy(itemData);
        if (success)
        {
            _moneyText.text = GameManager.Instance.money.ToString();
            ShowCurrentWeapon();
            ShowCurrentProp();
            SetAttrUI();
        }
        return success;
    }
}
