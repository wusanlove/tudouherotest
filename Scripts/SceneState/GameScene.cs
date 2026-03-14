namespace SceneState
{
    public class GameScene:ISceneState
    {
        public GameScene(SceneStateController controller) : base("03-GamePlay", controller) { }
        public override void StateStart()
        {
            //进入游戏场景：重置 isDead 标记，UI 栈清空
            UIFlowController.Instance.ClearAll();
            if (GameManager.Instance != null)
                GameManager.Instance.isDead = false;
        }

        public override void StateUpdate()
        {
            
        }

        public override  void StateEnd()
        {
            
        }
    }
}