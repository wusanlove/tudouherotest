using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人工厂：封装敌人预制体的注册和实例化（当前 Instantiate；后续接入 PoolMgr 对象池）。
/// 
/// 使用：
///   1. 在 LevelControl.Awake() 中调用 Register() 注册各预制体。
///   2. 调用 Spawn() 生成实例；后续改对象池只需修改 Spawn() 内部。
/// </summary>
public class EnemyFactory : BaseMgr<EnemyFactory>
{
    private readonly Dictionary<string, GameObject> _prefabs
        = new Dictionary<string, GameObject>();

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
            Debug.LogError($"[EnemyFactory] 未注册的敌人: {enemyName}");
            return null;
        }

        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        if (parent != null) go.transform.SetParent(parent);

        EnemyBase enemy = go.GetComponent<EnemyBase>();

        // 从 ConfigService 匹配数据，避免 EnemyBase.Start() 重复遍历
        foreach (EnemyData data in ConfigService.Instance.Enemies)
        {
            if (data.name == enemyName)
            {
                enemy.Init(data);
                break;
            }
        }

        return enemy;
    }

    /// <summary>清除已注册的预制体（切场景时调用）</summary>
    public void Clear() => _prefabs.Clear();
}
