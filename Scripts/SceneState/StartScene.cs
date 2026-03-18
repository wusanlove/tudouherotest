namespace SceneState
{
    /// <summary>
    /// 01-MainMenu 状态：显示主菜单 UI，播放菜单 BGM。
    /// </summary>
    public class StartScene : ISceneState
    {
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }

        public override void StateStart()
        {
            // 清理上一局遗留的 UI 缓存（防止跨场景空引用）
            UIMgr.Instance.DestroyDic();

            // 显示主菜单面板
            UIMgr.Instance.ShowPanel<BeginScenePanel>();

            // 通知音效系统播放菜单 BGM（AudioId 枚举驱动，无需关心音频名称）
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, AudioId.BGM_Menu);
        }

        public override void StateEnd()
        {
            UIMgr.Instance.HidePanel<BeginScenePanel>();
        }
    }
}