namespace SceneState
{
    /// <summary>
    /// 选择界面状态：角色 → 武器 → 难度三步选择，由 SelectSceneMediator 协调面板。
    /// </summary>
    public class SelectSecene : ISceneState
    {
        public SelectSecene(SceneStateController controller) : base("02-LevelSelect", controller) { }

        public override void StateStart()
        {
            // 选择界面 BGM
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm,
                new AudioBgmRequest { key = "SelectBgm", loop = true, volume = 0.7f });

            // 通知 SelectSceneMediator 打开第一步（角色选择）
            // Mediator 在场景加载后自行监听此事件
            EventCenter.Instance.EventTrigger(E_EventType.Select_OpenRole);
        }

        public override void StateEnd()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }
    }
}