using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 配置服务 – 统一封装所有 JSON / ScriptableObject 资源加载。
/// 避免 Resources.Load 分散在各处；提供简单缓存。
/// </summary>
public class ConfigService : BaseMgr<ConfigService>
{
    private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

    private ConfigService() { }

    // ──────────────── 泛型 JSON 加载 ────────────────

    /// <summary>从 Resources/Data/{fileName} 加载并反序列化 JSON，带缓存。</summary>
    public List<T> LoadList<T>(string fileName)
    {
        string key = "list_" + fileName;
        if (_cache.TryGetValue(key, out object cached))
            return (List<T>)cached;

        TextAsset ta = ResMgr.Instance.Load<TextAsset>("Data/" + fileName);
        if (ta == null)
        {
            Debug.LogWarning($"[ConfigService] Data/{fileName} not found.");
            return new List<T>();
        }

        List<T> result = JsonConvert.DeserializeObject<List<T>>(ta.text);
        _cache[key] = result;
        return result;
    }

    // ──────────────── 快捷方法 ────────────────

    public List<RoleData>       LoadRoles()      => LoadList<RoleData>("role");
    public List<WeaponData>     LoadWeapons()    => LoadList<WeaponData>("weapon");
    public List<PropData>       LoadProps()      => LoadList<PropData>("prop");
    public List<EnemyData>      LoadEnemies()    => LoadList<EnemyData>("enemy");
    public List<DifficultyData> LoadDifficulties() => LoadList<DifficultyData>("difficulty");
    public List<LevelData>      LoadLevel(string levelName) => LoadList<LevelData>(levelName);

    /// <summary>清空缓存（换难度/重新加载时调用）。</summary>
    public void ClearCache() => _cache.Clear();
}
