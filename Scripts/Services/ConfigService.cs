using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 配置服务：统一从 Resources/Data 加载 JSON 数据。
/// 内部做一次性缓存，避免各面板重复读文件。
/// 后续可替换为 Addressables 异步加载，只需修改此类。
/// </summary>
public class ConfigService : BaseMgr<ConfigService>
{
    private ConfigService() { }

    // ── 缓存 ──────────────────────────────────────────────
    private List<RoleData>       _roles;
    private List<WeaponData>     _weapons;
    private List<PropData>       _props;
    private List<EnemyData>      _enemies;
    private List<DifficultyData> _difficulties;
    // 关卡数据按文件名缓存
    private readonly Dictionary<string, List<LevelData>> _levelCache
        = new Dictionary<string, List<LevelData>>();

    // ── 公共访问器 ────────────────────────────────────────
    public List<RoleData>       Roles        => _roles       ??= Load<List<RoleData>>("Data/role");
    public List<WeaponData>     Weapons      => _weapons     ??= Load<List<WeaponData>>("Data/weapon");
    public List<PropData>       Props        => _props       ??= Load<List<PropData>>("Data/prop");
    public List<EnemyData>      Enemies      => _enemies     ??= Load<List<EnemyData>>("Data/enemy");
    public List<DifficultyData> Difficulties => _difficulties ??= Load<List<DifficultyData>>("Data/difficulty");

    /// <summary>按难度对应的 levelName 读取关卡波次数据（如 level0, level1…）</summary>
    public List<LevelData> GetLevelDatas(string levelName)
    {
        if (_levelCache.TryGetValue(levelName, out var cached)) return cached;
        var data = Load<List<LevelData>>($"Data/{levelName}");
        _levelCache[levelName] = data;
        return data;
    }

    // ── 私有加载 ─────────────────────────────────────────
    private T Load<T>(string path)
    {
        TextAsset ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogError($"[ConfigService] 找不到配置文件: {path}");
            return default;
        }
        return JsonConvert.DeserializeObject<T>(ta.text);
    }

    /// <summary>切场景后清除缓存（一般不需要，除非热更新数据）</summary>
    public void ClearCache()
    {
        _roles = null; _weapons = null; _props = null;
        _enemies = null; _difficulties = null;
        _levelCache.Clear();
    }
}
