using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public WeaponData weaponData;
    public Image _backImage;
    private Image _avatar;
    private Button _button;
    private void Awake()    
    {
        
        _backImage = GetComponent<Image>();
        _avatar = transform.GetChild(0).GetComponent<Image>();
        _button = GetComponent<Button>();
    }


    public void SetWeaponData(WeaponData weaponData)
    {
        if (weaponData != null)
        {
            this.weaponData = weaponData;
            _avatar.sprite= Resources.Load<Sprite>(weaponData.avatar);
            
        }
        _button.onClick.AddListener(() =>
        {
            //记录当前武器
            GameManager.Instance.currentWeapons.Add(weaponData);
            //关闭武器选择面板
            WeaponSelectPanel.Instance._canvasGroup.alpha = 0f;
            WeaponSelectPanel.Instance._canvasGroup.blocksRaycasts = false;
            WeaponSelectPanel.Instance._canvasGroup.interactable = false;
            //打开难度选择面板
            DifficultySelectPanel.Instance._canvasGroup.alpha = 1f;
            DifficultySelectPanel.Instance._canvasGroup.blocksRaycasts = true;
            DifficultySelectPanel.Instance._canvasGroup.interactable = true;
            //克隆角色 武器UI 激活难度UI
            Instantiate(RoleSelectPanel.Instance._roleDetailGameObject, DifficultySelectPanel.Instance._difficultyDetailTransform);
            Instantiate(WeaponSelectPanel.Instance._weaponDetailGameObject, DifficultySelectPanel.Instance._difficultyDetailTransform);
            DifficultySelectPanel.Instance._difficultyDetailGameObject.SetActive(true);
        });
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207/255f, 207/255f , 207/255f);
        WeaponSelectPanel.Instance.RenewUI(weaponData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34/255f, 34/255f , 34/255f);
        
    }
}

