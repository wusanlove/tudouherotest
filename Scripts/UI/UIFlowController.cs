using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI 流程控制器（UI Mediator）。
///
/// 职责：
/// - 订阅来自各 Panel 的 UI 事件（Panel 只抛事件，不关心结果）。
/// - 决定哪个面板打开、哪个关闭（互斥逻辑集中在此）。
/// - 负责场景级别的跳转（不在 Panel 内部调 SceneManager）。
/// - 商店请求代理：将 Shop_RequestBuy / Shop_RequestRefresh 转发给 ShopController，
///   再将结果通过 EventCenter 广播，Panel 收到结果后自行刷新视图。
///
/// 边界：
/// - UIFlowController 不持有业务数据，也不操作 GameManager 字段（那是 ShopController 的事）。
/// - UIFlowController 挂在跨场景持久化的 GameObject 上，或在各场景中单独挂载均可。
/// </summary>
public class UIFlowController : BaseMgrMono<UIFlowController>
{
    public override void Awake()
    {
        base.Awake();
        RegisterEvents();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    // ── 事件注册 / 注销 ──────────────────────────────────────────────────────

    private void RegisterEvents()
    {
        var ec = EventCenter.Instance;
        ec.AddEventListener(E_EventType.UI_StartGame,           OnStartGame);
        ec.AddEventListener(E_EventType.UI_ConfirmSelection,    OnConfirmSelection);
        ec.AddEventListener(E_EventType.Shop_ProceedToNextWave, OnProceedToNextWave);
        ec.AddEventListener(E_EventType.Battle_PlayerDied,      OnPlayerDied);
        ec.AddEventListener(E_EventType.Battle_AllWavesCompleted, OnAllWavesCompleted);
        ec.AddEventListener<ItemData>(E_EventType.Shop_RequestBuy,    OnShopBuyRequest);
        ec.AddEventListener(E_EventType.Shop_RequestRefresh,          OnShopRefreshRequest);
    }

    private void UnregisterEvents()
    {
        var ec = EventCenter.Instance;
        ec.RemoveEventListener(E_EventType.UI_StartGame,           OnStartGame);
        ec.RemoveEventListener(E_EventType.UI_ConfirmSelection,    OnConfirmSelection);
        ec.RemoveEventListener(E_EventType.Shop_ProceedToNextWave, OnProceedToNextWave);
        ec.RemoveEventListener(E_EventType.Battle_PlayerDied,      OnPlayerDied);
        ec.RemoveEventListener(E_EventType.Battle_AllWavesCompleted, OnAllWavesCompleted);
        ec.RemoveEventListener<ItemData>(E_EventType.Shop_RequestBuy,    OnShopBuyRequest);
        ec.RemoveEventListener(E_EventType.Shop_RequestRefresh,          OnShopRefreshRequest);
    }

    // ── 主菜单 → 选择场景 ────────────────────────────────────────────────────

    private void OnStartGame()
    {
        SceneManager.LoadScene("02-LevelSelect");
    }

    // ── 选择场景 → 游戏场景 ──────────────────────────────────────────────────

    private void OnConfirmSelection()
    {
        SceneManager.LoadScene("03-GamePlay");
    }

    // ── 商店 → 游戏场景（下一波）──────────────────────────────────────────────

    private void OnProceedToNextWave()
    {
        SceneManager.LoadScene("03-GamePlay");
    }

    // ── 玩家死亡（展示失败面板，由 LevelControl 负责面板可见性）───────────────

    private void OnPlayerDied()
    {
        // LevelControl.BadGame() 已通过 Battle_PlayerDied 事件触发，
        // 此处预留给未来可能的全局 UI 逻辑（如模糊背景、全屏蒙版等）
    }

    // ── 全部波次通关 ─────────────────────────────────────────────────────────

    private void OnAllWavesCompleted()
    {
        // 同上，由 LevelControl.GoodGame() 处理面板，此处做扩展预留
    }

    // ── 商店购买代理 ─────────────────────────────────────────────────────────

    private void OnShopBuyRequest(ItemData item)
    {
        ShopBuyResult result = ShopController.Instance.TryBuy(item);
        // 广播结果，ItemCardUI / ShopPanel 各自监听并刷新自己
        EventCenter.Instance.EventTrigger<ShopBuyResult>(E_EventType.Shop_BuyResult, result);

        if (result.success)
        {
            // 通知 HUD 相关数值变化
            EventCenter.Instance.EventTrigger(E_EventType.HUD_MoneyChanged);
        }
    }

    // ── 商店刷新代理 ─────────────────────────────────────────────────────────

    private void OnShopRefreshRequest()
    {
        bool ok = ShopController.Instance.TryRefresh();
        EventCenter.Instance.EventTrigger<bool>(E_EventType.Shop_RefreshResult, ok);

        if (ok)
            EventCenter.Instance.EventTrigger(E_EventType.HUD_MoneyChanged);
    }
}
