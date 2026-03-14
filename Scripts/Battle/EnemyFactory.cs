using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人工厂（Factory + Pool 统一入口）。
///
/// 职责：
/// - 接受预制体注册（由场景中的 LevelControl 在 Awake 中注册各类型敌人 Prefab）。
/// - 通过 PoolMgr 复用敌人 GameObject，减少 GC 压力。
/// - 为生成的敌人附加 EnemyData（深拷贝，防止运行时修改污染原始数据）。
/// - 提供 Recycle() 统一回收入口，替换场景中散落的 Destroy() 调用。
///
/// 扩展说明：
/// - 当需要支持 Addressables 时，替换 _prefabDic[key] 来源即可，接口不变。
/// - 精英/Boss 生成可增加 SpawnElite()/SpawnBoss() 重载或传入 SpawnOptions 结构体。
/// </summary>
public class EnemyFactory : BaseMgr<EnemyFactory>
{
    // 由 LevelControl.Awake() 注册：enemyName → Prefab GameObject
    private readonly Dictionary<string, GameObject> _prefabDic =
        new Dictionary<string, GameObject>();

    private EnemyFactory() { }

    // ── 注册 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// 注册敌人预制体。应在场景初始化（LevelControl.Awake）时调用。
    /// 可重复注册同一 key，后注册的覆盖前者。
    /// </summary>
    public void RegisterPrefab(string enemyName, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[EnemyFactory] RegisterPrefab: prefab is null for key '{enemyName}'");
            return;
        }
        _prefabDic[enemyName] = prefab;
    }

    // ── 生成 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// 在指定位置生成一个敌人，附加正确的 EnemyData，并可选是否设为精英。
    /// </summary>
    /// <param name="enemyName">敌人类型名（与 JSON enemyName 字段一致，如 "enemy1"）。</param>
    /// <param name="position">世界坐标。</param>
    /// <param name="parent">可选父节点（用于场景层级管理）。</param>
    /// <param name="elite">是否为精英（血量 × 2，提供额外经验）。</param>
    /// <returns>生成成功返回 EnemyBase，否则返回 null。</returns>
    public EnemyBase Spawn(string enemyName, Vector3 position,
                           Transform parent = null, bool elite = false)
    {
        if (!_prefabDic.TryGetValue(enemyName, out GameObject prefab))
        {
            Debug.LogWarning($"[EnemyFactory] Unknown enemy type: '{enemyName}'. " +
                             "Call RegisterPrefab() first.");
            return null;
        }

        // PoolMgr.GetObj requires a Resources-path key and manages its own instantiation.
        // Since prefabs here are Inspector references (not Resources paths), we use Instantiate
        // directly. To integrate with PoolMgr, move prefabs to a Resources subfolder and
        // replace Instantiate with PoolMgr.Instance.GetObj(enemyName).
        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        go.name = enemyName; // PoolMgr 以 name 为 key 回收
        if (parent != null) go.transform.SetParent(parent);

        EnemyBase enemy = go.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogError($"[EnemyFactory] Prefab '{enemyName}' has no EnemyBase component.");
            Object.Destroy(go);
            return null;
        }

        // 为敌人附加深拷贝的 EnemyData（避免运行时修改共享原始数据）
        var gm = GameManager.Instance;
        if (gm?.enemyDatas != null)
        {
            foreach (EnemyData data in gm.enemyDatas)
            {
                if (data.name == enemyName)
                {
                    enemy.enemyData = data.Clone();
                    break;
                }
            }
        }

        if (elite) enemy.SetElite();

        return enemy;
    }

    // ── 回收 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// 将敌人对象回收至对象池（替代 Destroy 调用）。
    /// </summary>
    public void Recycle(EnemyBase enemy)
    {
        if (enemy == null) return;
        // 让 PoolMgr 负责失活与父对象管理
        PoolMgr.Instance.PushObj(enemy.gameObject);
    }

    // ── 场景切换清理 ─────────────────────────────────────────────────────────

    /// <summary>场景卸载时清空注册列表，防止跨场景 Prefab 引用失效。</summary>
    public void ClearRegistrations()
    {
        _prefabDic.Clear();
    }
}
