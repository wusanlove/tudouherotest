using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopPanel : BaseMgrMono<ShopPanel>
{


    public Button _startButton; //进入下一关按钮
    public Button _refreshButton; //刷新商店按钮
    public TMP_Text _shopText; //商店信息
    public TMP_Text _moneyText; //金币信息
    public TMP_Text _weaponTitle; //武器信息

    public Transform _attrLayout; //属性布局
    public Transform _propsLayout; //道具布局
    public Transform _weaponsLayout; //武器布局
    public Transform _itemLayout; //选择道具布局

    public List<ItemData> props = new List<ItemData>();//商店中显示的道具列表


    public override void Awake()
    { 
        base.Awake();
        _refreshButton.onClick.AddListener(RefreshItem);
        
    }

    // Start is called before the first frame update
    void Start()
    {

        //商店信息
        _shopText.text = "商店 (第" + (GameManager.Instance.currentWave - 1) + "波)";
        //开始按钮
        _startButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
            "出发 (第" + (GameManager.Instance.currentWave) + "波)";
        _startButton.onClick.AddListener(() => { SceneManager.LoadScene("03-GamePlay"); });
        //金币值
        _moneyText.text = GameManager.Instance.money.ToString();

        SetAttrUI(); //设置属性UI
        ShowCurrentProp();//显示当前道具
        ShowCurrentWeapon();
        RandomProps(); //随机生成商店道具
        SetWeaponSlotIndex();



    }
    private void SetWeaponSlotIndex()
    {
        int count = _weaponsLayout.childCount;
        for (int i = 0; i < count; i++)
        {
            _weaponsLayout.GetChild(i).GetComponent<WeaponSlot>().slotCount = i;
        }
    }
    private void RefreshItem()
    {
        if (GameManager.Instance.money < 3 )
        {
            return;
        }

        GameManager.Instance.money -= 3; //扣钱
        _moneyText.text = GameManager.Instance.money.ToString();  //更新金币UI
        RandomProps();  //重新随机

    }
    private void SetAttrUI()
    {
        //TODO:优化不用按顺序设置
        //设置等级
        _attrLayout.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
            ((int)(GameManager.Instance.exp / 12)).ToString();
        //最大生命值
        _attrLayout.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.maxHp.ToString();

        //生命再生
        _attrLayout.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.revive.ToString();

        //近战伤害
        _attrLayout.GetChild(3).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.short_damage.ToString();


        //远程伤害
        _attrLayout.GetChild(4).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.long_damage.ToString();

        //暴击率
        _attrLayout.GetChild(5).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.critical_strikes_probability.ToString();

        //拾取范围
        _attrLayout.GetChild(6).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.pickRange.ToString();

        //速度
        _attrLayout.GetChild(7).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.speedPer.ToString();


        //收获
        _attrLayout.GetChild(8).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.harvest.ToString();

        //经验倍率
        _attrLayout.GetChild(9).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.expMuti.ToString();

        //商店折扣
        _attrLayout.GetChild(10).GetChild(2).GetComponent<TMP_Text>().text =
            GameManager.Instance.propData.shopDiscount.ToString();


    }

    private void ShowCurrentProp()
    {
        int count = _propsLayout.childCount;

        for (int i = 0; i < count; i++)
        {
            if ( i < GameManager.Instance.currentProps.Count)
            {
                //设置ui
                _propsLayout.GetChild(i).GetChild(0).GetComponent<Image>().enabled = true;
                _propsLayout.GetChild(i).GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.propsAtlas.GetSprite(GameManager.Instance.currentProps[i].name);
                
            }
            else
            {
                //空格子
                _propsLayout.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
            }
            
        }
        
    }
    
    public void ShowCurrentWeapon()
    {
        int count = _weaponsLayout.childCount;

        for (int i = 0; i < count; i++)
        {
            WeaponSlot slot = _weaponsLayout.GetChild(i).GetComponent<WeaponSlot>();
            
            //如果该位置有武器
            if ( i < GameManager.Instance.currentWeapons.Count)
            {
            
                //设置ui
                slot._weaponIcon.enabled = true;
                slot.weaponData = GameManager.Instance.currentWeapons[i];
                slot._weaponIcon.sprite = Resources.Load<Sprite>(slot.weaponData.avatar);

                //判断等级 设置不同背景色
                switch (slot.weaponData.grade)
                {
                    case 1:
                        slot._weaponBG.color = new Color(29 / 255f, 29 / 255f, 29 / 255f);
                        break;
                    case 2:
                        slot._weaponBG.color = new Color(58 / 255f, 83 / 255f, 99 / 255f);
                        break;
                    case 3:
                        slot._weaponBG.color = new Color(79 / 255f, 58 / 255f, 99 / 255f);
                        break;
                    case 4:
                        slot._weaponBG.color = new Color(99 / 255f, 50 / 255f, 50 / 255f);
                        break;
                }
            }
            //该位置没有武器
            else
            {
                //空格子3
                slot._weaponIcon.enabled = false;
                slot._weaponBG.color = new Color(29 / 255f, 29 / 255f, 29 / 255f);
                slot.weaponData = null;
            }
        }
        
        //设置武器数量
        _weaponTitle.text = "武器(" + GameManager.Instance.currentWeapons.Count + "/" +
                            GameManager.Instance.propData.slot + ")";
    }
    
    private void RandomProps()
    {
        props.Clear();

        //获取要生成的道具的数量
        int propCount = Random.Range(2, 4); //排除1 
        
        //随机生成道具
        for (int i = 0; i < propCount; i++)
        {
            props.Add(GameManager.Instance.RandomOne(GameManager.Instance.propDatas) as ItemData);
        }
        
        //随机武器
        for (int i = 0; i < 4 - propCount; i++)
        {
            props.Add(GameManager.Instance.RandomOne(GameManager.Instance.weaponDatas) as ItemData);
        }
        
        
        ShowPropUI();
    }

    private void ShowPropUI()
    {
        int i = 0;

        foreach (ItemData item in props)
        {
            
            //设置数据
            _itemLayout.GetChild(i).GetComponent<ItemCardUI>()._canvasGroup.alpha = 1;  //设置透明度
            _itemLayout.GetChild(i).GetComponent<ItemCardUI>()._canvasGroup.interactable = true; //设置交互
            _itemLayout.GetChild(i).GetComponent<ItemCardUI>().SetItemData(item);
            i++;

        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    public bool Shopping(ItemData itemData)
    {
        if (GameManager.Instance.money < 3 )
        {
            return false;
        }

        //武器是否超过 6
        if (itemData is WeaponData && GameManager.Instance.currentWeapons.Count >= GameManager.Instance.propData.slot )
        {
            return false;
        }
        
        //道具是否超过 20 
        if (itemData is PropData && GameManager.Instance.currentProps.Count >= 20 )
        {
            return false;
        }
        if(GameManager.Instance.money < itemData.price)
        {
            return false;
        }
        GameManager.Instance.money -= itemData.price; //扣钱
        _moneyText.text = GameManager.Instance.money.ToString(); //更新金币UI
        
        
        //如果是武器
        if (itemData is WeaponData)
        {
            WeaponData tempItem = JsonConvert.DeserializeObject<WeaponData>(JsonConvert.SerializeObject(itemData));
            GameManager.Instance.currentWeapons.Add(tempItem );
            ShowCurrentWeapon();
        }
        //如果是道具
        else
        { 
            PropData tempItem = JsonConvert.DeserializeObject<PropData>(JsonConvert.SerializeObject(itemData));
            
            GameManager.Instance.currentProps.Add(tempItem);
            ShowCurrentProp();
            
            //注入关联属性中
            GameManager.Instance.FusionAttr(tempItem);
            SetAttrUI(); //更新属性面板
        }

        return true;
        

    }
}