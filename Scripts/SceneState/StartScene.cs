namespace SceneState
{
    /// <summary>
    /// 主菜单状态：通过 UIMgr 加载 BeginScenePanel，并通知 AudioMgr 播放 BGM。
    /// 资源加载当前走 Resources；后续换 Addressables 只需改 UIMgr.ShowPanel 内部实现。
    /// </summary>
    public class StartScene : ISceneState
    {
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }

        public override void StateStart()
        {
            // 加载主菜单 Panel（UIMgr 负责实例化并缓存）
            UIMgr.Instance.ShowPanel<BeginScenePanel>();

            // 通知音频系统播放主菜单 BGM
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm,
                new AudioBgmRequest { key = "MainMenuBgm", loop = true, volume = 0.8f });
        }

        public override void StateEnd()
        {
            // 离开主菜单时停止 BGM、隐藏面板
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
            UIMgr.Instance.HidePanel<BeginScenePanel>();
        }
    }
}