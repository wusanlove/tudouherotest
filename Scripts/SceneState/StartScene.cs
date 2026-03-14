namespace SceneState
{
    /// <summary>
    /// 主菜单状态。
    /// 进入：通知 AudioMgr 播放 BGM。
    /// 退出：停止 BGM（可选，按游戏风格决定）。
    /// </summary>
    public class StartScene : ISceneState
    {
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }

        public override void StateStart()
        {
            // 通过 EventCenter 触发播放 BGM，AudioMgr 监听并处理（解耦）
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm);
        }

        public override void StateEnd()
        {
            // 切到选关场景前停止背景音乐
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }
    }
}