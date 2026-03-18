using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 配置数据统一加载入口 —— 所有 JSON 从这里取，消除各脚本散列 Resources.Load。
/// 每次调用都反序列化新实例，确保调用方拿到独立副本（可安全修改）。
/// TODO: 演进方向 → Addressables 异步加载 + ScriptableObject 热更替换。
/// </summary>
public class ConfigMgr : BaseMgr<ConfigMgr>
{
    private ConfigMgr() { }

    /// <summary>加载 Resources/Data/{key}.json 并反序列化为 List&lt;T&gt;。</summary>
    public List<T> LoadList<T>(string key)
    {
        string path = $"Data/{key}";
        TextAsset asset = Resources.Load<TextAsset>(path);
        if (asset == null)
        {
            Debug.LogError($"[ConfigMgr] 找不到配置: Resources/{path}");
            return new List<T>();
        }
        return JsonConvert.DeserializeObject<List<T>>(asset.text) ?? new List<T>();
    }

    /// <summary>加载 Resources/Data/{key}.json 并反序列化为单个对象 T。</summary>
    public T LoadSingle<T>(string key)
    {
        string path = $"Data/{key}";
        TextAsset asset = Resources.Load<TextAsset>(path);
        if (asset == null)
        {
            Debug.LogError($"[ConfigMgr] 找不到配置: Resources/{path}");
            return default;
        }
        return JsonConvert.DeserializeObject<T>(asset.text);
    }
}
