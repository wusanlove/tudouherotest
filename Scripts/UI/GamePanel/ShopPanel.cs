using System.Collections.Generic;
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
    private const int RefreshCost     = 3;
    private const int MinPropItems    = 2;
    private const int TotalShopItems  = 4;

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

    // 缓存属性面板的 TMP_Text 引用，避免 SetAttrUI 每次 GetComponent
    private TMP_Text[] _attrTexts;

    public override void Awake()
    {
        base.Awake();
        _refreshButton.onClick.AddListener(RefreshItem);

        // 缓存属性面板 TMP_Text 引用
        int count = _attrLayout.childCount;
        _attrTexts = new TMP_Text[count];
        for (int i = 0; i < count; i++)
            _attrTexts[i] = _attrLayout.GetChild(i).GetChild(2).GetComponent<TMP_Text>();
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
        if (GameManager.Instance.money < RefreshCost) return;
        GameManager.Instance.money -= RefreshCost;
        _moneyText.text = GameManager.Instance.money.ToString();
        RandomProps();
    }

    private void SetAttrUI()
    {
        if (_attrTexts == null || _attrTexts.Length < 11) return;
        PropData p = GameManager.Instance.propData;
        _attrTexts[0].text  = ((int)(GameManager.Instance.exp / 12)).ToString();
        _attrTexts[1].text  = p.maxHp.ToString();
        _attrTexts[2].text  = p.revive.ToString();
        _attrTexts[3].text  = p.short_damage.ToString();
        _attrTexts[4].text  = p.long_damage.ToString();
        _attrTexts[5].text  = p.critical_strikes_probability.ToString();
        _attrTexts[6].text  = p.pickRange.ToString();
        _attrTexts[7].text  = p.speedPer.ToString();
        _attrTexts[8].text  = p.harvest.ToString();
        _attrTexts[9].text  = p.expMuti.ToString();
        _attrTexts[10].text = p.shopDiscount.ToString();
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
        int propCount  = Random.Range(MinPropItems, TotalShopItems);
        var allProps   = ConfigService.Instance.Props;
        var allWeapons = ConfigService.Instance.Weapons;

        for (int i = 0; i < propCount; i++)
            props.Add(allProps[Random.Range(0, allProps.Count)]);
        for (int i = 0; i < TotalShopItems - propCount; i++)
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
