using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 武器列表项 — 只负责展示与交互。
/// 点击后发 UI_OnWeaponSelected 事件，UIFlowController 统一处理面板切换与数据保存。
/// 不再直接引用 DifficultySelectPanel 或手动操作 CanvasGroup。
/// </summary>
public class WeaponUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public WeaponData weaponData;
    public Image _backImage;
    private Image _avatar;
    private Button _button;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
    }

    public void SetWeaponData(WeaponData data)
    {
        if (data == null) return;
        weaponData = data;
        _avatar.sprite = Resources.Load<Sprite>(data.avatar);

        _button.onClick.AddListener(() =>
        {
            // 通知 UIFlowController 处理选武器逻辑（面板互斥、保存数据）
            EventCenter.Instance.EventTrigger(E_EventType.UI_OnWeaponSelected, data);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207 / 255f, 207 / 255f, 207 / 255f);
        WeaponSelectPanel.Instance?.RenewUI(weaponData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34 / 255f, 34 / 255f, 34 / 255f);
    }
}

