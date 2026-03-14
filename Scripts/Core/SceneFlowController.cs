using SceneState;

/// <summary>
/// 场景流程控制器 — 整个项目唯一合法的"切场景"入口。
///
/// UI 层只能通过 EventCenter 触发 Flow_Xxx 事件，禁止直接调用 SceneManager。
/// SceneFlowController 监听这些事件，驱动 SceneStateController 完成异步场景加载。
///
/// 状态流转：
///   01-MainMenu ──Start──► 02-LevelSelect ──选完──► 03-GamePlay
///                                                        │
///                                                   ◄────┤ 波次完成
///                                                   04-Shop
///                                                        │
///                                                   ──下一波──► 03-GamePlay
///                                                   ──胜利/失败──► 01-MainMenu
///
/// 未来演进思路（DI）：将 SceneFlowController 注册为服务，
/// 替换 Instance 访问，便于测试与热更新。
/// </summary>
public class SceneFlowController : BaseMgr<SceneFlowController>
{
    private SceneFlowController()
    {
        Controller = new SceneStateController();
        RegisterFlowEvents();
        // 每帧驱动 SceneStateController（检测异步加载完成并调用 StateStart）
        MonoMgr.Instance.AddUpdateListener(Tick);
    }

    /// <summary>底层状态控制器，外部一般不需要直接访问。</summary>
    public SceneStateController Controller { get; private set; }

    private void RegisterFlowEvents()
    {
        EventCenter.Instance.AddEventListener(E_EventType.Flow_GoToMainMenu,   GoToMainMenu);
        EventCenter.Instance.AddEventListener(E_EventType.Flow_GoToLevelSelect, GoToLevelSelect);
        EventCenter.Instance.AddEventListener(E_EventType.Flow_GoToGamePlay,   GoToGamePlay);
        EventCenter.Instance.AddEventListener(E_EventType.Flow_GoToShop,       GoToShop);
    }

    /// <summary>
    /// Bootstrap 调用一次：不加载场景，直接以当前场景（MainMenu）初始化状态机。
    /// </summary>
    public void StartFromMainMenu()
    {
        Controller.SetState(new StartScene(Controller), isLoadScene: false);
    }

    // ── 场景跳转方法（由事件触发） ────────────────────────────────

    public void GoToMainMenu()   => Controller.SetState(new StartScene(Controller));
    public void GoToLevelSelect() => Controller.SetState(new SelectSecene(Controller));
    public void GoToGamePlay()   => Controller.SetState(new GameScene(Controller));
    public void GoToShop()       => Controller.SetState(new ShopScene(Controller));

    private void Tick() => Controller.StateUpdate();
}
