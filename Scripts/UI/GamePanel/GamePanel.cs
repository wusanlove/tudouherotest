using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel :BaseMgrMono<GamePanel>
{
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

        // 订阅玩家状态事件，解耦 Player/Enemy → GamePanel 直接调用
        EventCenter.Instance.AddEventListener<float>(E_EventType.Player_HpChanged,   OnHpChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.Player_MoneyChanged, OnMoneyChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.Player_ExpChanged,   OnExpChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.Wave_TimerUpdated,   OnTimerUpdated);
        EventCenter.Instance.AddEventListener<int>  (E_EventType.Wave_InfoUpdated,    OnWaveInfoUpdated);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Player_HpChanged,   OnHpChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Player_MoneyChanged, OnMoneyChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Player_ExpChanged,   OnExpChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Wave_TimerUpdated,   OnTimerUpdated);
        EventCenter.Instance.RemoveEventListener<int>  (E_EventType.Wave_InfoUpdated,    OnWaveInfoUpdated);
    }

    void Start()
    {
        RenewExp();
        RenewHp();
        RenewMoney();
        RenewWaveCount();
    }

    // ──────────────── 事件回调 ────────────────

    private void OnHpChanged(float hp)      => RenewHp();
    private void OnMoneyChanged(float money) => RenewMoney();
    private void OnExpChanged(float exp)     => RenewExp();
    private void OnTimerUpdated(float time)  => RenewCountDown(time);
    private void OnWaveInfoUpdated(int wave) => RenewWaveCount();

    // ──────────────── UI 刷新方法 ────────────────

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
        _expSlider.value = GameManager.Instance.exp % 12 / 12;
        _expCount.text = "LV." + (int)(GameManager.Instance.exp / 12);
    }
    
    //更新倒计时
    public void RenewCountDown(float time)
    {
        _countDown.text = time.ToString("F0");
        _countDown.color = time <= 5 ? new Color(1f, 0f, 0f) : Color.white;
    }
    
    //更新波次
    public void RenewWaveCount()
    {
        _waveCount.text = "第" + GameManager.Instance.currentWave.ToString() + "关";
    }
}
