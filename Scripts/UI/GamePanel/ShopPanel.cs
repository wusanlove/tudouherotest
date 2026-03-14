using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 商店面板（View 层）。
///
/// 职责：
/// - 初始化并展示商店 UI（属性、已有道具、已有武器、随机可购道具）。
/// - 响应 UI 交互：刷新按钮→抛 Shop_RequestRefresh，
///   下一波按钮→抛 Shop_ProceedToNextWave（由 UIFlowController 处理场景跳转）。
/// - 监听 Shop_BuyResult、Shop_RefreshResult 更新视图（金币、武器列表等）。
///
/// 不应有的逻辑（已移至 ShopController）：
/// - 扣金币、校验购买条件、写 GameManager 状态。
/// </summary>
public class ShopPanel : BaseMgrMono<ShopPanel>
{
    [Header("按钮")]
    public Button _startButton;
    public Button _refreshButton;

    [Header("文本")]
    public TMP_Text _shopText;
    public TMP_Text _moneyText;
    public TMP_Text _weaponTitle;

    [Header("布局容器")]
    public Transform _attrLayout;
    public Transform _propsLayout;
    public Transform _weaponsLayout;
    public Transform _itemLayout;

    // 本次商店随机出的道具列表（WeaponData 或 PropData）
    private List<ItemData> _shopItems = new List<ItemData>();

    public override void Awake()
    {
        base.Awake();
        _refreshButton.onClick.AddListener(OnClickRefresh);
        _startButton.onClick.AddListener(OnClickProceed);
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<ShopBuyResult>(E_EventType.Shop_BuyResult, OnBuyResult);
        EventCenter.Instance.AddEventListener<bool>(E_EventType.Shop_RefreshResult, OnRefreshResult);
        EventCenter.Instance.AddEventListener(E_EventType.HUD_MoneyChanged, RefreshMoneyText);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<ShopBuyResult>(E_EventType.Shop_BuyResult, OnBuyResult);
        EventCenter.Instance.RemoveEventListener<bool>(E_EventType.Shop_RefreshResult, OnRefreshResult);
        EventCenter.Instance.RemoveEventListener(E_EventType.HUD_MoneyChanged, RefreshMoneyText);
    }

    private void Start()
    {
        var gm = GameManager.Instance;

        _shopText.text = $"商店 (第{gm.currentWave - 1}波)";
        _startButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
            $"出发 (第{gm.currentWave}波)";

        RefreshMoneyText();
        SetAttrUI();
        ShowCurrentProp();
        ShowCurrentWeapon();
        SetWeaponSlotIndex();
        RandomProps();
    }

    // ── 按钮回调（仅抛事件，不含业务）─────────────────────────────────────

    private void OnClickRefresh()
        => EventCenter.Instance.EventTrigger(E_EventType.Shop_RequestRefresh);

    private void OnClickProceed()
        => EventCenter.Instance.EventTrigger(E_EventType.Shop_ProceedToNextWave);

    // ── 事件监听：购买结果 ──────────────────────────────────────────────────

    private void OnBuyResult(ShopBuyResult result)
    {
        if (!result.success) return;
        // 购买成功后刷新视图（ItemCardUI 自己负责隐藏卡片）
        RefreshMoneyText();
        ShowCurrentWeapon();
        ShowCurrentProp();
        SetAttrUI();
    }

    // ── 事件监听：刷新结果 ──────────────────────────────────────────────────

    private void OnRefreshResult(bool success)
    {
        if (!success) return;
        RefreshMoneyText();
        RandomProps();
    }

    // ── 视图刷新方法 ────────────────────────────────────────────────────────

    public void RefreshMoneyText()
    {
        _moneyText.text = GameManager.Instance.money.ToString();
    }

    private void SetWeaponSlotIndex()
    {
        int count = _weaponsLayout.childCount;
        for (int i = 0; i < count; i++)
            _weaponsLayout.GetChild(i).GetComponent<WeaponSlot>().slotCount = i;
    }

