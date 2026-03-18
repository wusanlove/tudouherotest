namespace SceneState
{
    /// <summary>
    /// 02-LevelSelect 状态：角色 / 武器 / 难度选择场景。
    /// </summary>
    public class SelectSecene : ISceneState
    {
        public SelectSecene(SceneStateController controller) : base("02-LevelSelect", controller) { }

        public override void StateStart()
        {
            UIMgr.Instance.DestroyDic();
        }

        public override void StateEnd()
        {
            // 选择完成，配置已写入 GameManager；清理本场景 UI 引用
            UIMgr.Instance.DestroyDic();
        }
    }
}