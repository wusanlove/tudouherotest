namespace SceneState
{
    /// <summary>
    /// 关卡选择场景状态。
    /// 进入：重置本局选择数据（防止上一局残留脏数据）。
    /// RoleSelectPanel / WeaponSelectPanel / DifficultySelectPanel 在场景加载后自行初始化。
    /// </summary>
    public class SelectSecene : ISceneState
    {
        public SelectSecene(SceneStateController controller) : base("02-LevelSelect", controller) { }

        public override void StateStart()
        {
            // 重置选择数据，保证每次进入都是干净状态
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentRoleData   = null;
                GameManager.Instance.currentDifficulty = null;
                GameManager.Instance.currentWeapons.Clear();
            }
        }

        public override void StateEnd() { }
    }
}