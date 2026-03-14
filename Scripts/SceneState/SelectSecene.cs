namespace SceneState
{
    public class SelectSecene:ISceneState
    {
        public SelectSecene(SceneStateController controller) : base("02-LevelSelect", controller) { }
        public override void StateStart()
        {
            //进入选择场景：清空 UI 栈，重置 UIFlowController 状态
            UIFlowController.Instance.ClearAll();
        }

        public override void StateUpdate()
        {
            
        }

        public override  void StateEnd()
        {
            
        }
    }
}