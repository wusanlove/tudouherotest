using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class LevelControl :BaseMgrMono<LevelControl>
{
    public float waveTimer;
    public GameObject _failPanel; //失败面板
    public GameObject _successPanel; //成功面板
    public List<EnemyBase> enemy_list; //所有敌人列表
    public Transform _map;
    
    public GameObject enemy1_prefab;  // 敌人1预制体
    public GameObject enemy2_prefab;  // 敌人2预制体
    public GameObject enemy3_prefab;  // 敌人3预制体
    public GameObject enemy4_prefab;  // 敌人4预制体
    public GameObject enemy5_prefab;  // 敌人5预制体
    public Transform enemyFather; 
    
    public GameObject redfork_prefab;  //红叉预制体
    private TextAsset levelTextAsset; //json 
    public List<LevelData> levelDatas = new List<LevelData>(); //所有关卡信息
    private LevelData currentLevelData; //当前关卡信息

    public Dictionary<string, GameObject> enemyPrefabDic = new Dictionary<string, GameObject>();

    public override void Awake()
    {
        base.Awake();
      
        levelTextAsset = Resources.Load<TextAsset>("Data/" + GameManager.Instance.currentDifficulty.levelName);
        levelDatas = JsonConvert.DeserializeObject<List<LevelData>>(levelTextAsset.text);
        GameManager.Instance.maxWave=levelDatas.Count;
        
        
        enemyPrefabDic.Add("enemy1", enemy1_prefab);
        enemyPrefabDic.Add("enemy2", enemy2_prefab);
        enemyPrefabDic.Add("enemy3", enemy3_prefab);
        enemyPrefabDic.Add("enemy4", enemy4_prefab);
        enemyPrefabDic.Add("enemy5", enemy5_prefab);

        // 订阅游戏失败事件（由 Player.Die() 触发）
        EventCenter.Instance.AddEventListener(E_EventType.Game_Lose, BadGame);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener(E_EventType.Game_Lose, BadGame);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentLevelData = levelDatas[GameManager.Instance.currentWave - 1]; //保存当前关卡信息
        waveTimer = currentLevelData.waveTimer; //当前关卡的时间
        //生成敌人
        GenerateEnemy();
        
        //生成武器
        GenerateWeapon();
    }

    private void GenerateWeapon()
    {
    
        int i = 0;
        foreach (WeaponData weaponData in GameManager.Instance.currentWeapons)
        {
            GameObject go = Resources.Load<GameObject>("Prefabs/武器/"+weaponData.name);
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
            {
                StartCoroutine(SwawnEnemies(waveData));
            }
         
        }   
        
       
    }

    IEnumerator  SwawnEnemies(WaveData waveData)
    {
        yield return new WaitForSeconds(waveData.timeAxis);
        
        if (waveTimer > 0 && !GameManager.Instance.isDead)
        {
            var spawnPoint = GetRandomPosition(_map.GetComponent<SpriteRenderer>().bounds);
            
            GameObject go = Instantiate(redfork_prefab, spawnPoint, Quaternion.identity); 
            yield return new WaitForSeconds(1);
            Destroy(go);
            if (waveTimer > 0 && !GameManager.Instance.isDead)
            {
                EnemyBase enemy = Instantiate(enemyPrefabDic[waveData.enemyName], spawnPoint, Quaternion.identity).GetComponent<EnemyBase>();
                enemy.transform.parent = enemyFather;

                foreach (EnemyData e in GameManager.Instance.enemyDatas)
                {
                    if (e.name == waveData.enemyName)
                    {
                        enemy.enemyData = e;
                
                        if (waveData.elite == 1)
                        {
                            enemy.SetElite();
                        }
                    }
                }
                
                if (waveData.elite == 1)
                {
                    enemy.SetElite();
                }
                
                enemy_list.Add(enemy); 
            }
        }
    }
    
    
    
    //获取随机位置
    private Vector3 GetRandomPosition(Bounds bounds)
    {
        float safeDistance = 3.5f; //安全距离
        
        float randomX = Random.Range(bounds.min.x + safeDistance, bounds.max.x - safeDistance);
        float randomY = Random.Range(bounds.min.y + safeDistance, bounds.max.y - safeDistance);
        float randomZ = 0f;
        return new Vector3(randomX, randomY, randomZ);

    }

    // Update is called once per frame
    void Update()
    {
        if (waveTimer > 0 )
        {
            waveTimer -= Time.deltaTime;

            if (waveTimer <= 0)
            {
                waveTimer = 0;

                if (GameManager.Instance.currentWave <= GameManager.Instance.maxWave)
                {
                    Debug.Log(GameManager.Instance.currentWave+"   "+GameManager.Instance.maxWave);
                    NextWave();
                }
                else
                {
                    GoodGame();
                }
                
            }

            // 通过事件推送倒计时更新，解耦 GamePanel.Update 轮询
            EventCenter.Instance.EventTrigger(E_EventType.Wave_TimerUpdated, waveTimer);
        }
    }
    
   // 进入下一关
     private void NextWave()
     {
         GameManager.Instance.money += GameManager.Instance.propData.harvest; //收获添加到金币中
         GameManager.Instance.currentWave += 1; //波次+1

         EventCenter.Instance.EventTrigger(E_EventType.Wave_Ended);

         if(GameManager.Instance.currentWave <= GameManager.Instance.maxWave)
            GameStateMachine.Instance.EnterShop();  //通过状态机跳转商店
         else
         {
             GoodGame();
         }
     }

    
    
    //游戏胜利
    public void GoodGame()
    {
        
        _successPanel.GetComponent<CanvasGroup>().alpha = 1;
        StartCoroutine(GoMenu());
        
        for (int i = 0; i < enemy_list.Count; i++)
        {
            if (enemy_list[i])
            {
                enemy_list[i].Dead();
            }
          
        }

        // 通过进度服务解锁"多面手"
        SaveProgressService.Instance.TryUnlockRole("多面手", true, GameManager.Instance.roleDatas);

        EventCenter.Instance.EventTrigger(E_EventType.Game_Win);
    }
    
    
    //游戏失败
    public void BadGame()
    {
        _failPanel.GetComponent<CanvasGroup>().alpha = 1;
        StartCoroutine(GoMenu());
        
        for (int i = 0; i < enemy_list.Count; i++)
        {
            if (enemy_list[i])
            {
                enemy_list[i].Dead();
            }
          
        }
        
    }
    
     IEnumerator GoMenu()
     {
         yield return new WaitForSeconds(3);
         //通过状态机返回主菜单
         GameStateMachine.Instance.EnterMainMenu();
     }
}
