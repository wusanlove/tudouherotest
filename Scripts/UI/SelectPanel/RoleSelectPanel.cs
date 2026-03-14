using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 角色选择面板 — 展示角色列表与详情。
/// 数据来自 ConfigService（不再自行加载 JSON）。
/// </summary>
public class RoleSelectPanel : BaseMgrMono<RoleSelectPanel>
{
    [SerializeField] private GameObject  _rolePrefab;
    [SerializeField] private Transform   _roleListTransform;
    [SerializeField] private Image       _roleImage;
    [SerializeField] private TextMeshProUGUI _roleName;
    [SerializeField] private TextMeshProUGUI _roleDescription;
    [SerializeField] private TextMeshProUGUI _recordText;

    public GameObject  _roleDetailGameObject;
    public CanvasGroup _canvasGroup;

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 数据来自 ConfigService 缓存，避免重复 IO
        List<RoleData> roles = ConfigService.Instance.Roles;
        if (roles == null) return;
        foreach (RoleData data in roles)
        {
            RoleUI obj = Instantiate(_rolePrefab, _roleListTransform).GetComponent<RoleUI>();
            obj.SetRoleData(data);
        }
    }

    /// <summary>刷新右侧角色详情区。</summary>
    public void RenewUI(RoleData roleData)
    {
        bool locked = roleData.unlock == 0 && PlayerPrefs.GetInt(roleData.name, 1) == 1;
        if (locked)
        {
            _roleImage.sprite = Resources.Load<Sprite>("Image/UI/锁");
            _roleName.SetText("???");
            _roleDescription.SetText(roleData.unlockConditions);
            _recordText.SetText("尚无记录");
        }
        else
        {
            _roleImage.sprite = Resources.Load<Sprite>(roleData.avatar);
            _roleName.SetText(roleData.name);
            _roleDescription.SetText(roleData.describe);
            _recordText.SetText(GetRecord(roleData.record));
        }
    }

    private static string GetRecord(int record)
    {
        if (record < 0) return "尚无记录";
        return record == 0 ? "通关危险0" : $"通关危险{record}";
    }
}
