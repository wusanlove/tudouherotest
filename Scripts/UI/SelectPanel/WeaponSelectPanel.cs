using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

/// <summary>
/// 武器选择面板（View）：展示武器列表；用户点击后触发 Select_WeaponChosen 事件。
/// </summary>
public class WeaponSelectPanel : BaseMgrMono<WeaponSelectPanel>
{
    public CanvasGroup _canvasGroup;
    public Transform   _weaponListTransform;
    public Transform   _weaponDetailTransform;

    [SerializeField] private GameObject      _weaponPrefab;
    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private Image           _weaponImage;
    [SerializeField] private TextMeshProUGUI _weaponDescription;

    public GameObject _weaponDetailGameObject;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        foreach (WeaponData it in ConfigService.Instance.Weapons)
        {
            WeaponUI weaponUI = Instantiate(_weaponPrefab, _weaponListTransform).GetComponent<WeaponUI>();
            weaponUI.SetWeaponData(it);
        }
    }

    /// <summary>鼠标悬停时更新详情区域（由 WeaponUI 调用）</summary>
    public void RenewUI(WeaponData weaponData)
    {
        _weaponName.SetText(weaponData.name);
        _weaponDescription.SetText(weaponData.describe);
        _weaponImage.sprite = Resources.Load<Sprite>(weaponData.avatar);
    }
}
