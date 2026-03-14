using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 外观模式（Facade Pattern）示例
/// 
/// 作用：为游戏的多个复杂子系统提供一个简洁的统一入口。
/// 调用方不需要知道内部如何协调 GameManager / AudioMgr / UIMgr，
/// 只需调用 GameFacade 的简单方法即可。
/// 
/// 关键特征：
///   - 子系统（GameManager, AudioMgr 等）不知道 GameFacade 的存在
///   - 通信是单向的：外部 → 外观 → 子系统
///   - 隐藏复杂性，简化调用接口
/// 
/// 区别于中介者模式：
///   - 外观：外部只和外观说话，外观内部协调子系统
///   - 中介者：多个对象互相不认识，都通过中介者沟通
/// </summary>
public static class GameFacade
{
    // ────────────────────────────────────────────────────────────────
    // 简洁接口1：开始一局游戏
    // 内部协调 GameManager 数据初始化 + 音效切换 + 场景跳转
    // ────────────────────────────────────────────────────────────────
    public static void StartGame(RoleData role, List<WeaponData> weapons, DifficultyData difficulty)
    {
        // 写入选择数据
        GameManager.Instance.currentRoleData = role;
        GameManager.Instance.currentWeapons = new List<WeaponData>(weapons);
        GameManager.Instance.currentDifficulty = difficulty;

        // 重置游戏状态
        GameManager.Instance.currentWave = 1;
        GameManager.Instance.waveCount = 1;
        GameManager.Instance.isDead = false;
        // PropData 使用字段默认值初始化，与 GameManager.InitProp() 的前提一致
        GameManager.Instance.propData = new PropData();

        // 初始化角色属性（以上步骤对调用方透明）
        GameManager.Instance.InitProp();

        // 切换音效：停止菜单音乐，播放游戏音乐
        EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm,
            new AudioBgmRequest { key = "bgm_game", volume = 0.8f, loop = true });

        // 跳转场景
        SceneManager.LoadScene("03-GamePlay");
    }

    // ────────────────────────────────────────────────────────────────
    // 简洁接口2：游戏结算（胜利或失败）
    // ────────────────────────────────────────────────────────────────
    public static void EndGame(bool success)
    {
        GameManager.Instance.isDead = !success;
        if (success)
            UIMgr.Instance.ShowPanel<SuccessPanel>();
        else
            UIMgr.Instance.ShowPanel<FailPanel>();
    }

    // ────────────────────────────────────────────────────────────────
    // 简洁接口3：返回主菜单并清理所有状态
    // ────────────────────────────────────────────────────────────────
    public static void ReturnToMainMenu()
    {
        // 清理各子系统（顺序对调用方透明）
        EventCenter.Instance.Clear();
        PoolMgr.Instance.ClearPool();
        UIMgr.Instance.DestroyDic();

        SceneManager.LoadScene("01-MainMenu");
    }
}

// ────────────────────────────────────────────────────────────────
// 使用示例（替代当前 BeginScenePanel 中的直接 SceneManager.LoadScene）
// ────────────────────────────────────────────────────────────────
// btnStart.onClick.AddListener(() =>
// {
//     GameFacade.StartGame(
//         GameManager.Instance.currentRoleData,
//         GameManager.Instance.currentWeapons,
//         GameManager.Instance.currentDifficulty
//     );
// });
