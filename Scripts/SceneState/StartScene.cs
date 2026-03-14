namespace SceneState
{
    public class StartScene:ISceneState
    {
        
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }
        public override void StateStart()
        {
            //进入主菜单：通知音效系统播放菜单 BGM，清空 UI 栈
            UIFlowController.Instance.ClearAll();
            AudioService.Instance.PlayMenuBgm();
        }

        public override void StateUpdate()
        {
           
            
        }

        public override  void StateEnd()
        {
            //离开主菜单：停止 BGM
            AudioService.Instance.StopBgm();
        }
    }
}