using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 商店面板 — 购买道具/武器、合成武器、刷新商店。
/// 点击"出发"发 UI_OnNextWaveClicked 事件，由 UIFlowController → SceneFlowController
/// 统一处理跳回 GamePlay，不再直接调用 SceneManager。
/// </summary>
public class ShopPanel : BaseMgrMono<ShopPanel>
{
    public Button    _startButton;
    public Button    _refreshButton;
    public TMP_Text  _shopText;
    public TMP_Text  _moneyText;
    public TMP_Text  _weaponTitle;

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
        _startButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
            "出发 (第" + GameManager.Instance.currentWave + "波)";

        // 点击"出发" → 发事件，由 UIFlowController → SceneFlowController 切场景
        // 不直接调 SceneManager，保持 UI 与场景流程解耦
        _startButton.onClick.AddListener(() =>
            EventCenter.Instance.EventTrigger(E_EventType.UI_OnNextWaveClicked));

        _moneyText.text = GameManager.Instance.money.ToString();

        SetAttrUI();
        ShowCurrentProp();
        ShowCurrentWeapon();
        RandomProps();
        SetWeaponSlotIndex();
    }

    // ── 购买 ─────────────────────────────────────────────────────

    public bool Shopping(ItemData itemData)
    {
        if (GameManager.Instance.money < 3)                                                    return false;
        if (itemData is WeaponData &&
            GameManager.Instance.currentWeapons.Count >= GameManager.Instance.propData.slot)  return false;
        if (itemData is PropData &&
            GameManager.Instance.currentProps.Count   >= 20)                                  return false;
        if (GameManager.Instance.money < itemData.price)                                       return false;

        GameManager.Instance.money -= itemData.price;
        _moneyText.text = GameManager.Instance.money.ToString();
        // 广播金币变化（GamePanel 已订阅，但 Shop 场景里 GamePanel 不存在，此广播无害）
        EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerMoneyChanged, GameManager.Instance.money);

        if (itemData is WeaponData weaponData)
        {
            var copy = JsonConvert.DeserializeObject<WeaponData>(JsonConvert.SerializeObject(weaponData));
            GameManager.Instance.currentWeapons.Add(copy);
            ShowCurrentWeapon();
        }
        else if (itemData is PropData propData)
        {
            var copy = JsonConvert.DeserializeObject<PropData>(JsonConvert.SerializeObject(propData));
            GameManager.Instance.currentProps.Add(copy);
            ShowCurrentProp();
            GameManager.Instance.FusionAttr(copy);
            SetAttrUI();
        }

        return true;
    }

    // ── UI 刷新方法 ───────────────────────────────────────────────

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
        var p = GameManager.Instance.propData;
        SetAttrText(0,  ((int)(GameManager.Instance.exp / 12)).ToString());
        SetAttrText(1,  p.maxHp.ToString());
        SetAttrText(2,  p.revive.ToString());
        SetAttrText(3,  p.short_damage.ToString());
        SetAttrText(4,  p.long_damage.ToString());
        SetAttrText(5,  p.critical_strikes_probability.ToString());
        SetAttrText(6,  p.pickRange.ToString());
        SetAttrText(7,  p.speedPer.ToString());
        SetAttrText(8,  p.harvest.ToString());
        SetAttrText(9,  p.expMuti.ToString());
        SetAttrText(10, p.shopDiscount.ToString());
    }

    private void SetAttrText(int index, string value)
    {
        if (index < _attrLayout.childCount)
            _attrLayout.GetChild(index).GetChild(2).GetComponent<TMP_Text>().text = value;
    }

    private void ShowCurrentProp()
    {
        for (int i = 0; i < _propsLayout.childCount; i++)
        {
            var icon = _propsLayout.GetChild(i).GetChild(0).GetComponent<Image>();
            if (i < GameManager.Instance.currentProps.Count)
            {
                icon.enabled = true;
                icon.sprite  = GameManager.Instance.propsAtlas.GetSprite(GameManager.Instance.currentProps[i].name);
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
                slot.weaponData       = GameManager.Instance.currentWeapons[i];
                slot._weaponIcon.enabled = true;
                slot._weaponIcon.sprite  = Resources.Load<Sprite>(slot.weaponData.avatar);
                slot._weaponBG.color  = GetGradeColor(slot.weaponData.grade);
            }
            else
            {
                slot._weaponIcon.enabled = false;
                slot._weaponBG.color     = new Color(29 / 255f, 29 / 255f, 29 / 255f);
                slot.weaponData          = null;
            }
        }
        _weaponTitle.text = "武器(" + GameManager.Instance.currentWeapons.Count + "/" +
                            GameManager.Instance.propData.slot + ")";
    }

    private static Color GetGradeColor(int grade)
    {
        switch (grade)
        {
            case 2:  return new Color(58 / 255f, 83 / 255f, 99 / 255f);
            case 3:  return new Color(79 / 255f, 58 / 255f, 99 / 255f);
            case 4:  return new Color(99 / 255f, 50 / 255f, 50 / 255f);
            default: return new Color(29 / 255f, 29 / 255f, 29 / 255f);
        }
    }

    private void RandomProps()
    {
        props.Clear();
        int propCount = Random.Range(2, 4);
        for (int i = 0; i < propCount; i++)
            props.Add(GameManager.Instance.RandomOne(GameManager.Instance.propDatas) as ItemData);
        for (int i = 0; i < 4 - propCount; i++)
            props.Add(GameManager.Instance.RandomOne(GameManager.Instance.weaponDatas) as ItemData);
        ShowPropUI();
    }

    private void ShowPropUI()
    {
        for (int i = 0; i < props.Count; i++)
        {
            var card = _itemLayout.GetChild(i).GetComponent<ItemCardUI>();
            card._canvasGroup.alpha        = 1;
            card._canvasGroup.interactable = true;
            card.SetItemData(props[i]);
        }
    }
}