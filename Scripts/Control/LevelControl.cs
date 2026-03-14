using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡/波次控制器：从 ConfigService 读取波次数据，通过 EnemyFactory 生成敌人，
/// 通过 GameFlowController 跳转商店，不直接调用 GamePanel。
/// </summary>
public class LevelControl : BaseMgrMono<LevelControl>
{
    public float waveTimer;
    public GameObject _failPanel;
    public GameObject _successPanel;
    public List<EnemyBase> enemy_list = new List<EnemyBase>();
    public Transform _map;

    [Header("敌人预制体（Inspector 拖入）")]
    public GameObject enemy1_prefab;
    public GameObject enemy2_prefab;
    public GameObject enemy3_prefab;
    public GameObject enemy4_prefab;
    public GameObject enemy5_prefab;
    public Transform  enemyFather;

    public GameObject redfork_prefab;

    private LevelData currentLevelData;
    private List<LevelData> levelDatas;

    public override void Awake()
    {
        base.Awake();

        // 通过 ConfigService 读取关卡数据（缓存，不重复读文件）
        levelDatas = ConfigService.Instance.GetLevelDatas(GameManager.Instance.currentDifficulty.levelName);
        GameManager.Instance.maxWave = levelDatas.Count;

        // 向 EnemyFactory 注册预制体（不再用原始字典）
        EnemyFactory.Instance.Register("enemy1", enemy1_prefab);
        EnemyFactory.Instance.Register("enemy2", enemy2_prefab);
        EnemyFactory.Instance.Register("enemy3", enemy3_prefab);
        EnemyFactory.Instance.Register("enemy4", enemy4_prefab);
        EnemyFactory.Instance.Register("enemy5", enemy5_prefab);
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
            GameObject go = Resources.Load<GameObject>($"Prefabs/武器/{weaponData.name}");
            WeaponBase wb = Instantiate(go, Player.Instance.weaponsPos.GetChild(i).transform)
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
                StartCoroutine(SpawnEnemy(waveData));
        }
    }

    IEnumerator SpawnEnemy(WaveData waveData)
    {
        yield return new WaitForSeconds(waveData.timeAxis);

        if (waveTimer <= 0 || GameManager.Instance.isDead) yield break;

        Vector3 spawnPoint = GetRandomPosition(_map.GetComponent<SpriteRenderer>().bounds);

        // 红叉提示
        GameObject fork = Instantiate(redfork_prefab, spawnPoint, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Destroy(fork);

        if (waveTimer <= 0 || GameManager.Instance.isDead) yield break;

        // 通过 EnemyFactory 生成敌人
        EnemyBase enemy = EnemyFactory.Instance.Spawn(waveData.enemyName, spawnPoint, enemyFather);
        if (enemy == null) yield break;

        if (waveData.elite == 1)
            enemy.SetElite();

        enemy_list.Add(enemy);
    }

    private Vector3 GetRandomPosition(Bounds bounds)
    {
        float safe = 3.5f;
        return new Vector3(
            Random.Range(bounds.min.x + safe, bounds.max.x - safe),
            Random.Range(bounds.min.y + safe, bounds.max.y - safe),
            0f);
    }

    private void Update()
    {
        if (waveTimer <= 0) return;

        waveTimer -= Time.deltaTime;

        // 广播倒计时事件（GamePanel 订阅后刷新）
        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_CountDown, waveTimer);

        if (waveTimer <= 0)
        {
            waveTimer = 0;
            if (GameManager.Instance.currentWave <= GameManager.Instance.maxWave)
                NextWave();
            else
                GoodGame();
        }
    }

    private void NextWave()
    {
        GameManager.Instance.money += GameManager.Instance.propData.harvest;
        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_MoneyChanged, GameManager.Instance.money);
        GameManager.Instance.currentWave += 1;

        if (GameManager.Instance.currentWave <= GameManager.Instance.maxWave)
        {
            // 跳转商店，由 GameFlowController 管理
            GameFlowController.Instance.GoToShop();
        }
        else
        {
            GoodGame();
        }
    }

    public void GoodGame()
    {
        var cg = _successPanel.GetComponent<CanvasGroup>();
        cg.alpha = 1; cg.interactable = false; cg.blocksRaycasts = false;

        StartCoroutine(GoMenu());
        KillAllEnemies();

        // 解锁"多面手"
        if (!SaveService.Instance.IsRoleUnlocked("多面手"))
        {
            SaveService.Instance.UnlockRole("多面手");
            Debug.Log("多面手解锁");
            SyncRoleUnlockToConfig("多面手");
        }
    }

    public void BadGame()
    {
        var cg = _failPanel.GetComponent<CanvasGroup>();
        cg.alpha = 1; cg.interactable = false; cg.blocksRaycasts = false;

        StartCoroutine(GoMenu());
        KillAllEnemies();
    }

    private void KillAllEnemies()
    {
        for (int i = 0; i < enemy_list.Count; i++)
            if (enemy_list[i]) enemy_list[i].Dead();
    }

    IEnumerator GoMenu()
    {
        yield return new WaitForSeconds(3f);
        GameFlowController.Instance.GoToMainMenu();
    }

    private void SyncRoleUnlockToConfig(string roleName)
    {
        var roles = ConfigService.Instance.Roles;
        for (int i = 0; i < roles.Count; i++)
            if (roles[i].name == roleName) { roles[i].unlock = 1; break; }
    }
}
