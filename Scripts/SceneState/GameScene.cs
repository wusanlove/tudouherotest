namespace SceneState
{
    /// <summary>
    /// 游戏场景状态：每次进入新波次时重置波次相关状态。
    /// LevelControl 在 Awake/Start 中从 ConfigService 读取关卡数据并生成敌人。
    /// </summary>
    public class GameScene : ISceneState
    {
        public GameScene(SceneStateController controller) : base(GameFlowController.Scene_GamePlay, controller) { }

        public override void StateStart()
        {
            // 战斗 BGM
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm,
                new AudioBgmRequest { key = "GameBgm", loop = true, volume = 0.6f });
        }

        public override void StateEnd()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }
    }
}