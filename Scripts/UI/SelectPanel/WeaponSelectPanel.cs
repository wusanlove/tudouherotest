using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 武器选择面板（在 02-LevelSelect 场景中）。
/// 使用 GameManager 中已加载的 weaponDatas，不再重复读 JSON。
/// 监听 Select_WeaponChosen 事件，由自身负责"收起自己→展示难度面板"。
/// </summary>
public class WeaponSelectPanel : BaseMgrMono<WeaponSelectPanel>
{
    public  CanvasGroup _canvasGroup;
    public  Transform   _weaponListTransform;
    public  Transform   _weaponDetailTransform;
    public  GameObject  _weaponDetailGameObject;

    [SerializeField] private GameObject      _weaponPrefab;
    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private Image           _weaponImage;
    [SerializeField] private TextMeshProUGUI _weaponDescription;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();

        // 使用 GameManager 中已集中加载的武器数据
        foreach (WeaponData data in GameManager.Instance.weaponDatas)
        {
            WeaponUI ui = Instantiate(_weaponPrefab, _weaponListTransform).GetComponent<WeaponUI>();
            ui.SetWeaponData(data);
        }
    }

    private void Start()
    {
        EventCenter.Instance.AddEventListener<WeaponData>(E_EventType.Select_WeaponChosen, OnWeaponChosen);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<WeaponData>(E_EventType.Select_WeaponChosen, OnWeaponChosen);
    }

    // 当玩家选定武器后：隐藏武器面板，展示难度选择面板
    private void OnWeaponChosen(WeaponData data)
    {
        _canvasGroup.alpha          = 0;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;

        var dp = DifficultySelectPanel.Instance;
        dp._canvasGroup.alpha          = 1;
        dp._canvasGroup.interactable   = true;
        dp._canvasGroup.blocksRaycasts = true;

        // 将角色详情和武器详情克隆到难度面板的详情区
        Instantiate(RoleSelectPanel.Instance._roleDetailGameObject, dp._difficultyDetailTransform);
        Instantiate(_weaponDetailGameObject, dp._difficultyDetailTransform);
        dp._difficultyDetailGameObject.SetActive(true);
    }

    /// <summary>悬停时刷新右侧武器详情区。</summary>
    public void RenewUI(WeaponData weaponData)
    {
        _weaponName.SetText(weaponData.name);
        _weaponDescription.SetText(weaponData.describe);
        _weaponImage.sprite = Resources.Load<Sprite>(weaponData.avatar);
    }
}
