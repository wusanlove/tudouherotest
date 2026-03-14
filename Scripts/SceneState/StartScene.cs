namespace SceneState
{
    public class StartScene:ISceneState
    {
        
        public StartScene(SceneStateController controller) : base("01-MainMenu", controller) { }
        public override void StateStart()
        {
            //进入游戏界面实例化开始游戏面板（用资源管理系统），通知音效系统播放背景音乐
            
        }

        public override void StateUpdate()
        {
           
            
        }

        public override  void StateEnd()
        {
            //跳转到选择界面，销毁开始游戏面板。（资源管理系统负责直接销毁）
            
        }
    }
}