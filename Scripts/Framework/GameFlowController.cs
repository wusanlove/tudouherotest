using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏流程总控制器（单例 MonoBehaviour，跨场景持久）。
/// 封装 SceneStateController；外部只调 Go* 系列方法，不直接操作 SceneManager。
/// 
/// 依赖注入演进方向：
///   当前用单例获取各服务（AudioMgr/ConfigService/…）；
///   后续若引入 DI 容器（如 Zenject），只需把构造函数改为注入，无需修改调用方。
/// </summary>
public class GameFlowController : BaseMgrMono<GameFlowController>
{
    // SceneStateController 由 GameFlowController 独占
    private SceneState.SceneStateController _stateCtrl;

    public override void Awake()
    {
        base.Awake();
        _stateCtrl = new SceneState.SceneStateController();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        _stateCtrl?.StateUpdate();
    }

    // ── 流程入口 ─────────────────────────────────────────

    public void GoToMainMenu()
        => _stateCtrl.SetState(new SceneState.StartScene(_stateCtrl));

    public void GoToLevelSelect()
        => _stateCtrl.SetState(new SceneState.SelectSecene(_stateCtrl));

    public void GoToGamePlay()
        => _stateCtrl.SetState(new SceneState.GameScene(_stateCtrl));

    public void GoToShop()
        => _stateCtrl.SetState(new SceneState.ShopScene(_stateCtrl));

    // ── 初始化入口（在 GameManager.Awake 后调用一次）────────
    public void StartGame()
    {
        // 如果当前已在主菜单场景则不重新加载，直接 Start
        string current = SceneManager.GetActiveScene().name;
        if (current == "01-MainMenu")
            _stateCtrl.SetState(new SceneState.StartScene(_stateCtrl), false);
        else
            GoToMainMenu();
    }
}
