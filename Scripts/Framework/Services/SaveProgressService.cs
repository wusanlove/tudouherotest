using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档/进度服务 – 统一封装 PlayerPrefs 读写，负责角色解锁、通关记录等持久化数据。
/// 避免各处直接调用 PlayerPrefs，方便未来替换为云存档或 JSON 文件。
/// </summary>
public class SaveProgressService : BaseMgr<SaveProgressService>
{
    // ────── 已知角色名列表（用于初始化默认值）──────
    private static readonly string[] KnownRoleNames = { "多面手", "公牛" };

    private SaveProgressService() { }

    // ──────────────── 角色解锁 ────────────────

    /// <summary>初始化所有角色的默认解锁状态（若 PlayerPrefs 中无记录则设为 0）。</summary>
    public void InitRoleUnlockDefaults()
    {
        foreach (string name in KnownRoleNames)
        {
            if (!PlayerPrefs.HasKey(name))
                PlayerPrefs.SetInt(name, 0);
        }
    }

    /// <summary>获取某角色的解锁状态。0=未解锁，1=已解锁。</summary>
    public int GetRoleUnlock(string roleName) => PlayerPrefs.GetInt(roleName, 0);

    /// <summary>设置某角色为已解锁。</summary>
    public void UnlockRole(string roleName)
    {
        if (GetRoleUnlock(roleName) != 0) return;
        PlayerPrefs.SetInt(roleName, 1);
        PlayerPrefs.Save();
        Debug.Log($"[SaveProgressService] 角色解锁: {roleName}");
    }

    /// <summary>检查并按条件解锁角色（若满足条件且尚未解锁则解锁，并同步到 roleDatas 列表）。</summary>
    public void TryUnlockRole(string roleName, bool conditionMet, List<RoleData> roleDatas)
    {
        if (!conditionMet || GetRoleUnlock(roleName) != 0) return;
        UnlockRole(roleName);
        foreach (RoleData rd in roleDatas)
        {
            if (rd.name == roleName)
                rd.unlock = 1;
        }
    }

    // ──────────────── 通关记录 ────────────────

    private const string RecordKeyPrefix = "record_";

    /// <summary>获取某角色的通关记录（难度等级，-1 = 无记录）。</summary>
    public int GetRecord(string roleName) => PlayerPrefs.GetInt(RecordKeyPrefix + roleName, -1);

    /// <summary>保存通关记录（仅当新记录优于旧记录时更新）。</summary>
    public void SaveRecord(string roleName, int difficultyLevel)
    {
        int old = GetRecord(roleName);
        if (difficultyLevel > old)
        {
            PlayerPrefs.SetInt(RecordKeyPrefix + roleName, difficultyLevel);
            PlayerPrefs.Save();
        }
    }
}
