using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店道具卡片 UI（View 层）。
/// 职责：
/// - 展示道具数据（图标、名称、类型、描述、价格）。
/// - 购买按钮点击后触发 <see cref="E_EventType.Shop_RequestBuy"/> 事件，
///   由 UIFlowController 转发给 ShopController 处理，结果通过
///   <see cref="E_EventType.Shop_BuyResult"/> 回调。
/// - 收到购买成功的结果后隐藏本卡片（视觉反馈）。
/// </summary>
public class ItemCardUI : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private Button _btnBuy;
    public CanvasGroup _canvasGroup;

    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemType;
    [SerializeField] private TextMeshProUGUI _itemDesc;
    [SerializeField] private TextMeshProUGUI _itemPrice;
    [SerializeField] private Image _itemIcon;

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<ShopBuyResult>(E_EventType.Shop_BuyResult, OnBuyResult);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<ShopBuyResult>(E_EventType.Shop_BuyResult, OnBuyResult);
    }

    /// <summary>填充卡片显示数据。由 ShopPanel 在随机生成道具列表后调用。</summary>
    public void SetItemData(ItemData data)
    {
        itemData       = data;
        _itemName.text = data.name;
        _itemDesc.text = data.describe;
        _itemPrice.text = data.price.ToString();

        if (data is WeaponData)
        {
            _itemType.text  = "武器";
            _itemIcon.sprite = Resources.Load<Sprite>(data.avatar);
        }
        else if (data is PropData)
        {
            _itemType.text  = "道具";
            _itemIcon.sprite = GameManager.Instance.propsAtlas.GetSprite(data.name);
        }
    }

    private void Start()
    {
        _btnBuy.onClick.AddListener(OnClickBuy);
    }

    private void OnClickBuy()
    {
        if (itemData == null) return;
        // 向 UIFlowController 请求购买，不直接调用 ShopPanel
        EventCenter.Instance.EventTrigger<ItemData>(E_EventType.Shop_RequestBuy, itemData);
    }

    /// <summary>收到购买结果事件；只处理与本卡片相同道具的结果。</summary>
    private void OnBuyResult(ShopBuyResult result)
    {
        if (result.item != itemData) return; // 不是自己被购买的回调
        if (!result.success) return;

        // 购买成功：隐藏本卡片（已售出）
        _canvasGroup.alpha        = 0;
        _canvasGroup.interactable = false;
    }
}