namespace SceneState
{
    /// <summary>
    /// 主菜单状态（01-MainMenu）
    /// 职责：播放背景音乐、监听"前往选择场景"的事件。
    /// </summary>
    public class StartScene : ISceneState
    {
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }

        public override void StateStart()
        {
            // 通知音效系统播放菜单背景音乐（通过 EventCenter 保持解耦）
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, "MenuBGM");
        }

        public override void StateEnd()
        {
            // 离开主菜单时停止背景音乐
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }

        public override void StateUpdate() { }
    }
}
