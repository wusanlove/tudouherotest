using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 角色列表中的单个角色卡。
/// 点击后只负责存数据 + 发事件，不直接操作其他面板。
/// 面板间切换由 RoleSelectPanel 的事件处理函数统一完成。
/// </summary>
public class RoleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image  _backImage;
    private Image  _avatar;
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

        _avatar.sprite = (roleData.unlock == 0 && PlayerPrefs.GetInt(roleData.name, 1) == 1)
            ? Resources.Load<Sprite>("Image/UI/锁")
            : Resources.Load<Sprite>(roleData.avatar);

        _button.onClick.AddListener(() => OnRoleButtonClick(data));
    }

    private void OnRoleButtonClick(RoleData data)
    {
        // 存储选择结果
        GameManager.Instance.currentRoleData = data;
        // 通知面板层处理跳转（解耦：本脚本不再依赖 RoleSelectPanel / WeaponSelectPanel）
        EventCenter.Instance.EventTrigger(E_EventType.Select_RoleChosen, data);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207 / 255f, 207 / 255f, 207 / 255f);
        RoleSelectPanel.Instance.RenewUI(roleData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34 / 255f, 34 / 255f, 34 / 255f);
    }
}
