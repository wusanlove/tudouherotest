namespace SceneState
{
    /// <summary>
    /// 04-Shop 状态：商店场景，玩家在此购买道具/武器后进入下一波。
    /// </summary>
    public class ShopScene : ISceneState
    {
        public ShopScene(SceneStateController controller) : base("04-Shop", controller) { }

        public override void StateStart()
        {
            UIMgr.Instance.DestroyDic();
        }

        public override void StateEnd()
        {
            UIMgr.Instance.DestroyDic();
        }
    }
}