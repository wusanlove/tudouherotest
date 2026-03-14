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
    // ── 场景名称常量 ──────────────────────────────────────
    // 修改场景名时只需改这里，所有跳转自动同步
    public const string Scene_MainMenu   = "01-MainMenu";
    public const string Scene_LevelSelect = "02-LevelSelect";
    public const string Scene_GamePlay   = "03-GamePlay";
    public const string Scene_Shop       = "04-Shop";

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
        string current = SceneManager.GetActiveScene().name;
        if (current == Scene_MainMenu)
            _stateCtrl.SetState(new SceneState.StartScene(_stateCtrl), false);
        else
            GoToMainMenu();
    }
}