    private void SetAttrUI()
    {
        var p = GameManager.Instance.propData;
        var gm = GameManager.Instance;

        _attrLayout.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
            ((int)(gm.exp / 12)).ToString();
        _attrLayout.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = p.maxHp.ToString();
        _attrLayout.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = p.revive.ToString();
        _attrLayout.GetChild(3).GetChild(2).GetComponent<TMP_Text>().text = p.short_damage.ToString();
        _attrLayout.GetChild(4).GetChild(2).GetComponent<TMP_Text>().text = p.long_damage.ToString();
        _attrLayout.GetChild(5).GetChild(2).GetComponent<TMP_Text>().text = p.critical_strikes_probability.ToString();
        _attrLayout.GetChild(6).GetChild(2).GetComponent<TMP_Text>().text = p.pickRange.ToString();
        _attrLayout.GetChild(7).GetChild(2).GetComponent<TMP_Text>().text = p.speedPer.ToString();
        _attrLayout.GetChild(8).GetChild(2).GetComponent<TMP_Text>().text = p.harvest.ToString();
        _attrLayout.GetChild(9).GetChild(2).GetComponent<TMP_Text>().text = p.expMuti.ToString();
        _attrLayout.GetChild(10).GetChild(2).GetComponent<TMP_Text>().text = p.shopDiscount.ToString();
    }

    private void ShowCurrentProp()
    {
        var gm = GameManager.Instance;
        int count = _propsLayout.childCount;
        for (int i = 0; i < count; i++)
        {
            var icon = _propsLayout.GetChild(i).GetChild(0).GetComponent<Image>();
            if (i < gm.currentProps.Count)
            {
                icon.enabled = true;
                icon.sprite   = gm.propsAtlas.GetSprite(gm.currentProps[i].name);
            }
            else
            {
                icon.enabled = false;
            }
        }
    }

    public void ShowCurrentWeapon()
    {
        var gm    = GameManager.Instance;
        int count = _weaponsLayout.childCount;
        for (int i = 0; i < count; i++)
        {
            var slot = _weaponsLayout.GetChild(i).GetComponent<WeaponSlot>();
            if (i < gm.currentWeapons.Count)
            {
                slot._weaponIcon.enabled = true;
                slot.weaponData          = gm.currentWeapons[i];
                slot._weaponIcon.sprite  = Resources.Load<Sprite>(slot.weaponData.avatar);

                slot._weaponBG.color = slot.weaponData.grade switch
                {
                    1 => new Color(29 / 255f, 29 / 255f, 29 / 255f),
                    2 => new Color(58 / 255f, 83 / 255f, 99 / 255f),
                    3 => new Color(79 / 255f, 58 / 255f, 99 / 255f),
                    4 => new Color(99 / 255f, 50 / 255f, 50 / 255f),
                    _ => new Color(29 / 255f, 29 / 255f, 29 / 255f)
                };
            }
            else
            {
                slot._weaponIcon.enabled = false;
                slot._weaponBG.color     = new Color(29 / 255f, 29 / 255f, 29 / 255f);
                slot.weaponData          = null;
            }
        }
        _weaponTitle.text = $"武器({gm.currentWeapons.Count}/{gm.propData.slot})";
    }

    private void RandomProps()
    {
        _shopItems.Clear();
        var gm = GameManager.Instance;

        int propCount = Random.Range(2, 4);
        for (int i = 0; i < propCount; i++)
            _shopItems.Add(gm.RandomOne(gm.propDatas) as ItemData);
        for (int i = 0; i < 4 - propCount; i++)
            _shopItems.Add(gm.RandomOne(gm.weaponDatas) as ItemData);

        ShowItemUI();
    }

    private void ShowItemUI()
    {
        for (int i = 0; i < _shopItems.Count; i++)
        {
            var card = _itemLayout.GetChild(i).GetComponent<ItemCardUI>();
            card._canvasGroup.alpha        = 1;
            card._canvasGroup.interactable = true;
            card.SetItemData(_shopItems[i]);
        }
    }
}