namespace SceneState
{
    /// <summary>
    /// 商店场景状态（04-Shop 场景）。
    /// 进入时：通知商店 UI 刷新当前角色属性与可购买道具。
    /// 离开时：由 UIFlowController 监听 Shop_ProceedToNextWave 完成场景跳转。
    /// </summary>
    public class ShopScene : ISceneState
    {
        public ShopScene(SceneStateController controller) : base("04-Shop", controller) { }

        public override void StateStart()
        {
            // 商店场景初始化（ShopPanel 在自己的 Start 中读取 GameManager 数据）
            // 通知 HUD 刷新金币（商店结算后可能有变化）
            EventCenter.Instance.EventTrigger(E_EventType.HUD_MoneyChanged);
        }

        public override void StateEnd() { }

        public override void StateUpdate() { }
    }
}