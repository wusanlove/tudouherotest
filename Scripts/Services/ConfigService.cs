using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 集中化 JSON 配置加载服务（Single Source of Truth）。
/// 所有 JSON → C# 数据解析均在此完成；其他模块不应直接调用 Resources.Load 加载 JSON。
/// 
/// 扩展说明：
/// - 目前从 Resources/Data/ 加载；如需 Addressables，只需替换 LoadRaw() 实现即可。
/// - 如需缓存，可在此添加 Dictionary 缓存层（目前每次解析以保证数据纯净）。
/// </summary>
public class ConfigService : BaseMgr<ConfigService>
{
    private ConfigService() { }

    // ── 基础加载 ─────────────────────────────────────────────────────────────

    private T LoadList<T>(string resourcePath)
    {
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null)
        {
            Debug.LogWarning($"[ConfigService] JSON not found at Resources/{resourcePath}");
            return default;
        }
        return JsonConvert.DeserializeObject<T>(ta.text);
    }

    // ── 公开接口 ─────────────────────────────────────────────────────────────

    /// <summary>加载所有角色配置（Data/role.json）。</summary>
    public List<RoleData> LoadRoles()
        => LoadList<List<RoleData>>("Data/role");

    /// <summary>加载所有武器配置（Data/weapon.json）。</summary>
    public List<WeaponData> LoadWeapons()
        => LoadList<List<WeaponData>>("Data/weapon");

    /// <summary>加载所有敌人配置（Data/enemy.json）。</summary>
    public List<EnemyData> LoadEnemies()
        => LoadList<List<EnemyData>>("Data/enemy");

    /// <summary>加载所有道具配置（Data/prop.json）。</summary>
    public List<PropData> LoadProps()
        => LoadList<List<PropData>>("Data/prop");

    /// <summary>加载所有难度配置（Data/difficulty.json）。</summary>
    public List<DifficultyData> LoadDifficulties()
        => LoadList<List<DifficultyData>>("Data/difficulty");

    /// <summary>
    /// 加载指定关卡的波次配置。
    /// levelName 对应 DifficultyData.levelName（如 "level0"、"level1"…）。
    /// </summary>
    public List<LevelData> LoadLevel(string levelName)
        => LoadList<List<LevelData>>($"Data/{levelName}");
}
