namespace SceneState
{
    /// <summary>
    /// 游戏场景状态（03-GamePlay 场景）。
    /// 进入时：通知 Battle 子系统当前波次已开始。
    /// 离开时：广播 Battle_WaveCompleted 供其他系统做清理（如停止生成 Coroutine）。
    /// </summary>
    public class GameScene : ISceneState
    {
        public GameScene(SceneStateController controller) : base("03-GamePlay", controller) { }

        public override void StateStart()
        {
            // 通知所有监听者当前波次已开始（body = currentWave 索引）
            EventCenter.Instance.EventTrigger<int>(
                E_EventType.Battle_WaveStarted,
                GameManager.Instance.currentWave
            );
        }

        public override void StateEnd()
        {
            // 通知波次结束（供 LevelControl、AudioMgr 等清理资源）
            EventCenter.Instance.EventTrigger<int>(
                E_EventType.Battle_WaveCompleted,
                GameManager.Instance.currentWave
            );
        }

        public override void StateUpdate() { }
    }
}