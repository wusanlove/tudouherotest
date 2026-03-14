using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

/// <summary>
/// 角色选择面板（View）：展示角色列表；用户点击角色后触发 Select_RoleChosen 事件，
/// 由 SelectSceneMediator 负责切换面板，面板自身不知道其他面板的存在。
/// </summary>
public class RoleSelectPanel : BaseMgrMono<RoleSelectPanel>
{
    [SerializeField] private GameObject _rolePrefab;
    [SerializeField] private Transform  _roleListTransform;

    [SerializeField] private Image           _roleImage;
    [SerializeField] private TextMeshProUGUI _roleName;
    [SerializeField] private TextMeshProUGUI _roleDescription;
    [SerializeField] private TextMeshProUGUI _recordText;

    // 公开给 SelectSceneMediator 使用的 Detail 对象引用
    public GameObject  _roleDetailGameObject;
    public CanvasGroup _canvasGroup;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // 从 ConfigService 获取角色列表（不再自行读 JSON）
        foreach (RoleData data in ConfigService.Instance.Roles)
        {
            RoleUI obj = Instantiate(_rolePrefab, _roleListTransform).GetComponent<RoleUI>();
            obj.SetRoleData(data);
        }
    }

    /// <summary>鼠标悬停时更新详情区域（由 RoleUI 调用）</summary>
    public void RenewUI(RoleData roleData)
    {
        _roleName.SetText(roleData.name);
        _roleDescription.SetText(roleData.describe);

        bool isLocked = roleData.unlock == 0 && !SaveService.Instance.IsRoleUnlocked(roleData.name);
        if (isLocked)
        {
            _roleImage.sprite = Resources.Load<Sprite>("Image/UI/锁");
            _roleDescription.SetText(roleData.unlockConditions);
            _roleName.SetText("???");
            _recordText.SetText("尚无记录");
        }
        else
        {
            _roleImage.sprite = Resources.Load<Sprite>(roleData.avatar);
            _recordText.SetText(GetRecord(SaveService.Instance.GetBestRecord(roleData.name)));
        }
    }

    private string GetRecord(int record)
    {
        return record < 0 ? "尚无记录" : $"通关危险{record}";
    }
}
