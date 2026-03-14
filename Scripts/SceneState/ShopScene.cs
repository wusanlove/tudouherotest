namespace SceneState
{
    /// <summary>
    /// 商店场景状态：每波结束后进入；ShopPanel 由 ShopController 驱动。
    /// </summary>
    public class ShopScene : ISceneState
    {
        public ShopScene(SceneStateController controller) : base("04-Shop", controller) { }

        public override void StateStart()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm,
                new AudioBgmRequest { key = "ShopBgm", loop = true, volume = 0.6f });
        }

        public override void StateEnd()
        {
            EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        }
    }
}