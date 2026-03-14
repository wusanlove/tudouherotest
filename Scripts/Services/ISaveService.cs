/// <summary>
/// 存档/进度服务接口。
/// 当前提供基本签名与注释；具体持久化逻辑（PlayerPrefs / JSON / 云存档）可在
/// SaveService 中补齐，业务层只依赖此接口，方便后续替换。
/// </summary>
public interface ISaveService
{
    // ── 角色解锁 ──────────────────────────────────────────
    /// <summary>检查角色是否已解锁</summary>
    bool IsRoleUnlocked(string roleName);
    /// <summary>解锁角色</summary>
    void UnlockRole(string roleName);

    // ── 通关记录 ──────────────────────────────────────────
    /// <summary>获取某角色的最高通关难度（-1 表示无记录）</summary>
    int GetBestRecord(string roleName);
    /// <summary>更新通关记录（取最大值）</summary>
    void UpdateRecord(string roleName, int difficultyId);

    // ── 持久化 ────────────────────────────────────────────
    void Save();
    void Load();
}
