using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 角色选择面板（在 02-LevelSelect 场景中）。
/// 使用 GameManager 中已加载的 roleDatas，不再重复读 JSON。
/// 监听 Select_RoleChosen 事件，由自身负责"收起自己→展示武器面板"的流程，
/// 而非由子条目 RoleUI 直接操作其他面板（降低耦合）。
/// </summary>
public class RoleSelectPanel : BaseMgrMono<RoleSelectPanel>
{
    [SerializeField] private GameObject  _rolePrefab;
    [SerializeField] private Transform   _roleListTransform;
    [SerializeField] private Image       _roleImage;
    [SerializeField] private TextMeshProUGUI _roleName;
    [SerializeField] private TextMeshProUGUI _roleDescription;
    [SerializeField] private TextMeshProUGUI _recordText;

    public CanvasGroup  _canvasGroup;
    public GameObject   _roleDetailGameObject;

    public override void Awake()
    {
        base.Awake();
        // 使用 GameManager 中已集中加载的角色数据，避免重复 IO
        var roleDataList = GameManager.Instance.roleDatas;
        foreach (RoleData data in roleDataList)
        {
            RoleUI obj = Instantiate(_rolePrefab, _roleListTransform).GetComponent<RoleUI>();
            obj.SetRoleData(data);
        }
    }

    private void Start()
    {
        // 订阅角色选定事件：由本面板负责面板间的跳转逻辑
        EventCenter.Instance.AddEventListener<RoleData>(E_EventType.Select_RoleChosen, OnRoleChosen);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<RoleData>(E_EventType.Select_RoleChosen, OnRoleChosen);
    }

    // 当玩家选定角色后：隐藏角色面板，展示武器选择面板
    private void OnRoleChosen(RoleData data)
    {
        // 隐藏自身
        _canvasGroup.alpha          = 0;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;

        // 展示武器面板
        var wp = WeaponSelectPanel.Instance;
        wp._canvasGroup.alpha          = 1;
        wp._canvasGroup.interactable   = true;
        wp._canvasGroup.blocksRaycasts = true;

        // 将角色详情克隆到武器面板的详情区
        Instantiate(_roleDetailGameObject, wp._weaponDetailTransform);
        wp._weaponDetailGameObject.SetActive(true);
    }

    /// <summary>悬停时刷新右侧角色详情区。</summary>
    public void RenewUI(RoleData roleData)
    {
        _roleName.SetText(roleData.name);
        _roleDescription.SetText(roleData.describe);

        if (roleData.unlock == 0 && PlayerPrefs.GetInt(roleData.name, 1) == 1)
        {
            _roleImage.sprite = Resources.Load<Sprite>("Image/UI/锁");
            _roleDescription.SetText(roleData.unlockConditions);
            _roleName.SetText("???");
            _recordText.SetText("尚无记录");
        }
        else if (roleData.unlock == 1)
        {
            _roleImage.sprite = Resources.Load<Sprite>(roleData.avatar);
            _recordText.SetText(GetRecord(roleData.record));
        }
    }

    private string GetRecord(int rRecord)
    {
        return rRecord switch
        {
            0  => "通关危险0",
            1  => "通关危险1",
            2  => "通关危险2",
            3  => "通关危险3",
            4  => "通关危险4",
            5  => "通关危险5",
            _  => "尚无记录",
        };
    }
}
