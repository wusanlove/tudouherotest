using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSelectPanel : BaseMgrMono<WeaponSelectPanel>
{
    public CanvasGroup _canvasGroup;
    public Transform _weaponListTransform;
    public Transform _weaponDetailTransform;
    [SerializeField] private GameObject _weaponPrefab;

    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private Image _weaponImage;
    [SerializeField] private TextMeshProUGUI _weaponDescription;

    public GameObject _weaponDetailGameObject;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
        // 复用 GameManager 已加载的武器列表，避免重复读取 JSON
    }

    private void Start()
    {
        foreach (var it in GameManager.Instance.weaponDatas)
        {
            WeaponUI weaponUI = Instantiate(_weaponPrefab, _weaponListTransform).GetComponent<WeaponUI>();
            weaponUI.SetWeaponData(it);
        }
    }

    public void RenewUI(WeaponData weaponData)
    {
        _weaponName.SetText(weaponData.name);
        _weaponDescription.SetText(weaponData.describe);
        _weaponImage.sprite = Resources.Load<Sprite>(weaponData.avatar);
    }
}
