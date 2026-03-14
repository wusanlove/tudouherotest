using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏 HUD 面板 — 显示 HP / 经验 / 金币 / 倒计时 / 波次。
/// 通过 EventCenter 订阅游戏状态事件来刷新 UI，
/// 不再被 Player / EnemyBase / LevelControl 直接调用。
/// </summary>
public class GamePanel : BaseMgrMono<GamePanel>
{
    public CanvasGroup _canvasGroup;

    public Slider       _hpSlider;
    public Slider       _expSlider;
    public TMP_Text     _moenyCount;
    public TMP_Text     _expCount;
    public TMP_Text     _hpCount;
    public TMP_Text     _countDown;
    public TMP_Text     _waveCount;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup.alpha          = 1;
        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;

        // 订阅游戏状态事件
        EventCenter.Instance.AddEventListener<float>(E_EventType.Game_PlayerHpChanged,    OnHpChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.Game_PlayerMoneyChanged, OnMoneyChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.Game_PlayerExpChanged,   OnExpChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.Game_WaveTimerChanged,   OnWaveTimerChanged);
    }

    private void OnDestroy()
    {
        // 面板销毁时取消订阅，防止跨场景内存泄漏
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Game_PlayerHpChanged,    OnHpChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Game_PlayerMoneyChanged, OnMoneyChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Game_PlayerExpChanged,   OnExpChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.Game_WaveTimerChanged,   OnWaveTimerChanged);
    }

    private void Start()
    {
        RenewWaveCount();
        // 初始值从 GameManager 读取
        OnHpChanged(GameManager.Instance.hp);
        OnMoneyChanged(GameManager.Instance.money);
        OnExpChanged(GameManager.Instance.exp);
    }

    // ── 事件响应 ──────────────────────────────────────────────────

    private void OnHpChanged(float hp)
    {
        float maxHp = GameManager.Instance.propData.maxHp;
        _hpCount.text    = hp + "/" + maxHp;
        _hpSlider.value  = maxHp > 0 ? hp / maxHp : 0;
    }

    private void OnMoneyChanged(float money)
    {
        _moenyCount.text = money.ToString();
    }

    private void OnExpChanged(float exp)
    {
        _expSlider.value = exp % 12 / 12;
        _expCount.text   = "LV." + (int)(exp / 12);
    }

    private void OnWaveTimerChanged(float time)
    {
        _countDown.text  = time.ToString("F0");
        _countDown.color = time <= 5f ? new Color(1f, 0f, 0f) : Color.white;
    }

    // ── 公开工具方法（保留，其他地方可能直接调） ────────────────

    public void RenewWaveCount()
    {
        _waveCount.text = "第" + GameManager.Instance.currentWave + "关";
    }

    // 以下方法保留兼容，内部改为触发对应事件
    public void RenewMoney() => OnMoneyChanged(GameManager.Instance.money);
    public void RenewHp()    => OnHpChanged(GameManager.Instance.hp);
    public void RenewExp()   => OnExpChanged(GameManager.Instance.exp);
}
