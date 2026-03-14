using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.U2D;
using Random = System.Random;

/// <summary>
/// 全局运行时状态存储（当前局游戏数据）。
/// 只保存"运行时可变状态"，静态配置数据由 ConfigService 提供。
/// 
/// 依赖注入演进：若引入 DI 容器，将 ConfigService/SaveService 通过构造注入，
/// 去掉内部 Instance 调用，即可实现完全解耦。
/// </summary>
public class GameManager : BaseMgrMono<GameManager>
{
    // ── 当前局选择的数据 ────────────────────────────────
    public RoleData currentRoleData;
    public DifficultyData currentDifficulty;
    public List<WeaponData> currentWeapons = new List<WeaponData>();
    public List<PropData>   currentProps   = new List<PropData>();

    // ── 运行时道具属性（战斗中会被商店修改）────────────
    public PropData propData = new PropData();

    // ── 玩家运行时状态 ──────────────────────────────────
    public float hp;
    public float money  = 0;
    public float exp    = 0;
    public float speed  = 5f;
    public bool  isDead = false;

    // ── 波次状态 ────────────────────────────────────────
    public int currentWave = 1;
    public int maxWave;
    public int waveCount   = 1;

    // ── 场景内需引用的 Transform（由场景对象赋值）───────
    public Transform weaponsPos;
    public Transform moeny_prefab;
    public Transform number_prefab;
    public SpriteAtlas propsAtlas;

    // ── 兼容旧代码的音效引用（后续可完全由 AudioMgr 接管）
    public GameObject attackMusic;
    public GameObject shootMusic;
    public GameObject menuMusic;
    public GameObject hurtMusic;

    public GameObject enemyBullet_prefab;
    public GameObject arrowBullet_prefab;
    public GameObject pistolBullet_prefab;
    public GameObject medicalBullet_prefab;

    public override void Awake()
    {
        base.Awake();
        propsAtlas = Resources.Load<SpriteAtlas>("Image/其他/Props");

        // 音效预制体加载（后续可改为 AudioMgr 统一管理）
        attackMusic  = Resources.Load<GameObject>("Audio/AttackMusic");
        shootMusic   = Resources.Load<GameObject>("Audio/ShootMusic");
        menuMusic    = Resources.Load<GameObject>("Audio/MenuMusic");
        hurtMusic    = Resources.Load<GameObject>("Audio/HurtMusic");

        // 通过 SaveService 初始化角色解锁状态（替代原散落的 PlayerPrefs 调用）
        InitUnlockStates();

        DontDestroyOnLoad(gameObject);
    }

    private void InitUnlockStates()
    {
        // SaveService 使用 HasKey 检查，只会在首次运行时写入默认值
        // 这里只需确保 SaveService 已初始化（访问 Instance 即可触发构造）
        _ = SaveService.Instance;
    }

    /// <summary>根据角色特性初始化运行时 PropData，游戏开始第一波时调用一次</summary>
    public void InitProp()
    {
        propData = new PropData(); // 重置

        switch (currentRoleData?.name)
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
                propData.slot          = 12;
                break;
        }

        hp    = propData.maxHp;
        money = 100;
        exp   = 0;
    }

    /// <summary>随机返回列表中的一个元素</summary>
    public object RandomOne<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[new Random().Next(0, list.Count)];
    }

    /// <summary>商店购买道具后融合属性</summary>
    public void FusionAttr(PropData shopProp)
    {
        propData.maxHp                      += shopProp.maxHp;
        propData.revive                     += shopProp.revive;
        if (propData.revive < 0) propData.revive = 0;
        propData.short_damage               += shopProp.short_damage;
        propData.short_range                += shopProp.short_range;
        propData.short_attackSpeed          += shopProp.short_attackSpeed;
        propData.long_damage                += shopProp.long_damage;
        propData.long_range                 += shopProp.long_range;
        propData.long_attackSpeed           += shopProp.long_attackSpeed;
        propData.speedPer                   += shopProp.speedPer;
        propData.harvest                    += shopProp.harvest;
        propData.shopDiscount               += shopProp.shopDiscount;
        propData.expMuti                    += shopProp.expMuti;
        propData.pickRange                  += shopProp.pickRange;
        propData.critical_strikes_probability += shopProp.critical_strikes_probability;
    }
}

