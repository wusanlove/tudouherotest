using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleSelectPanel : BaseMgrMono<RoleSelectPanel>
{
    [SerializeField] private GameObject _rolePrefab;
    public GameObject _roleDetailGameObject;
    [SerializeField] private Transform _roleListTransform;

    [SerializeField] private Image _roleImage;
    [SerializeField] private TextMeshProUGUI _roleName;
    [SerializeField] private TextMeshProUGUI _roleDescription;
    [SerializeField] private TextMeshProUGUI _recordText;

    public CanvasGroup _canvasGroup;

    public override void Awake()
    {
        base.Awake();
        // 复用 GameManager 已加载的角色列表，避免重复读取 JSON
    }

    private void Start()
    {
        foreach (RoleData data in GameManager.Instance.roleDatas)
        {
            RoleUI obj = Instantiate(_rolePrefab, _roleListTransform).GetComponent<RoleUI>();
            obj.SetRoleData(data);
        }
    }

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

    private string GetRecord(int record)
    {
        if (record == -1) return "尚无记录";
        return record >= 0 && record <= 5 ? $"通关危险{record}" : "尚无记录";
    }
}
