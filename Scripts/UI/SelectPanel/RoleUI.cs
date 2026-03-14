using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个角色 Item（View）：点击后触发 Select_RoleChosen 事件，
/// 由 SelectSceneMediator 接管后续逻辑，无需知道其他面板。
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
        _button.onClick.AddListener(OnClick);
    }

    public void SetRoleData(RoleData data)
    {
        this.roleData = data;
        bool isLocked = data.unlock == 0 && !SaveService.Instance.IsRoleUnlocked(data.name);
        _avatar.sprite = isLocked
            ? Resources.Load<Sprite>("Image/UI/锁")
            : Resources.Load<Sprite>(data.avatar);
    }

    private void OnClick()
        => EventCenter.Instance.EventTrigger(E_EventType.Select_RoleChosen, roleData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207/255f, 207/255f, 207/255f);
        RoleSelectPanel.Instance.RenewUI(roleData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34/255f, 34/255f, 34/255f);
    }
}
