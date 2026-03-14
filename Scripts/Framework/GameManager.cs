using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using Newtonsoft.Json;
using UnityEngine.U2D;
using Random = System.Random;
public class GameManager : BaseMgrMono<GameManager>
{
    //角色信息
    public RoleData currentRoleData;
    public PropData currentPropData;
    public List<RoleData> roleDatas;
    //武器信息
    public List<WeaponData> currentWeapons;
    public  List<WeaponData> weaponDatas;
    public Transform weaponsPos;
    //难度信息
    public DifficultyData currentDifficulty;
    //道具信息
    public List<PropData> propDatas=new List<PropData>();
    public List<PropData> currentProps=new List<PropData>();
    public SpriteAtlas propsAtlas;
    
    public Transform moeny_prefab; 
    public Transform number_prefab;
    //玩家属性
    public float speed = 5f;
    public float hp;
    public float money = 0;
    public float exp=0;
    
    public bool isDead=false;
    
    //游戏数据
    public int currentWave=1;
    public int maxWave;
    public float waveInterval=10f;
    public float waveTimer=0f;
    public int waveCount=1;
    public PropData propData=new PropData();
    public List<EnemyData> enemyDatas;
    public GameObject enemyBullet_prefab;
    public GameObject arrowBullet_prefab;
    public GameObject pistolBullet_prefab;
    public GameObject medicalBullet_prefab;
    //音效
    public GameObject attackMusic; //攻击音效
    public GameObject shootMusic; //射击音效
    public GameObject menuMusic; //菜单音效
    public GameObject hurtMusic; //受伤音效
    public override void Awake()
    {
        base.Awake();
        propsAtlas = Resources.Load<SpriteAtlas>("Image/其他/Props");
        //获得所有enemy信息
        TextAsset textAsset = Resources.Load<TextAsset>("Data/enemy");
        enemyDatas = JsonConvert.DeserializeObject<List<EnemyData>>(textAsset.text);
        //获取所有角色信息
        TextAsset roleTextAsset = Resources.Load<TextAsset>("Data/role");
        roleDatas = JsonConvert.DeserializeObject<List<RoleData>>(roleTextAsset.text);
        //获取所有武器信息
        TextAsset weaponTextAsset = Resources.Load<TextAsset>("Data/weapon");
        weaponDatas = JsonConvert.DeserializeObject<List<WeaponData>>(weaponTextAsset.text);
        //获取所有道具信息
        TextAsset propTextAsset = Resources.Load<TextAsset>("Data/prop");
        propDatas = JsonConvert.DeserializeObject<List<PropData>>(propTextAsset.text);
        //加载角色解锁状态
        InitUnlockRole();
        //加载音效预制体
        attackMusic = Resources.Load<GameObject>("Audio/AttackMusic");
        shootMusic = Resources.Load<GameObject>("Audio/ShootMusic");
        menuMusic = Resources.Load<GameObject>("Audio/MenuMusic");
        hurtMusic = Resources.Load<GameObject>("Audio/HurtMusic");
        
        DontDestroyOnLoad(gameObject);
    }

    private void InitUnlockRole()
    {
        if (!PlayerPrefs.HasKey("多面手"))
        {
            PlayerPrefs.SetInt("多面手", 0);
        }
        if (!PlayerPrefs.HasKey("公牛"))
        {
            PlayerPrefs.SetInt("公牛", 0);
        }
    }

    public void InitProp()
    {
      

        if (currentRoleData.name == "全能者")
        {
            propData.maxHp += 5;
            propData.speedPer += 0.05f;
            propData.harvest += 8;

        }else if (currentRoleData.name == "斗士")
        {
            propData.short_attackSpeed += 0.5f;
            propData.long_range -= 0.5f;
            propData.short_range -= 0.5f;
            propData.long_damage -= 0.5f;

        }
        else if (currentRoleData.name == "医生")
        {
            propData.revive += 5f;
            propData.short_attackSpeed -= 0.5f;
            propData.long_attackSpeed -= 0.5f;
        }
        else if (currentRoleData.name == "公牛")
        {
            propData.maxHp += 20f;
            propData.revive += 15f;
            propData.slot = 0;
            
        }
        else if (currentRoleData.name == "多面手")
        {
            propData.long_damage += 0.2f;
            propData.short_damage += 0.2f;
            propData.slot = 12;
        }


        hp = propData.maxHp;
        money = 100;
        exp = 0;

    }


    public object RandomOne<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return null;
        }

        Random random = new Random();//使用c#中的random类
        int index = random.Next(0, list.Count);

        return list[index];
    }
    //融合属性
    public void FusionAttr(PropData shopProp)
    {
        propData.maxHp += shopProp.maxHp;
        
        propData.revive += shopProp.revive; 
        if(propData.revive<0)
            propData.revive=0;
        propData.short_damage += shopProp.short_damage; 
        propData.short_range += shopProp.short_range; 
        propData.short_attackSpeed += shopProp.short_attackSpeed; 
        propData.long_damage += shopProp.long_damage; 
        propData.long_range += shopProp.long_range; 
        propData.long_attackSpeed += shopProp.long_attackSpeed; 
        propData.speedPer += shopProp.speedPer; 
        propData.harvest += shopProp.harvest; 
        propData.shopDiscount += shopProp.shopDiscount; 
        propData.expMuti += shopProp.expMuti; 
        propData.pickRange += shopProp.pickRange;
        propData.critical_strikes_probability += shopProp.critical_strikes_probability; 
        
    }
}

