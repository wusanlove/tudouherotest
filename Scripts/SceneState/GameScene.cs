namespace SceneState
{
    /// <summary>
    /// 游戏场景状态。
    /// 进入：确保 isDead 复位（Player.Awake 会在 waveCount==1 时调用 InitProp）。
    /// </summary>
    public class GameScene : ISceneState
    {
        public GameScene(SceneStateController controller) : base("03-GamePlay", controller) { }

        public override void StateStart()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.isDead = false;
        }

        public override void StateEnd() { }
    }
}