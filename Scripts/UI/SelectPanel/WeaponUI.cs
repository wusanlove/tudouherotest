using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 武器列表中的单个武器卡。
/// 点击后只负责存数据 + 发事件，不直接操作其他面板。
/// 面板间切换由 WeaponSelectPanel 的事件处理函数统一完成。
/// </summary>
public class WeaponUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public WeaponData weaponData;
    public Image      _backImage;
    private Image     _avatar;
    private Button    _button;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
    }

    public void SetWeaponData(WeaponData data)
    {
        weaponData     = data;
        _avatar.sprite = Resources.Load<Sprite>(data.avatar);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => OnWeaponButtonClick(data));
    }

    private void OnWeaponButtonClick(WeaponData data)
    {
        GameManager.Instance.currentWeapons.Add(data);
        // 通知面板层处理跳转（解耦：本脚本不再依赖 WeaponSelectPanel / DifficultySelectPanel）
        EventCenter.Instance.EventTrigger(E_EventType.Select_WeaponChosen, data);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207 / 255f, 207 / 255f, 207 / 255f);
        WeaponSelectPanel.Instance.RenewUI(weaponData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34 / 255f, 34 / 255f, 34 / 255f);
    }
}
