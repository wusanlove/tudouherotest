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
        //遍历所有波次信息
        //TODO:可以测试的时候不按时间轴生成，而且不报错
        foreach (WaveData waveData in currentLevelData.enemys)
        {
            //遍历敌人数量
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
                        enemy.Init(e);
                        break;
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
        }

        GamePanel.Instance.RenewCountDown(waveTimer);

    }
    
   // 进入下一关
     private void NextWave()
     {
         GameManager.Instance.money += GameManager.Instance.propData.harvest; //收获添加到金币中
         GameManager.Instance.currentWave += 1; //波次+1
         if(GameManager.Instance.currentWave <= GameManager.Instance.maxWave)
            SceneManager.LoadScene("04-Shop");  //跳转商店
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
        
        //todo ：所有敌人 消失
        for (int i = 0; i < enemy_list.Count; i++)
        {
            if (enemy_list[i])
            {
                enemy_list[i].Dead();
            }
          
        }

        if (PlayerPrefs.GetInt("多面手") == 0)
        {
            Debug.Log("多面手解锁");
            PlayerPrefs.SetInt("多面手", 1);
        
            for (int i = 0; i < GameManager.Instance.roleDatas.Count; i++)
            {
                if (GameManager.Instance.roleDatas[i].name == "多面手")
                {
                    GameManager.Instance.roleDatas[i].unlock = 1;
                }
            }
        }
        
    }
    
    
    //todo 波次完成
    
    //游戏失败
    public void BadGame()
    {
        _failPanel.GetComponent<CanvasGroup>().alpha = 1;
        StartCoroutine(GoMenu());
        
        
        //todo 所有敌人 消失
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
         SceneManager.LoadScene(0);
     }
}
