using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel :BaseMgrMono<GamePanel>
{   //TODO:使用BasePanel基类并优化UI管理器 统一管理UI面板的打开关闭
    public CanvasGroup _canvasGroup;
    
    public Slider _hpSlider;
    public Slider _expSlider;
    public TMP_Text _moenyCount; //金币
    public TMP_Text _expCount; //等级 LV.0
    public TMP_Text _hpCount; //生命值 10/15
    public TMP_Text _countDown; //关卡倒计时 15
    public TMP_Text _waveCount; //波次 15

    public override void Awake()
    {
        base.Awake();
        this.GetComponent<CanvasGroup>().alpha = 1;
        this.GetComponent<CanvasGroup>().interactable = true;
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        //更新经验条
        RenewExp();
        //更新生命值
        RenewHp();
        //更新金币
        RenewMoney();
        //更新波次信息
        RenewWaveCount();
    }

    public void RenewMoney()
    {
        _moenyCount.text = GameManager.Instance.money.ToString();
    }

    public void RenewHp()
    {
        _hpCount.text = GameManager.Instance.hp + "/" + GameManager.Instance.propData.maxHp;
        _hpSlider.value = GameManager.Instance.hp / GameManager.Instance.propData.maxHp;
    }

    public void RenewExp()
    {
        // 25, 12 2级 ,1   1/12 = 0.1
        _expSlider.value = GameManager.Instance.exp % 12 / 12;
        _expCount.text = "LV." + (int)(GameManager.Instance.exp / 12);
    }
    
    

    // Update is called once per frame
    void Update()
    {
      
        RenewCountDown(LevelControl.Instance.waveTimer);
    }
    
    //更新倒计时
    public void RenewCountDown(float time)
    {
        _countDown.text = time.ToString("F0");

        //最后5秒 颜色变成红色
        if (time <= 5 )
        {
            _countDown.color = new Color(255 / 255f, 0, 0);
        }
    }
    
    //更新波次
    public void RenewWaveCount()
    {
        _waveCount.text = "第" + GameManager.Instance.currentWave.ToString() + "关";
    }
    
    
    
    
}
