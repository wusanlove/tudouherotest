namespace SceneState
{
    /// <summary>
    /// 03-GamePlay 状态：战斗场景。
    /// </summary>
    public class GameScene : ISceneState
    {
        public GameScene(SceneStateController controller) : base("03-GamePlay", controller) { }

        public override void StateStart()
        {
            UIMgr.Instance.DestroyDic();
            // LevelControl 和 Player 的 Awake/Start 负责具体初始化，此处只清理上下文
        }

        public override void StateEnd()
        {
            // 波次结束后清理对象池（子弹/特效等），防止泄漏到商店场景
            PoolMgr.Instance.ClearPool();
            UIMgr.Instance.DestroyDic();
        }
    }
}