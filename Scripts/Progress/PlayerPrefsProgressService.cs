using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerPrefs 版进度服务（<see cref="IProgressSystem"/> 默认实现）。
///
/// 职责：
/// - 角色解锁状态的初始化、查询与持久化（key = 角色名，value = 0/1）。
/// - 游戏完成后的通关解锁判断（多面手：首次通关；公牛：通关时 maxHp >= 50）。
///
/// TODO（后续扩展）：
/// - 加入成就系统时，在 OnGameCompleted 中检测各类成就条件并调用 UnlockAchievement。
/// - 如需云存档，继承本类或新建类实现 IProgressSystem，在 ProgressService 中切换。
/// </summary>
public class PlayerPrefsProgressService : IProgressSystem
{
    // 默认上锁的角色（未达成条件前不可选择）
    private static readonly HashSet<string> DefaultLockedRoles =
        new HashSet<string> { "多面手", "公牛" };

    public void Load()
    {
        // 初始化 PlayerPrefs 中不存在的 key，防止首次运行时 GetInt 返回错误值
        foreach (string role in DefaultLockedRoles)
        {
            if (!PlayerPrefs.HasKey(role))
                PlayerPrefs.SetInt(role, 0);
        }
    }

    public bool IsRoleUnlocked(string roleName)
    {
        if (!DefaultLockedRoles.Contains(roleName)) return true;
        return PlayerPrefs.GetInt(roleName, 0) == 1;
    }

    public void UnlockRole(string roleName)
    {
        if (IsRoleUnlocked(roleName)) return; // 已解锁，无需重复写盘

        PlayerPrefs.SetInt(roleName, 1);
        PlayerPrefs.Save();

        // 同步运行时 roleDatas，使选择界面立即生效（无需重启）
        var roleDatas = GameManager.Instance?.roleDatas;
        if (roleDatas != null)
        {
            foreach (RoleData rd in roleDatas)
            {
                if (rd.name == roleName)
                {
                    rd.unlock = 1;
                    break;
                }
            }
        }

        Debug.Log($"[Progress] Role unlocked: {roleName}");
    }

    public void OnGameCompleted(string roleName, string difficultyName, int wavesCleared)
    {
        // 通关即解锁"多面手"
        if (!IsRoleUnlocked("多面手"))
            UnlockRole("多面手");

        // 公牛解锁条件：通关时 maxHp >= 50（由 Player 运行时判断，这里也做兜底检测）
        if (!IsRoleUnlocked("公牛") && GameManager.Instance != null
            && GameManager.Instance.propData.maxHp >= 50)
            UnlockRole("公牛");

        // TODO: 接入成就系统后，在此处逐条检测成就条件
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }
}
