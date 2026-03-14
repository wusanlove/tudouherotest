namespace SceneState
{
    /// <summary>
    /// 商店场景状态。
    /// 进入：ShopPanel.Start 会自行读取 GameManager 数据并刷新 UI。
    /// </summary>
    public class ShopScene : ISceneState
    {
        public ShopScene(SceneStateController controller) : base("04-Shop", controller) { }

        public override void StateStart() { }

        public override void StateEnd() { }
    }
}