using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 角色列表项 — 只负责展示与交互。
/// 点击后发 UI_OnRoleSelected 事件，UIFlowController 统一处理面板切换与数据保存。
/// 不再直接引用 WeaponSelectPanel 或手动操作 CanvasGroup。
/// </summary>
public class RoleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _backImage;
    private Image _avatar;
    private Button _button;
    private RoleData roleData;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
    }

    public void SetRoleData(RoleData data)
    {
        roleData = data;
        bool locked = roleData.unlock == 0 && PlayerPrefs.GetInt(roleData.name, 1) == 1;
        _avatar.sprite = locked
            ? Resources.Load<Sprite>("Image/UI/锁")
            : Resources.Load<Sprite>(roleData.avatar);

        _button.onClick.AddListener(() =>
        {
            // 通知 UIFlowController 处理选角逻辑（面板互斥、保存数据）
            EventCenter.Instance.EventTrigger(E_EventType.UI_OnRoleSelected, data);
            RoleSelectPanel.Instance?.RenewUI(roleData);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207 / 255f, 207 / 255f, 207 / 255f);
        RoleSelectPanel.Instance?.RenewUI(roleData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34 / 255f, 34 / 255f, 34 / 255f);
    }
}
