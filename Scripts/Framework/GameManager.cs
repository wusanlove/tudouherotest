using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Random = System.Random;

/// <summary>
/// GameManager：跨场景持久化的游戏状态容器。
/// - 不再直接解析 JSON；所有配置通过 <see cref="ConfigService"/> 加载。
/// - 角色解锁初始化转移至 <see cref="PlayerPrefsProgressService"/>，由其统一管理。
/// - 音效预制体由 AudioMgr + AudioRegistry 管理，此处不再 Resources.Load 音效。
/// </summary>
public class GameManager : BaseMgrMono<GameManager>
{
    // ── 角色 / 武器 / 难度 / 道具 ──────────────────────────────────────────
    public RoleData       currentRoleData;
    public PropData       currentPropData;
    public List<RoleData> roleDatas;

    public List<WeaponData> currentWeapons = new List<WeaponData>();
    public List<WeaponData> weaponDatas;
    public Transform weaponsPos;

    public DifficultyData currentDifficulty;

    public List<PropData> propDatas    = new List<PropData>();
    public List<PropData> currentProps = new List<PropData>();
    public SpriteAtlas    propsAtlas;

    public Transform moeny_prefab;
    public Transform number_prefab;

    // ── 玩家属性 ──────────────────────────────────────────────────────────
    public float speed  = 5f;
    public float hp;
    public float money  = 0;
    public float exp    = 0;
    public bool  isDead = false;

    // ── 关卡 / 波次 ───────────────────────────────────────────────────────
    public int   currentWave   = 1;
    public int   maxWave;
    public float waveInterval  = 10f;
    public float waveTimer     = 0f;
    public int   waveCount     = 1;
    public PropData propData   = new PropData();

    public List<EnemyData> enemyDatas;

    // ── Prefabs（Inspector 引用，仍由场景/预制体挂载）───────────────────
    public GameObject enemyBullet_prefab;
    public GameObject arrowBullet_prefab;
    public GameObject pistolBullet_prefab;
    public GameObject medicalBullet_prefab;

    public override void Awake()
    {
        base.Awake();

        // ── 使用 ConfigService 集中加载所有 JSON 配置 ──────────────────
        enemyDatas  = ConfigService.Instance.LoadEnemies();
        roleDatas   = ConfigService.Instance.LoadRoles();
        weaponDatas = ConfigService.Instance.LoadWeapons();
        propDatas   = ConfigService.Instance.LoadProps();

        // ── 道具图集 ─────────────────────────────────────────────────────
        propsAtlas = Resources.Load<SpriteAtlas>("Image/其他/Props");

        // ── 角色解锁状态初始化（委托 ProgressService）────────────────────
        ProgressService.Instance.Load();

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>根据选定角色初始化 propData 与玩家属性。</summary>
    public void InitProp()
    {
        if (currentRoleData == null) return;

        switch (currentRoleData.name)
        {
            case "全能者":
                propData.maxHp      += 5;
                propData.speedPer   += 0.05f;
                propData.harvest    += 8;
                break;
            case "斗士":
                propData.short_attackSpeed += 0.5f;
                propData.long_range        -= 0.5f;
                propData.short_range       -= 0.5f;
                propData.long_damage       -= 0.5f;
                break;
            case "医生":
                propData.revive            += 5f;
                propData.short_attackSpeed -= 0.5f;
                propData.long_attackSpeed  -= 0.5f;
                break;
            case "公牛":
                propData.maxHp  += 20f;
                propData.revive += 15f;
                propData.slot    = 0;
                break;
            case "多面手":
                propData.long_damage  += 0.2f;
                propData.short_damage += 0.2f;
                propData.slot         = 12;
                break;
        }

        hp    = propData.maxHp;
        money = 100;
        exp   = 0;
    }

    /// <summary>从列表中随机返回一个元素（使用 System.Random 保证线程无关）。</summary>
    public object RandomOne<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return null;
        Random rng = new Random();
        return list[rng.Next(0, list.Count)];
    }

    /// <summary>将商店道具的所有属性累加到当前 propData（Fusion 模式）。</summary>
    public void FusionAttr(PropData shopProp)
    {
        propData.maxHp                       += shopProp.maxHp;
        propData.revive                      += shopProp.revive;
        if (propData.revive < 0) propData.revive = 0;
        propData.short_damage                += shopProp.short_damage;
        propData.short_range                 += shopProp.short_range;
        propData.short_attackSpeed           += shopProp.short_attackSpeed;
        propData.long_damage                 += shopProp.long_damage;
        propData.long_range                  += shopProp.long_range;
        propData.long_attackSpeed            += shopProp.long_attackSpeed;
        propData.speedPer                    += shopProp.speedPer;
        propData.harvest                     += shopProp.harvest;
        propData.shopDiscount                += shopProp.shopDiscount;
        propData.expMuti                     += shopProp.expMuti;
        propData.pickRange                   += shopProp.pickRange;
        propData.critical_strikes_probability+= shopProp.critical_strikes_probability;
    }
}


