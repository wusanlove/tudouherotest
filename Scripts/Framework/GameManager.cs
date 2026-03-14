using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Random = System.Random;

/// <summary>
/// GameManager — Bootstrap 入口 + 运行时玩家状态容器。
///
/// 职责：
///   ① 游戏启动时初始化所有服务单例（Bootstrap）。
///   ② 跨场景持久化运行时数据（当前选角/武器/难度/血量/金币等）。
///
/// 不再负责：
///   · JSON 加载 → 由 ConfigService 统一处理。
///   · 场景切换 → 由 SceneFlowController + EventCenter 统一处理。
///   · UI 控制   → 由 UIFlowController 统一处理。
///
/// 未来演进思路（DI）：
///   将服务注册移到 Zenject Installer，GameManager 退化为纯粹的
///   "运行时数据容器（GameContext）"，不再持有服务单例引用。
/// </summary>
public class GameManager : BaseMgrMono<GameManager>
{
    // ── 选择数据（LevelSelect 阶段写入，GamePlay 阶段读取） ───────
    public RoleData currentRoleData;
    public List<WeaponData> currentWeapons = new List<WeaponData>();
    public DifficultyData currentDifficulty;
    public List<PropData> currentProps = new List<PropData>();

    // ── 玩家运行时状态 ────────────────────────────────────────────
    public Transform weaponsPos;
    public SpriteAtlas propsAtlas;
    public float speed = 5f;
    public float hp;
    public float money = 0;
    public float exp = 0;
    public bool isDead = false;
    public PropData propData = new PropData();

    // ── 关卡进度 ──────────────────────────────────────────────────
    public int currentWave = 1;
    public int maxWave;
    public int waveCount = 1;

    // ── 场景对象引用（挂载在 GameManager 预制体上，跨场景使用） ───
    public Transform moeny_prefab;
    public Transform number_prefab;
    public GameObject enemyBullet_prefab;
    public GameObject arrowBullet_prefab;
    public GameObject pistolBullet_prefab;
    public GameObject medicalBullet_prefab;

    // ── 音效预制体（兼容旧逻辑，后续迁移到 AudioMgr + AudioRegistry）──
    public GameObject attackMusic;
    public GameObject shootMusic;
    public GameObject menuMusic;
    public GameObject hurtMusic;

    // ── 快捷属性：数据来自 ConfigService（只读缓存）──────────────

    /// <summary>所有角色配置（来自 ConfigService，只读）。</summary>
    public List<RoleData> roleDatas   => ConfigService.Instance.Roles;
    /// <summary>所有武器配置（来自 ConfigService，只读）。</summary>
    public List<WeaponData> weaponDatas => ConfigService.Instance.Weapons;
    /// <summary>所有敌人配置（来自 ConfigService，只读）。</summary>
    public List<EnemyData> enemyDatas  => ConfigService.Instance.Enemies;
    /// <summary>所有道具配置（来自 ConfigService，只读）。</summary>
    public List<PropData> propDatas    => ConfigService.Instance.Props;

    // ── Unity 生命周期 ────────────────────────────────────────────

    public override void Awake()
    {
        base.Awake();
        if (Instance != this) return; // 重复实例，由基类销毁

        DontDestroyOnLoad(gameObject);
        Bootstrap();
    }

    /// <summary>一次性初始化所有服务，仅在首次创建时执行。</summary>
    private void Bootstrap()
    {
        // 预热服务单例（触发构造函数中的事件注册等初始化逻辑）
        _ = MonoMgr.Instance;          // 先创建 MonoHelper，后续服务依赖它
        _ = ConfigService.Instance;    // 预热配置缓存
        _ = SceneFlowController.Instance; // 注册场景流程事件，驱动状态机
        _ = UIFlowController.Instance;    // 注册 UI 流程事件
        _ = AudioMgr.Instance;            // 注册音频事件

        // 加载跨场景图集
        propsAtlas = Resources.Load<SpriteAtlas>("Image/其他/Props");

        // 加载音效预制体（兼容旧方式，待迁移到 AudioRegistry）
        attackMusic = Resources.Load<GameObject>("Audio/AttackMusic");
        shootMusic  = Resources.Load<GameObject>("Audio/ShootMusic");
        menuMusic   = Resources.Load<GameObject>("Audio/MenuMusic");
        hurtMusic   = Resources.Load<GameObject>("Audio/HurtMusic");

        // 初始化角色解锁状态（兼容旧 PlayerPrefs 键名）
        InitLegacyUnlockKeys();

        // 启动主菜单状态（不加载场景，当前已在 MainMenu）
        SceneFlowController.Instance.StartFromMainMenu();
    }

    /// <summary>保证旧版 PlayerPrefs 键存在（不破坏已有存档）。</summary>
    private void InitLegacyUnlockKeys()
    {
        if (!PlayerPrefs.HasKey("多面手")) PlayerPrefs.SetInt("多面手", 0);
        if (!PlayerPrefs.HasKey("公牛"))  PlayerPrefs.SetInt("公牛",  0);
    }

    // ── 游戏逻辑方法 ──────────────────────────────────────────────

    /// <summary>根据选定角色初始化基础属性；进入 GamePlay 前 Player.Awake 调用一次。</summary>
    public void InitProp()
    {
        propData = new PropData();
        if (currentRoleData == null) return;

        switch (currentRoleData.name)
        {
            case "全能者":
                propData.maxHp     += 5;
                propData.speedPer  += 0.05f;
                propData.harvest   += 8;
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
                propData.slot   =  0;
                break;
            case "多面手":
                propData.long_damage  += 0.2f;
                propData.short_damage += 0.2f;
                propData.slot         =  12;
                break;
        }

        hp    = propData.maxHp;
        money = 100;
        exp   = 0;
    }

    /// <summary>购买道具时将道具属性叠加到当前属性中。</summary>
    public void FusionAttr(PropData shopProp)
    {
        propData.maxHp                      += shopProp.maxHp;
        propData.revive                      = Mathf.Max(0, propData.revive + shopProp.revive);
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

    /// <summary>从列表中随机取一个元素（保留旧接口兼容）。</summary>
    public object RandomOne<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[new Random().Next(0, list.Count)];
    }
}

