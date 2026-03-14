namespace SceneState
{
    /// <summary>
    /// 主菜单状态（01-MainMenu 场景）。
    /// 进入时：通过 UIMgr 显示 BeginScenePanel，并通过 EventCenter 播放背景音乐。
    /// 离开时：停止 BGM（可选）。
    /// </summary>
    public class StartScene : ISceneState
    {
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }

        public override void StateStart()
        {
            // 显示主菜单面板（资源由 UIMgr 从 Resources/Prefabs/UI/ 加载）
            UIMgr.Instance.ShowPanel<BeginScenePanel>();

            // 通过 EventCenter → AudioMgr 播放背景音乐
            EventCenter.Instance.EventTrigger<AudioBgmRequest>(
                E_EventType.Audio_PlayBgm,
                new AudioBgmRequest { key = "menu_bgm", loop = true, volume = 0f }
            );
        }

        public override void StateEnd()
        {
            // 离开主菜单时停止 BGM（进入游戏有自己的 BGM）
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }

        public override void StateUpdate() { }
    }
}