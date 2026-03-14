using System.Collections.Generic;
using SceneState;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.U2D;
using Random = System.Random;

/// <summary>
/// 游戏全局管理器（跨场景单例）
/// 职责：持有所有配置数据、当前会话状态、以及驱动场景状态机。
/// ──────────────────────────────────────────────────────────────
/// 演进提示：目前所有子系统通过 GameManager.Instance 直接访问（单例聚合）。
/// 后续可引入 ServiceLocator 或依赖注入容器，将 ConfigService / AudioService /
/// ProgressService 各自独立注册，GameManager 仅保留会话状态与场景流转入口。
/// </summary>
public class GameManager : BaseMgrMono<GameManager>
{
    // ── 配置数据（从JSON加载，只读；集中在此处，各Panel直接读，避免各Panel重复Load）
    public List<RoleData>       roleDatas;
    public List<WeaponData>     weaponDatas;
    public List<EnemyData>      enemyDatas;
    public List<PropData>       propDatas = new List<PropData>();
    public List<DifficultyData> difficultyDatas;

    // ── 当前会话选择（玩家在选择场景做出的决定）
    public RoleData         currentRoleData;
    public DifficultyData   currentDifficulty;
    public List<WeaponData> currentWeapons = new List<WeaponData>();
    public List<PropData>   currentProps   = new List<PropData>();

    // ── 玩家运行时属性
    public PropData propData = new PropData();
    public float    speed    = 5f;
    public float    hp;
    public float    money    = 0;
    public float    exp      = 0;
    public bool     isDead   = false;

    // ── 关卡进度
    public int   currentWave  = 1;
    public int   maxWave;
    public float waveInterval = 10f;
    public float waveTimer    = 0f;
    public int   waveCount    = 1;

    // ── 共享资源引用（由 Inspector 赋值）
    public SpriteAtlas propsAtlas;
    public Transform   weaponsPos;
    public Transform   moeny_prefab;
    public Transform   number_prefab;
    public GameObject  enemyBullet_prefab;
    public GameObject  arrowBullet_prefab;
    public GameObject  pistolBullet_prefab;
    public GameObject  medicalBullet_prefab;

    // ── 音效预制体（旧方案保留兼容；后续可统一交给 AudioMgr 管理）
    public GameObject attackMusic;
    public GameObject shootMusic;
    public GameObject menuMusic;
    public GameObject hurtMusic;

    // ── 场景状态机（GameManager 是其宿主，因为它跨场景持久化）
    private SceneStateController sceneController;

    public override void Awake()
    {
        base.Awake();

        // ── 集中加载所有静态配置 JSON
        enemyDatas      = LoadJson<List<EnemyData>>("Data/enemy");
        roleDatas       = LoadJson<List<RoleData>>("Data/role");
        weaponDatas     = LoadJson<List<WeaponData>>("Data/weapon");
        propDatas       = LoadJson<List<PropData>>("Data/prop");
        difficultyDatas = LoadJson<List<DifficultyData>>("Data/difficulty");

        propsAtlas = Resources.Load<SpriteAtlas>("Image/其他/Props");

        // ── 加载音效预制体（TODO: 迁移至 AudioMgr/AudioRegistry）
        attackMusic = Resources.Load<GameObject>("Audio/AttackMusic");
        shootMusic  = Resources.Load<GameObject>("Audio/ShootMusic");
        menuMusic   = Resources.Load<GameObject>("Audio/MenuMusic");
        hurtMusic   = Resources.Load<GameObject>("Audio/HurtMusic");

        InitUnlockRole();

        // ── 初始化场景状态机并订阅场景切换事件
        sceneController = new SceneStateController();
        SubscribeSceneEvents();

        // 游戏只能从 01-MainMenu 启动，无需重新加载场景
        sceneController.SetState(new StartScene(sceneController), isLoadScene: false);

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 推动场景状态机：检测异步加载完成并依次调用 StateStart / StateUpdate
        sceneController?.StateUpdate();
    }

    // ── 订阅全局场景切换事件（GameManager 跨场景存活，只需注册一次）
    private void SubscribeSceneEvents()
    {
        EventCenter.Instance.AddEventListener(E_EventType.Scene_GoToSelect, () =>
        {
            ResetSession();
            sceneController.SetState(new SelectSecene(sceneController));
        });

        EventCenter.Instance.AddEventListener(E_EventType.Scene_GoToGame, () =>
        {
            sceneController.SetState(new GameScene(sceneController));
        });

        EventCenter.Instance.AddEventListener(E_EventType.Scene_GoToShop, () =>
        {
            sceneController.SetState(new ShopScene(sceneController));
        });

        EventCenter.Instance.AddEventListener(E_EventType.Scene_GoToMenu, () =>
        {
            sceneController.SetState(new StartScene(sceneController));
        });
    }

    /// <summary>
    /// 重置本局会话数据（进入选择场景时调用，确保新局从干净状态开始）
    /// </summary>
    public void ResetSession()
    {
        currentWeapons.Clear();
        currentProps.Clear();
        currentWave       = 1;
        waveCount         = 1;
        isDead            = false;
        propData          = new PropData();
        currentRoleData   = null;
        currentDifficulty = null;
    }

    /// <summary>
    /// 根据选定角色初始化角色专属加成，并重置初始 HP / 金币 / 经验。
    /// 由 Player.Awake() 在第一波时调用。
    /// TODO: 用角色 ID（int）替代字符串比较，防止改名导致的隐性 bug。
    ///       可将每个角色的属性加成抽象为 RoleData 中的字段，彻底消除此 switch。
    /// </summary>
    public void InitProp()
    {
        if (currentRoleData == null) return;

        switch (currentRoleData.name)
        {
            case "全能者":
                propData.maxHp    += 5;
                propData.speedPer += 0.05f;
                propData.harvest  += 8;
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

    /// <summary>
    /// 将商店购买的道具属性融合到全局 propData 中。
    /// </summary>
    public void FusionAttr(PropData shopProp)
    {
        propData.maxHp     += shopProp.maxHp;
        propData.revive    += shopProp.revive;
        if (propData.revive < 0) propData.revive = 0;

        propData.short_damage      += shopProp.short_damage;
        propData.short_range       += shopProp.short_range;
        propData.short_attackSpeed += shopProp.short_attackSpeed;
        propData.long_damage       += shopProp.long_damage;
        propData.long_range        += shopProp.long_range;
        propData.long_attackSpeed  += shopProp.long_attackSpeed;
        propData.speedPer          += shopProp.speedPer;
        propData.harvest           += shopProp.harvest;
        propData.shopDiscount      += shopProp.shopDiscount;
        propData.expMuti           += shopProp.expMuti;
        propData.pickRange         += shopProp.pickRange;
        propData.critical_strikes_probability += shopProp.critical_strikes_probability;
    }

    /// <summary>从列表中随机返回一个元素。</summary>
    public object RandomOne<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[new Random().Next(0, list.Count)];
    }

    // ── 内部工具：加载并反序列化 Resources 下的 JSON 文件
    private static T LoadJson<T>(string path)
    {
        TextAsset text = Resources.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<T>(text.text);
    }

    private void InitUnlockRole()
    {
        if (!PlayerPrefs.HasKey("多面手")) PlayerPrefs.SetInt("多面手", 0);
        if (!PlayerPrefs.HasKey("公牛"))   PlayerPrefs.SetInt("公牛",   0);
    }
}
