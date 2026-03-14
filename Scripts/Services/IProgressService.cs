/// <summary>
/// 进度与存档服务接口 — 目前只定义接口，使用 PlayerPrefs 最小实现。
///
/// 未来演进思路：
///   实现 JsonProgressService（本地 JSON 文件）或 CloudProgressService（云存档），
///   通过 ServiceLocator 注册替换实现，调用方无需修改任何代码。
/// </summary>
public interface IProgressService
{
    /// <summary>角色是否已解锁（解锁状态由玩法条件触发写入）。</summary>
    bool IsRoleUnlocked(string roleName);

    /// <summary>设置角色解锁状态。</summary>
    void SetRoleUnlocked(string roleName, bool unlocked);

    /// <summary>获取角色最佳通关难度等级（-1 = 尚无记录）。</summary>
    int GetRoleRecord(string roleName);

    /// <summary>更新角色通关记录（仅在新记录 > 旧记录时覆盖）。</summary>
    void UpdateRoleRecord(string roleName, int difficultyLevel);
}

/// <summary>
/// 基于 PlayerPrefs 的最小可用实现。
/// 进入 GamePlay 前由 Bootstrap 注册到 ServiceLocator（当前直接在 GameManager 里使用）。
/// </summary>
public class PlayerPrefsProgressService : IProgressService
{
    private const string UnlockSuffix = "_unlocked";
    private const string RecordSuffix = "_record";

    public bool IsRoleUnlocked(string roleName)
        => UnityEngine.PlayerPrefs.GetInt(roleName + UnlockSuffix, 0) == 1;

    public void SetRoleUnlocked(string roleName, bool unlocked)
        => UnityEngine.PlayerPrefs.SetInt(roleName + UnlockSuffix, unlocked ? 1 : 0);

    public int GetRoleRecord(string roleName)
        => UnityEngine.PlayerPrefs.GetInt(roleName + RecordSuffix, -1);

    public void UpdateRoleRecord(string roleName, int difficultyLevel)
    {
        int current = GetRoleRecord(roleName);
        if (difficultyLevel > current)
            UnityEngine.PlayerPrefs.SetInt(roleName + RecordSuffix, difficultyLevel);
    }
}
