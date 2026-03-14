namespace SceneState
{
    /// <summary>
    /// 选择场景状态（02-LevelSelect）
    /// 职责：确保选择流程（角色→武器→难度）正确启动。
    /// 具体面板显示由各 Panel 的 Start() 自行处理；
    /// 此处只做"全局层面"的进场 / 离场清理。
    /// </summary>
    public class SelectSecene : ISceneState
    {
        public SelectSecene(SceneStateController controller) : base("02-LevelSelect", controller) { }

        public override void StateStart()
        {
            // 选择场景进入时播放背景音乐（可与主菜单相同或不同曲目）
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, "MenuBGM");
        }

        public override void StateEnd()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }

        public override void StateUpdate() { }
    }
}
