using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardUI : MonoBehaviour
{
    [SerializeField]private ItemData itemData; //数据
    [SerializeField]private Button _btnBuy; //按钮
    public CanvasGroup _canvasGroup; //设置透明度
    
    [SerializeField]private TextMeshProUGUI _itemName;
    [SerializeField]private TextMeshProUGUI _itemType;
    [SerializeField]private TextMeshProUGUI _itemDesc;
    [SerializeField]private TextMeshProUGUI _itemPrice;
    [SerializeField]private Image _itemIcon;
    private  void Awake()
    {
     
    }

   
    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        _itemName.text = itemData.name;
        if (itemData is WeaponData)
        {
            _itemType.text = "武器";
            _itemIcon.sprite = Resources.Load<Sprite>(itemData.avatar);
        }
        else if (itemData is PropData)
        {
            _itemType.text = "道具";
            _itemIcon.sprite =GameManager.Instance.propsAtlas.GetSprite(itemData.name);
        }
        _itemDesc.text = itemData.describe;
        _itemPrice.text = itemData.price.ToString();
        
        
    }
    // Start is called before the first frame update
    void Start()
    {
        _btnBuy.onClick.AddListener(() =>
        {
            //购物  
            bool result = ShopPanel.Instance.Shopping(itemData);
            if (result)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
            }
           
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}