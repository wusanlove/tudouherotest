using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// 关卡控制器：负责波次计时、敌人生成调度、胜负判定。
///
/// 变更说明：
/// - 敌人生成改为通过 <see cref="EnemyFactory"/>，不再散落 Instantiate 调用。
/// - 胜负结果通过 <see cref="EventCenter"/> 广播（Battle_AllWavesCompleted / Battle_PlayerDied），
///   SuccessPanel/FailPanel 各自监听并显示，不再由此脚本直接操作 CanvasGroup。
/// - 角色解锁逻辑委托 <see cref="ProgressService"/>，职责分离。
/// - 关卡数据通过 <see cref="ConfigService"/> 加载，不再直接 Resources.Load JSON。
/// </summary>
public class LevelControl : BaseMgrMono<LevelControl>
{
    public float waveTimer;

    public List<EnemyBase> enemy_list = new List<EnemyBase>();
    public Transform _map;

    [Header("敌人预制体（Inspector 拖拽，由 EnemyFactory 统一管理）")]
    public GameObject enemy1_prefab;
    public GameObject enemy2_prefab;
    public GameObject enemy3_prefab;
    public GameObject enemy4_prefab;
    public GameObject enemy5_prefab;
    public Transform  enemyFather;

    public GameObject redfork_prefab;

    private List<LevelData> levelDatas    = new List<LevelData>();
    private LevelData       currentLevelData;

    public override void Awake()
    {
        base.Awake();

        // ── 通过 ConfigService 加载关卡 JSON ────────────────────────────────
        levelDatas = ConfigService.Instance.LoadLevel(GameManager.Instance.currentDifficulty.levelName);
        GameManager.Instance.maxWave = levelDatas.Count;

        // ── 向 EnemyFactory 注册本场景的敌人预制体 ──────────────────────────
        EnemyFactory.Instance.RegisterPrefab("enemy1", enemy1_prefab);
        EnemyFactory.Instance.RegisterPrefab("enemy2", enemy2_prefab);
        EnemyFactory.Instance.RegisterPrefab("enemy3", enemy3_prefab);
        EnemyFactory.Instance.RegisterPrefab("enemy4", enemy4_prefab);
        EnemyFactory.Instance.RegisterPrefab("enemy5", enemy5_prefab);
    }

    private void Start()
    {
        currentLevelData = levelDatas[GameManager.Instance.currentWave - 1];
        waveTimer        = currentLevelData.waveTimer;

        GenerateEnemy();
        GenerateWeapon();
    }

    private void GenerateWeapon()
    {
        int i = 0;
        foreach (WeaponData weaponData in GameManager.Instance.currentWeapons)
        {
            if (i >= Player.Instance.weaponsPos.childCount)
            {
                Debug.LogWarning($"[LevelControl] weaponsPos has only {Player.Instance.weaponsPos.childCount} slots; weapon '{weaponData.name}' at index {i} cannot be equipped.");
                break;
            }
            GameObject go = Resources.Load<GameObject>("Prefabs/武器/" + weaponData.name);
            WeaponBase wb = Object.Instantiate(go, Player.Instance.weaponsPos.GetChild(i).transform)
                                  .GetComponent<WeaponBase>();
            wb.data = weaponData;
            i++;
        }
    }

    private void GenerateEnemy()
    {
        foreach (WaveData waveData in currentLevelData.enemys)
        {
            for (int i = 0; i < waveData.count; i++)
                StartCoroutine(SpawnEnemyCoroutine(waveData));
        }
    }

    private IEnumerator SpawnEnemyCoroutine(WaveData waveData)
    {
        yield return new WaitForSeconds(waveData.timeAxis);
        if (waveTimer <= 0 || GameManager.Instance.isDead) yield break;

        var spawnPoint = GetRandomPosition(_map.GetComponent<SpriteRenderer>().bounds);

        // 红叉预警
        GameObject fork = Object.Instantiate(redfork_prefab, spawnPoint, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Object.Destroy(fork);

        if (waveTimer <= 0 || GameManager.Instance.isDead) yield break;

        // 通过 EnemyFactory 生成（Factory 内部赋值 EnemyData + 处理 Elite）
        EnemyBase enemy = EnemyFactory.Instance.Spawn(
            waveData.enemyName, spawnPoint, enemyFather, waveData.elite == 1);

        if (enemy != null)
            enemy_list.Add(enemy);
    }

    private Vector3 GetRandomPosition(Bounds bounds)
    {
        const float safeDistance = 3.5f;
        return new Vector3(
            Random.Range(bounds.min.x + safeDistance, bounds.max.x - safeDistance),
            Random.Range(bounds.min.y + safeDistance, bounds.max.y - safeDistance),
            0f
        );
    }

    private void Update()
    {
        if (waveTimer <= 0) return;
        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0)
        {
            waveTimer = 0;
            if (GameManager.Instance.currentWave < GameManager.Instance.maxWave)
                NextWave();
            else
                GoodGame();
        }
    }

    private void NextWave()
    {
        GameManager.Instance.money += GameManager.Instance.propData.harvest;
        GameManager.Instance.currentWave += 1;
        SceneManager.LoadScene("04-Shop");
    }

    // ── 游戏胜利 ────────────────────────────────────────────────────────────

    public void GoodGame()
    {
        // 广播通关事件（SuccessPanel 监听并显示自己）
        EventCenter.Instance.EventTrigger(E_EventType.Battle_AllWavesCompleted);

        // 委托 ProgressService 处理解锁逻辑
        ProgressService.Instance.OnGameCompleted(
            GameManager.Instance.currentRoleData?.name ?? "",
            GameManager.Instance.currentDifficulty?.name ?? "",
            GameManager.Instance.currentWave
        );

        KillAllEnemies();
        StartCoroutine(GoMenu());
    }

    // ── 游戏失败 ────────────────────────────────────────────────────────────

    public void BadGame()
    {
        // 广播死亡事件（FailPanel 监听并显示自己）
        EventCenter.Instance.EventTrigger(E_EventType.Battle_PlayerDied);

        KillAllEnemies();
        StartCoroutine(GoMenu());
    }

    private void KillAllEnemies()
    {
        for (int i = 0; i < enemy_list.Count; i++)
        {
            if (enemy_list[i])
                enemy_list[i].Dead();
        }
    }

    private IEnumerator GoMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
    }
}
