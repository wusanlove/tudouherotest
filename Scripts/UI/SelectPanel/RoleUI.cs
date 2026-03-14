using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class RoleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _backImage; //背景图片
    private Image _avatar;  //角色头像
    private Button _button;  //按钮
    
    private RoleData roleData;
    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar = transform.GetChild(0).GetComponent<Image>();
        _button = GetComponent<Button>();
        
    }

   public void SetRoleData(RoleData data)
    {
        this.roleData = data;
        
        if (roleData.unlock == 0 && SaveProgressService.Instance.GetRoleUnlock(roleData.name) == 0)
        {
            _avatar.sprite = Resources.Load<Sprite>("Image/UI/锁");
        }
        else
        {
            _avatar.sprite = Resources.Load<Sprite>(roleData.avatar);
        }

        _button.onClick.AddListener(() =>
        {
            ButtonClickRole(data);
        });
    }

    void ButtonClickRole(RoleData data)
    {
        //记录下选角色的信息
        GameManager.Instance.currentRoleData = data;
        //关闭角色选择面板
        //TODO:使用UIMgr进行管理
        RoleSelectPanel.Instance._canvasGroup.alpha = 0;
        RoleSelectPanel.Instance._canvasGroup.interactable = false;
        RoleSelectPanel.Instance._canvasGroup.blocksRaycasts = false;
        //打开武器选择面板
        WeaponSelectPanel.Instance._canvasGroup.alpha = 1;
        WeaponSelectPanel.Instance._canvasGroup.interactable = true;
        WeaponSelectPanel.Instance._canvasGroup.blocksRaycasts = true;
        //克隆生成武器UI->直接在WeaponSelectPanel的函数中调用
        //TODO:UIMgr优化需要/事件系统优化
        
        Instantiate(RoleSelectPanel.Instance._roleDetailGameObject, WeaponSelectPanel.Instance._weaponDetailTransform);
        WeaponSelectPanel.Instance._weaponDetailGameObject.SetActive(true);
    }
    //鼠标移入
    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207/255f, 207/255f , 207/255f);
        RoleSelectPanel.Instance.RenewUI(roleData);

    }
    

 
    //鼠标移出
    public void OnPointerExit(PointerEventData eventData)
    {
        _backImage.color = new Color(34/255f, 34/255f , 34/255f);
    }
}
