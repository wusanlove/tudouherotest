using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗 HUD（View）：订阅 EventCenter 事件，刷新 UI，不持有对 Player/LevelControl 的引用。
/// </summary>
public class GamePanel : BaseMgrMono<GamePanel>
{
    public CanvasGroup _canvasGroup;

    public Slider      _hpSlider;
    public Slider      _expSlider;
    public TMP_Text    _moenyCount;
    public TMP_Text    _expCount;
    public TMP_Text    _hpCount;
    public TMP_Text    _countDown;
    public TMP_Text    _waveCount;

    public override void Awake()
    {
        base.Awake();
        var cg = GetComponent<CanvasGroup>();
        cg.alpha = 1; cg.interactable = true; cg.blocksRaycasts = true;

        // 订阅事件
        EventCenter.Instance.AddEventListener<float>(E_EventType.GamePlay_HpChanged,     OnHpChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.GamePlay_ExpChanged,    OnExpChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.GamePlay_MoneyChanged,  OnMoneyChanged);
        EventCenter.Instance.AddEventListener<int>  (E_EventType.GamePlay_WaveChanged,   OnWaveChanged);
        EventCenter.Instance.AddEventListener<float>(E_EventType.GamePlay_CountDown,     OnCountDown);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.GamePlay_HpChanged,    OnHpChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.GamePlay_ExpChanged,   OnExpChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.GamePlay_MoneyChanged, OnMoneyChanged);
        EventCenter.Instance.RemoveEventListener<int>  (E_EventType.GamePlay_WaveChanged,  OnWaveChanged);
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.GamePlay_CountDown,    OnCountDown);
    }

    private void Start()
    {
        // 初始化显示（拉取当前状态快照）
        OnHpChanged(GameManager.Instance.hp);
        OnExpChanged(GameManager.Instance.exp);
        OnMoneyChanged(GameManager.Instance.money);
        OnWaveChanged(GameManager.Instance.currentWave);
    }

    // ── 事件处理 ─────────────────────────────────────────
    private void OnHpChanged(float hp)
    {
        _hpCount.text   = $"{hp:F0}/{GameManager.Instance.propData.maxHp:F0}";
        _hpSlider.value = hp / GameManager.Instance.propData.maxHp;
    }

    private void OnExpChanged(float exp)
    {
        _expSlider.value = exp % 12 / 12f;
        _expCount.text   = $"LV.{(int)(exp / 12)}";
    }

    private void OnMoneyChanged(float money)
    {
        _moenyCount.text = money.ToString("F0");
    }

    private void OnWaveChanged(int wave)
    {
        _waveCount.text = $"第{wave}关";
    }

    private void OnCountDown(float time)
    {
        _countDown.text  = time.ToString("F0");
        _countDown.color = time <= 5f
            ? new Color(1f, 0f, 0f)
            : new Color(1f, 1f, 1f);
    }

    // ── 保留供 LevelControl 直接调用的倒计时（Update 中使用）─
    public void RenewCountDown(float time)
    {
        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_CountDown, time);
    }

    // ── 以下保留向后兼容（可逐步移除）─────────────────────
    public void RenewMoney()  => OnMoneyChanged(GameManager.Instance.money);
    public void RenewHp()     => OnHpChanged(GameManager.Instance.hp);
    public void RenewExp()    => OnExpChanged(GameManager.Instance.exp);
    public void RenewWaveCount() => OnWaveChanged(GameManager.Instance.currentWave);
}
