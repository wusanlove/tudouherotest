using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using Newtonsoft.Json;

    public class RoleSelectPanel : BaseMgrMono<RoleSelectPanel>
    {
        List<RoleData> roleDataList;
        TextAsset roleTextAsset;
        [SerializeField] private GameObject _rolePrefab;
        public GameObject _roleDetailGameObject;
        [SerializeField] private Transform _roleListTransform;

        [SerializeField] private Image _roleImage;
        [SerializeField] private TextMeshProUGUI _roleName;
        [SerializeField] private TextMeshProUGUI _roleDescription;
        [SerializeField] private TextMeshProUGUI _recordText;
        
        public  CanvasGroup _canvasGroup;

        public override void Awake()
        {
            base.Awake();
            // //单例赋值
            // _rolePrefab = Resources.Load<GameObject>("Prefabs/UI/roleSelect"); //加载方式需要优化，不方便改变路劲（所以还是Addressable好）
            // _roleListTransform = transform.Find("RoleSelectList");
            // _roleImage = transform.Find("RoleDetail/Avator/imgRole").GetComponent<Image>();
            // _roleName = transform.Find("RoleDetail/txtRoleName").GetComponent<TextMeshProUGUI>();
            // _roleDescription = transform.Find("RoleDetail/txtRoleDescription").GetComponent<TextMeshProUGUI>();
            //json数据加载
            roleTextAsset = Resources.Load<TextAsset>("Data/role");
            roleDataList = JsonConvert.DeserializeObject<List<RoleData>>(roleTextAsset.text);
        }

        private void Start()
        {
            foreach (RoleData data in roleDataList)
            {
                //实例化角色UI预制体
                RoleUI obj = Instantiate(_rolePrefab, _roleListTransform).GetComponent<RoleUI>();
                obj.SetRoleData(data);
    
            }
        }

        public void RenewUI(RoleData roleData)
        {
            this._roleName.SetText(roleData.name);
            this._roleDescription.SetText(roleData.describe);
            if (roleData.unlock == 0 && SaveProgressService.Instance.GetRoleUnlock(roleData.name) == 0)
            {
                _roleImage.sprite = Resources.Load<Sprite>("Image/UI/锁");
                this._roleDescription.SetText(roleData.unlockConditions);
                this._roleName.SetText("???");
                this._recordText.SetText("尚无记录");
                
            }
            else if(roleData.unlock == 1 )
            {
                _roleImage.sprite = Resources.Load<Sprite>(roleData.avatar);
                this._recordText.SetText(GetRecord(roleData.record));
            }
          
        }
           
        //获取通关记录
        private string GetRecord(int rRecord)
        {
            string result = "";
        
            switch (rRecord)
            {
                case -1:
                    result = "尚无记录";
                    break;
                case 0:
                    result = "通关危险0";
                    break;
                case 1:
                    result = "通关危险1";
                    break;
                case 2:
                    result = "通关危险2";
                    break;
                case 3:
                    result = "通关危险3";
                    break;
                case 4:
                    result = "通关危险4";
                    break;
                case 5:
                    result = "通关危险5";
                    break;
            }


            return result;
        }


    }
