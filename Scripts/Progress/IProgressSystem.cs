/// <summary>
/// 进度/成就/解锁系统接口。
///
/// 设计思路：
/// - 当前实现：<see cref="PlayerPrefsProgressService"/>（PlayerPrefs 本地存储）。
/// - 后续升级路径：替换为 JSON 存档、云存档，或连接成就平台（Steam/GameCenter），
///   只需新建实现类并在 ProgressService 中切换，其余代码零改动。
///
/// 如需添加成就系统，在此接口中加入：
///   void CheckAchievements(AchievementContext ctx);
///   bool IsAchievementUnlocked(string achievementId);
/// </summary>
public interface IProgressSystem
{
    /// <summary>从持久化存储中载入进度。</summary>
    void Load();

    /// <summary>将进度持久化到存储。</summary>
    void Save();

    /// <summary>查询角色是否已解锁。</summary>
    bool IsRoleUnlocked(string roleName);

    /// <summary>解锁指定角色并同步到运行时 roleDatas。</summary>
    void UnlockRole(string roleName);

    /// <summary>
    /// 游戏完成时调用，触发解锁判断。
    /// </summary>
    /// <param name="roleName">使用的角色名称。</param>
    /// <param name="difficultyName">通关难度名称。</param>
    /// <param name="wavesCleared">完成波次数。</param>
    void OnGameCompleted(string roleName, string difficultyName, int wavesCleared);
}
