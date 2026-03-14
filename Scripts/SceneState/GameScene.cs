namespace SceneState
{
    /// <summary>
    /// 游戏场景状态（03-GamePlay）
    /// Player.Awake() 负责在第一波调用 GameManager.InitProp()，
    /// 此处只做状态层面的进入 / 离开处理。
    /// </summary>
    public class GameScene : ISceneState
    {
        public GameScene(SceneStateController controller) : base("03-GamePlay", controller) { }

        public override void StateStart()
        {
            // 游戏场景进入后播放战斗背景音乐
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, "BattleBGM");
        }

        public override void StateEnd()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }

        public override void StateUpdate() { }
    }
}
