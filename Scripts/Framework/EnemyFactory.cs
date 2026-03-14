using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人工厂：封装敌人预制体的注册和实例化（当前 Instantiate；后续接入 PoolMgr 对象池）。
/// 内部维护 O(1) 字典，避免每次生成时遍历列表。
/// 
/// 使用：
///   1. 在 LevelControl.Awake() 中调用 Register() 注册各预制体。
///   2. 调用 Spawn() 生成实例；后续改对象池只需修改 Spawn() 内部。
/// </summary>
public class EnemyFactory : BaseMgr<EnemyFactory>
{
    private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

    // 缓存 EnemyData 字典，避免每次 Spawn 遍历列表
    private Dictionary<string, EnemyData> _dataCache;

    private EnemyFactory() { }

    /// <summary>注册敌人预制体（在 LevelControl.Awake 中调用）</summary>
    public void Register(string enemyName, GameObject prefab)
    {
        if (!_prefabs.ContainsKey(enemyName))
            _prefabs.Add(enemyName, prefab);
    }

    /// <summary>
    /// 在指定位置生成一个敌人并初始化数据。
    /// TODO: 将 Instantiate 替换为 PoolMgr.GetObj 即可接入对象池。
    /// </summary>
    public EnemyBase Spawn(string enemyName, Vector3 position, Transform parent = null)
    {
        if (!_prefabs.TryGetValue(enemyName, out GameObject prefab))
        {
            Debug.LogError($"[EnemyFactory] Unregistered enemy: {enemyName}");
            return null;
        }

        // 延迟初始化数据缓存（首次 Spawn 时建立）
        if (_dataCache == null)
            BuildDataCache();

        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        if (parent != null) go.transform.SetParent(parent);

        EnemyBase enemy = go.GetComponent<EnemyBase>();

        if (_dataCache.TryGetValue(enemyName, out EnemyData data))
            enemy.Init(data);
        else
            Debug.LogWarning($"[EnemyFactory] No EnemyData found for: {enemyName}");

        return enemy;
    }

    /// <summary>清除已注册的预制体和数据缓存（切场景时调用）</summary>
    public void Clear()
    {
        _prefabs.Clear();
        _dataCache = null;
    }

    private void BuildDataCache()
    {
        _dataCache = new Dictionary<string, EnemyData>();
        foreach (EnemyData d in ConfigService.Instance.Enemies)
        {
            if (!_dataCache.ContainsKey(d.name))
                _dataCache.Add(d.name, d);
        }
    }
}
