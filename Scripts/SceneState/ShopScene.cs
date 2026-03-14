namespace SceneState
{
    public class ShopScene:ISceneState
    {
        public ShopScene(SceneStateController controller) : base("04-Shop", controller) { }
        public override void StateStart()
        {
            //进入商店场景：waveCount 递增，UI 栈清空
            UIFlowController.Instance.ClearAll();
            if (GameManager.Instance != null)
                GameManager.Instance.waveCount++;
        }

        public override void StateUpdate()
        {
            
        }

        public override  void StateEnd()
        {
            
        }
    }
}