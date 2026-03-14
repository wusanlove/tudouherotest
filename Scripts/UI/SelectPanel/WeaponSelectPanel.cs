using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 武器选择面板 — 展示武器列表与详情。
/// 数据来自 ConfigService（不再自行加载 JSON）。
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
        List<WeaponData> weapons = ConfigService.Instance.Weapons;
        if (weapons == null) return;
        foreach (WeaponData data in weapons)
        {
            WeaponUI ui = Instantiate(_weaponPrefab, _weaponListTransform).GetComponent<WeaponUI>();
            ui.SetWeaponData(data);
        }
    }

    /// <summary>刷新右侧武器详情区。</summary>
    public void RenewUI(WeaponData weaponData)
    {
        _weaponName.SetText(weaponData.name);
        _weaponDescription.SetText(weaponData.describe);
        _weaponImage.sprite = Resources.Load<Sprite>(weaponData.avatar);
    }
}
