using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 关卡控制器 — 驱动波次计时、生成敌人、处理胜利/失败流程。
///
/// 改进：
///   · 波次数据由 ConfigService 加载，不再自行解析 JSON。
///   · 波次计时每帧发 Game_WaveTimerChanged 事件，GamePanel 订阅刷新，
///     不再每帧直接调用 GamePanel.Instance.RenewCountDown()。
///   · 场景切换统一通过 EventCenter → SceneFlowController，不再直接调 SceneManager。
/// </summary>
public class LevelControl : BaseMgrMono<LevelControl>
{
    [HideInInspector] public float waveTimer;

    public GameObject _failPanel;
    public GameObject _successPanel;
    public List<EnemyBase> enemy_list;
    public Transform _map;

    public GameObject enemy1_prefab;
    public GameObject enemy2_prefab;
    public GameObject enemy3_prefab;
    public GameObject enemy4_prefab;
    public GameObject enemy5_prefab;
    public Transform  enemyFather;
    public GameObject redfork_prefab;

    private List<LevelData> levelDatas;
    private LevelData currentLevelData;
    private Dictionary<string, GameObject> enemyPrefabDic;

    public override void Awake()
    {
        base.Awake();

        // 数据来自 ConfigService，避免重复 JSON 解析
        string levelName = GameManager.Instance.currentDifficulty?.levelName;
        if (!string.IsNullOrEmpty(levelName))
            levelDatas = ConfigService.Instance.GetLevelData(levelName);

        if (levelDatas != null)
            GameManager.Instance.maxWave = levelDatas.Count;

        enemyPrefabDic = new Dictionary<string, GameObject>
        {
            { "enemy1", enemy1_prefab },
            { "enemy2", enemy2_prefab },
            { "enemy3", enemy3_prefab },
            { "enemy4", enemy4_prefab },
            { "enemy5", enemy5_prefab },
        };
    }

    private void Start()
    {
        if (levelDatas == null || levelDatas.Count == 0)
        {
            Debug.LogError("[LevelControl] 关卡数据为空！");
            return;
        }
        currentLevelData = levelDatas[GameManager.Instance.currentWave - 1];
        waveTimer        = currentLevelData.waveTimer;

        GenerateEnemy();
        GenerateWeapon();
    }

    private void Update()
    {
        if (waveTimer <= 0) return;

        waveTimer -= Time.deltaTime;
        if (waveTimer < 0) waveTimer = 0;

        // 通知 GamePanel 刷新倒计时（事件驱动）
        EventCenter.Instance.EventTrigger(E_EventType.Game_WaveTimerChanged, waveTimer);

        if (waveTimer <= 0)
        {
            if (GameManager.Instance.currentWave < GameManager.Instance.maxWave)
                NextWave();
            else
                GoodGame();
        }
    }

    // ── 波次逻辑 ──────────────────────────────────────────────────

    private void NextWave()
    {
        GameManager.Instance.money += GameManager.Instance.propData.harvest;
        GameManager.Instance.currentWave += 1;
        GameManager.Instance.waveCount   += 1;

        if (GameManager.Instance.currentWave <= GameManager.Instance.maxWave)
            EventCenter.Instance.EventTrigger(E_EventType.Flow_GoToShop);
        else
            GoodGame();
    }

    public void GoodGame()
    {
        _successPanel.GetComponent<CanvasGroup>().alpha = 1;

        // 解锁"多面手"
        if (PlayerPrefs.GetInt("多面手") == 0)
        {
            PlayerPrefs.SetInt("多面手", 1);
            var roles = GameManager.Instance.roleDatas;
            if (roles != null)
                foreach (var r in roles)
                    if (r.name == "多面手") r.unlock = 1;
        }

        for (int i = 0; i < enemy_list.Count; i++)
            if (enemy_list[i]) enemy_list[i].Dead();

        StartCoroutine(GoMenu());
    }

    public void BadGame()
    {
        _failPanel.GetComponent<CanvasGroup>().alpha = 1;

        for (int i = 0; i < enemy_list.Count; i++)
            if (enemy_list[i]) enemy_list[i].Dead();

        StartCoroutine(GoMenu());
    }

    private IEnumerator GoMenu()
    {
        yield return new WaitForSeconds(3);
        EventCenter.Instance.EventTrigger(E_EventType.Flow_GoToMainMenu);
    }

    // ── 生成方法 ─────────────────────────────────────────────────

    private void GenerateWeapon()
    {
        int i = 0;
        foreach (WeaponData wd in GameManager.Instance.currentWeapons)
        {
            GameObject go = Resources.Load<GameObject>("Prefabs/武器/" + wd.name);
            if (go == null) { i++; continue; }
            WeaponBase wb = Instantiate(go, Player.Instance.weaponsPos.GetChild(i).transform)
                              .GetComponent<WeaponBase>();
            wb.data = wd;
            i++;
        }
    }

    private void GenerateEnemy()
    {
        if (currentLevelData?.enemys == null) return;
        foreach (WaveData waveData in currentLevelData.enemys)
            for (int i = 0; i < waveData.count; i++)
                StartCoroutine(SpawnEnemy(waveData));
    }

    private IEnumerator SpawnEnemy(WaveData waveData)
    {
        yield return new WaitForSeconds(waveData.timeAxis);
        if (waveTimer <= 0 || GameManager.Instance.isDead) yield break;

        Vector3 pos = GetRandomPosition(_map.GetComponent<SpriteRenderer>().bounds);
        GameObject fork = Instantiate(redfork_prefab, pos, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Destroy(fork);

        if (waveTimer <= 0 || GameManager.Instance.isDead) yield break;

        if (!enemyPrefabDic.TryGetValue(waveData.enemyName, out GameObject prefab))
        {
            Debug.LogWarning($"[LevelControl] 找不到敌人预制体：{waveData.enemyName}，跳过生成。");
            yield break;
        }

        EnemyBase enemy = Instantiate(prefab, pos, Quaternion.identity).GetComponent<EnemyBase>();
        enemy.transform.SetParent(enemyFather);

        List<EnemyData> allEnemies = GameManager.Instance.enemyDatas;
        if (allEnemies != null)
            foreach (EnemyData ed in allEnemies)
                if (ed.name == waveData.enemyName) { enemy.enemyData = ed; break; }

        if (waveData.elite == 1) enemy.SetElite();
        enemy_list.Add(enemy);
    }

    private static Vector3 GetRandomPosition(Bounds bounds)
    {
        const float safe = 3.5f;
        return new Vector3(
            Random.Range(bounds.min.x + safe, bounds.max.x - safe),
            Random.Range(bounds.min.y + safe, bounds.max.y - safe),
            0f);
    }
}
