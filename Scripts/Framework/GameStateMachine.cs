using SceneState;
using UnityEngine;

/// <summary>
/// 游戏状态机入口 – 非 MonoBehaviour 单例，内部创建隐藏 MonoBehaviour 来驱动 Update。
/// 所有场景切换统一由此发起，避免各 Manager/Panel 直接调用 SceneManager.LoadScene。
///
/// 使用方式：
///   GameStateMachine.Instance.EnterSelect();    // 跳转到选择场景
///   GameStateMachine.Instance.EnterGame();      // 跳转到游戏场景
///   GameStateMachine.Instance.EnterShop();      // 跳转到商店场景
///   GameStateMachine.Instance.EnterMainMenu();  // 返回主菜单
/// </summary>
public class GameStateMachine : BaseMgr<GameStateMachine>
{
    private readonly SceneStateController _controller = new SceneStateController();
    private GameStateMachineUpdater _updater;

    private GameStateMachine()
    {
        var go = new GameObject("[GameStateMachine]");
        Object.DontDestroyOnLoad(go);
        _updater = go.AddComponent<GameStateMachineUpdater>();
        _updater.owner = this;
    }

    internal void OnUpdate() => _controller.StateUpdate();

    // ──────────────── 状态切换 API ────────────────

    /// <summary>进入主菜单（默认加载场景 01-MainMenu）。</summary>
    public void EnterMainMenu(bool loadScene = true)
        => _controller.SetState(new StartScene(_controller), loadScene);

    /// <summary>进入角色/武器/难度选择场景（02-LevelSelect）。</summary>
    public void EnterSelect(bool loadScene = true)
        => _controller.SetState(new SelectSecene(_controller), loadScene);

    /// <summary>进入战斗游戏场景（03-GamePlay）。</summary>
    public void EnterGame(bool loadScene = true)
        => _controller.SetState(new GameScene(_controller), loadScene);

    /// <summary>进入商店场景（04-Shop）。</summary>
    public void EnterShop(bool loadScene = true)
        => _controller.SetState(new ShopScene(_controller), loadScene);
}

/// <summary>GameStateMachine 的内部 MonoBehaviour 代理，每帧驱动状态机 Update。</summary>
public class GameStateMachineUpdater : MonoBehaviour
{
    public GameStateMachine owner;
    private void Update() => owner?.OnUpdate();
}
