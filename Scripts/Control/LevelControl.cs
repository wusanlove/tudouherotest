using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class LevelControl : BaseMgrMono<LevelControl>
{
    public float waveTimer;
    public GameObject _failPanel;
    public GameObject _successPanel;
    public List<EnemyBase> enemy_list;
    public Transform _map;

    public GameObject enemy1_prefab;
    public GameObject enemy2_prefab;
    public GameObject enemy3_prefab;
    public GameObject enemy4_prefab;
    public GameObject enemy5_prefab;
    public Transform enemyFather;

    public GameObject redfork_prefab;
    public List<LevelData> levelDatas = new List<LevelData>();
    private LevelData currentLevelData;

    public Dictionary<string, GameObject> enemyPrefabDic = new Dictionary<string, GameObject>();

    public override void Awake()
    {
        base.Awake();

        // 通过 ConfigMgr 按当前难度加载关卡数据
        string levelKey = GameManager.Instance.currentDifficulty.levelName;
        levelDatas = ConfigMgr.Instance.LoadList<LevelData>(levelKey);
        GameManager.Instance.maxWave = levelDatas.Count;

        enemyPrefabDic["enemy1"] = enemy1_prefab;
        enemyPrefabDic["enemy2"] = enemy2_prefab;
        enemyPrefabDic["enemy3"] = enemy3_prefab;
        enemyPrefabDic["enemy4"] = enemy4_prefab;
        enemyPrefabDic["enemy5"] = enemy5_prefab;
    }

    private void Start()
    {
        currentLevelData = levelDatas[GameManager.Instance.currentWave - 1];
        waveTimer = currentLevelData.waveTimer;
        GenerateEnemy();
        GenerateWeapon();

        EventCenter.Instance.EventTrigger(E_EventType.Wave_Started, GameManager.Instance.currentWave);
    }

    private void GenerateWeapon()
    {
        int i = 0;
        foreach (WeaponData weaponData in GameManager.Instance.currentWeapons)
        {
            GameObject go = Resources.Load<GameObject>("Prefabs/武器/" + weaponData.name);
            WeaponBase wb = Instantiate(go, Player.Instance.weaponsPos.GetChild(i).transform).GetComponent<WeaponBase>();
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

    private IEnumerator SpawnEnemy(WaveData waveData)
    {
        yield return new WaitForSeconds(waveData.timeAxis);

        if (waveTimer > 0 && !GameManager.Instance.isDead)
        {
            var spawnPoint = GetRandomPosition(_map.GetComponent<SpriteRenderer>().bounds);

            GameObject fork = Instantiate(redfork_prefab, spawnPoint, Quaternion.identity);
            yield return new WaitForSeconds(1);
            Destroy(fork);

            if (waveTimer > 0 && !GameManager.Instance.isDead)
            {
                EnemyBase enemy = Instantiate(enemyPrefabDic[waveData.enemyName], spawnPoint, Quaternion.identity)
                    .GetComponent<EnemyBase>();
                enemy.transform.parent = enemyFather;

                foreach (EnemyData e in GameManager.Instance.enemyDatas)
                {
                    if (e.name == waveData.enemyName)
                    {
                        enemy.enemyData = e;
                        if (waveData.elite == 1) enemy.SetElite();
                        break;
                    }
                }

                enemy_list.Add(enemy);
            }
        }
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
        if (waveTimer > 0)
        {
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

        GamePanel.Instance.RenewCountDown(waveTimer);
    }

    private void NextWave()
    {
        EventCenter.Instance.EventTrigger(E_EventType.Wave_Ended, GameManager.Instance.currentWave);
        GameManager.Instance.money += GameManager.Instance.propData.harvest;
        GameManager.Instance.currentWave++;
        // 场景切换交由 EventCenter → GameManager → SceneStateController
        EventCenter.Instance.EventTrigger(E_EventType.Scene_ToShop);
    }

    public void GoodGame()
    {
        _successPanel.GetComponent<CanvasGroup>().alpha = 1;
        StartCoroutine(GoMenu());

        DismissAllEnemies();

        EventCenter.Instance.EventTrigger(E_EventType.Wave_AllCleared);

        if (PlayerPrefs.GetInt("多面手") == 0)
        {
            PlayerPrefs.SetInt("多面手", 1);
            foreach (var r in GameManager.Instance.roleDatas)
            {
                if (r.name == "多面手") r.unlock = 1;
            }
        }
    }

    public void BadGame()
    {
        _failPanel.GetComponent<CanvasGroup>().alpha = 1;
        StartCoroutine(GoMenu());
        DismissAllEnemies();
    }

    private void DismissAllEnemies()
    {
        for (int i = 0; i < enemy_list.Count; i++)
        {
            if (enemy_list[i]) enemy_list[i].Dead();
        }
    }

    private IEnumerator GoMenu()
    {
        yield return new WaitForSeconds(3);
        // 通过 EventCenter 触发回主菜单（GameManager 负责重置状态）
        EventCenter.Instance.EventTrigger(E_EventType.Scene_ToMainMenu);
    }
}
