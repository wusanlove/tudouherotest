using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 配置服务 — 统一从 Resources/Data/ 读取 JSON，首次访问时加载并缓存。
/// 取代原来散落在 GameManager / RoleSelectPanel / WeaponSelectPanel / DifficultySelectPanel /
/// LevelControl 等各处的重复 Resources.Load + JsonConvert.Deserialize 调用。
///
/// 用法示例：
///   ConfigService.Instance.Roles          → List＜RoleData＞
///   ConfigService.Instance.GetLevelData("danger1") → List＜LevelData＞
///
/// 未来演进思路（DI）：将 ConfigService 注册到 ServiceLocator/Zenject，
/// 注入需要配置的类，便于单元测试时注入 Mock 数据。
/// </summary>
public class ConfigService : BaseMgr<ConfigService>
{
    private ConfigService() { }

    // ── 缓存字段 ──────────────────────────────────────────────────
    private List<RoleData>       _roles;
    private List<WeaponData>     _weapons;
    private List<EnemyData>      _enemies;
    private List<PropData>       _props;
    private List<DifficultyData> _difficulties;

    private readonly Dictionary<string, List<LevelData>> _levelCache =
        new Dictionary<string, List<LevelData>>();

    // ── 公开属性（懒加载 + 缓存） ────────────────────────────────

    public List<RoleData> Roles
    {
        get { return _roles ?? (_roles = Load<List<RoleData>>("Data/role")); }
    }

    public List<WeaponData> Weapons
    {
        get { return _weapons ?? (_weapons = Load<List<WeaponData>>("Data/weapon")); }
    }

    public List<EnemyData> Enemies
    {
        get { return _enemies ?? (_enemies = Load<List<EnemyData>>("Data/enemy")); }
    }

    public List<PropData> Props
    {
        get { return _props ?? (_props = Load<List<PropData>>("Data/prop")); }
    }

    public List<DifficultyData> Difficulties
    {
        get { return _difficulties ?? (_difficulties = Load<List<DifficultyData>>("Data/difficulty")); }
    }

    /// <summary>按难度 levelName 读取关卡波次数据（带缓存）。</summary>
    public List<LevelData> GetLevelData(string levelName)
    {
        if (!_levelCache.TryGetValue(levelName, out List<LevelData> data))
        {
            data = Load<List<LevelData>>("Data/" + levelName);
            _levelCache[levelName] = data;
        }
        return data;
    }

    // ── 内部工具 ─────────────────────────────────────────────────

    private static T Load<T>(string path)
    {
        TextAsset text = Resources.Load<TextAsset>(path);
        if (text == null)
        {
            Debug.LogError($"[ConfigService] 找不到配置文件：Resources/{path}");
            return default;
        }
        return JsonConvert.DeserializeObject<T>(text.text);
    }
}
