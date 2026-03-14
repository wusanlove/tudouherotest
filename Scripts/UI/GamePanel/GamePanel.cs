using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗 HUD 面板（View 层）。
/// 监听 EventCenter 中的 HUD_MoneyChanged / HUD_HpChanged / HUD_ExpChanged 事件刷新显示，
/// 不再被 Player/LevelControl 直接调用（解耦游戏逻辑层 ↔ UI 层）。
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
        var cg = GetComponent<CanvasGroup>();
        cg.alpha          = 1;
        cg.interactable   = true;
        cg.blocksRaycasts = true;
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(E_EventType.HUD_MoneyChanged, RenewMoney);
        EventCenter.Instance.AddEventListener(E_EventType.HUD_HpChanged,    RenewHp);
        EventCenter.Instance.AddEventListener(E_EventType.HUD_ExpChanged,   RenewExp);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(E_EventType.HUD_MoneyChanged, RenewMoney);
        EventCenter.Instance.RemoveEventListener(E_EventType.HUD_HpChanged,    RenewHp);
        EventCenter.Instance.RemoveEventListener(E_EventType.HUD_ExpChanged,   RenewExp);
    }

    private void Start()
    {
        RenewExp();
        RenewHp();
        RenewMoney();
        RenewWaveCount();
    }

    // ── HUD 刷新方法（可被外部代码安全调用，也响应事件）──────────────────────

    public void RenewMoney()
    {
        _moenyCount.text = GameManager.Instance.money.ToString();
    }

    public void RenewHp()
    {
        float hp    = GameManager.Instance.hp;
        float maxHp = GameManager.Instance.propData.maxHp;
        _hpCount.text    = $"{hp}/{maxHp}";
        _hpSlider.value  = maxHp > 0 ? hp / maxHp : 0;
    }

    public void RenewExp()
    {
        float exp = GameManager.Instance.exp;
        _expSlider.value = exp % 12 / 12f;
        _expCount.text   = $"LV.{(int)(exp / 12)}";
    }

    public void RenewCountDown(float time)
    {
        _countDown.text  = time.ToString("F0");
        _countDown.color = time <= 5
            ? new Color(1f, 0f, 0f)
            : Color.white;
    }

    public void RenewWaveCount()
    {
        _waveCount.text = $"第{GameManager.Instance.currentWave}关";
    }

    private void Update()
    {
        // 倒计时需每帧更新，直接从 LevelControl 读（HUD 与 LevelControl 同场景，可接受）
        if (LevelControl.Instance != null)
            RenewCountDown(LevelControl.Instance.waveTimer);
    }
}
