namespace SceneState
{
    /// <summary>
    /// 商店场景状态（04-Shop）
    /// 商店 UI 逻辑由 ShopPanel 自行处理，此处只做音乐切换。
    /// </summary>
    public class ShopScene : ISceneState
    {
        public ShopScene(SceneStateController controller) : base("04-Shop", controller) { }

        public override void StateStart()
        {
            // 商店场景播放轻松的商店背景音乐
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, "ShopBGM");
        }

        public override void StateEnd()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }

        public override void StateUpdate() { }
    }
}
