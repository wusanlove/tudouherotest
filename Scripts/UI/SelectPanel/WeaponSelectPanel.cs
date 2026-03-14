using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

public class WeaponSelectPanel : BaseMgrMono<WeaponSelectPanel>
{ 
    public CanvasGroup _canvasGroup;
   public Transform _weaponListTransform;
   public Transform _weaponDetailTransform;
   [SerializeField]private GameObject _weaponPrefab;
   //武器Detail
   [SerializeField]private TextMeshProUGUI _weaponName;
   [SerializeField] private Image _weaponImage;
   [SerializeField] private TextMeshProUGUI _weaponDescription;
   
   List<WeaponData> weaponDataList;
   public GameObject _weaponDetailGameObject;

   public override void Awake()
    {
        base.Awake(); 
        _canvasGroup = GetComponent<CanvasGroup>();
        // 通过 ConfigService 集中加载武器数据
        weaponDataList = ConfigService.Instance.LoadWeapons();
        
        
    }

    private void Start()
    {
        if (weaponDataList != null)
        {
             foreach (var it in weaponDataList)
            {
                
                WeaponUI weaponUI=Instantiate(_weaponPrefab, _weaponListTransform).GetComponent<WeaponUI>();
                weaponUI.SetWeaponData(it);

            }
        }
       
        
        
    }

    public void RenewUI(WeaponData weaponData)
    {
        this._weaponName.SetText(weaponData.name);
        this._weaponDescription.SetText(weaponData.describe);
        this._weaponImage.sprite= Resources.Load<Sprite>(weaponData.avatar);
          
    }
}
