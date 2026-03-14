using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个武器 Item（View）：点击后触发 Select_WeaponChosen 事件。
/// </summary>
public class WeaponUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public WeaponData weaponData;
    public Image _backImage;
    private Image  _avatar;
    private Button _button;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    public void SetWeaponData(WeaponData data)
    {
        this.weaponData = data;
        if (data != null)
            _avatar.sprite = Resources.Load<Sprite>(data.avatar);
    }

    private void OnClick()
        => EventCenter.Instance.EventTrigger(E_EventType.Select_WeaponChosen, weaponData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207/255f, 207/255f, 207/255f);
        WeaponSelectPanel.Instance.RenewUI(weaponData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34/255f, 34/255f, 34/255f);
    }
}

