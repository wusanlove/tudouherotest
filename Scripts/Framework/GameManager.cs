using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using SceneState;

/// <summary>
/// 全局游戏管理器 —— 唯一跨场景持久化的 MonoBehaviour 单例。
/// 职责：持有当局游戏状态（角色/武器/难度/波次/属性）、驱动场景状态机、注册场景流转事件。
/// 注意：JSON 配置加载已集中到 ConfigMgr，此处只保留运行时状态。
/// TODO: 演进方向 → 将 GameState（血量/金币/经验等）提取到独立的 GameStateModel，
///        用 ServiceLocator 或 DI 容器统一注册，GameManager 只做流程调度。
/// </summary>
public class GameManager : BaseMgrMono<GameManager>
{
    // ── 角色 / 武器 / 难度 ───────────────────────────────────────────────
    public RoleData currentRoleData;
    public PropData currentPropData;
    public List<RoleData> roleDatas;

    public List<WeaponData> currentWeapons;
    public List<WeaponData> weaponDatas;
    public Transform weaponsPos;

    public DifficultyData currentDifficulty;

    // ── 道具 ─────────────────────────────────────────────────────────────
    public List<PropData> propDatas = new List<PropData>();
    public List<PropData> currentProps = new List<PropData>();
    public SpriteAtlas propsAtlas;

    // ── 玩家属性（运行时状态）────────────────────────────────────────────
    public float speed = 5f;
    public float hp;
    public float money = 0;
    public float exp = 0;
    public bool isDead = false;
    public PropData propData = new PropData();

    // ── 波次信息 ─────────────────────────────────────────────────────────
    public int currentWave = 1;
    public int maxWave;
    public float waveInterval = 10f;
    public float waveTimer = 0f;
    public int waveCount = 1;

    // ── 静态配置（只读引用）──────────────────────────────────────────────
    public List<EnemyData> enemyDatas;

    // ── 生成节点 / 特效 ──────────────────────────────────────────────────
    public Transform moeny_prefab;
    public Transform number_prefab;
    public GameObject enemyBullet_prefab;
    public GameObject arrowBullet_prefab;
    public GameObject pistolBullet_prefab;
    public GameObject medicalBullet_prefab;

    // ── 音效（旧方式保留，新增请改用 AudioMgr + EventCenter）────────────
    // legacy prefabs removed: use AudioMgr + EventCenter and configure clips in AudioRegistry

    // ── 场景状态机 ───────────────────────────────────────────────────────
    private readonly SceneStateController sceneController = new SceneStateController();

    public override void Awake()
    {
        base.Awake();

        // 通过 ConfigMgr 统一加载（各调用方不再自行 Resources.Load JSON）
        enemyDatas = ConfigMgr.Instance.LoadList<EnemyData>("enemy");
        roleDatas = ConfigMgr.Instance.LoadList<RoleData>("role");
        weaponDatas = ConfigMgr.Instance.LoadList<WeaponData>("weapon");
        propDatas = ConfigMgr.Instance.LoadList<PropData>("prop");

        propsAtlas = Resources.Load<SpriteAtlas>("Image/其他/Props");

        InitUnlockRole();
        RegisterSceneEvents();

        // 首次进入：已在 01-MainMenu，不需要再异步加载场景
        sceneController.SetState(new StartScene(sceneController), false);

        // 通过事件驱动 AudioMgr 播放主菜单 BGM（AudioId 枚举驱动，无需关心音频名称）
        EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, AudioId.BGM_Menu);

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // SceneStateController 通过此帧更新检测异步加载完成并调用 StateStart/StateUpdate
        sceneController.StateUpdate();
    }

    // ── 场景流转事件注册 ─────────────────────────────────────────────────

    private void RegisterSceneEvents()
    {
        EventCenter.Instance.AddEventListener(E_EventType.Scene_ToLevelSelect,
            () => sceneController.SetState(new SelectSecene(sceneController)));

        EventCenter.Instance.AddEventListener(E_EventType.Scene_ToGamePlay,
            () => sceneController.SetState(new GameScene(sceneController)));

        EventCenter.Instance.AddEventListener(E_EventType.Scene_ToShop,
            () => sceneController.SetState(new ShopScene(sceneController)));

        EventCenter.Instance.AddEventListener(E_EventType.Scene_ToMainMenu, OnReturnToMainMenu);
    }

    private void OnReturnToMainMenu()
    {
        ResetGameState();
        UIMgr.Instance.DestroyDic(); // 清理跨场景 UI 引用
        PoolMgr.Instance.ClearPool(); // 清理对象池
        sceneController.SetState(new StartScene(sceneController));
    }

    // ── 游戏状态 ─────────────────────────────────────────────────────────

    /// <summary>开始新局时重置所有运行时状态（回主菜单 / 重新开始时调用）。</summary>
    public void ResetGameState()
    {
        isDead = false;
        currentWave = 1;
        waveCount = 1;
        waveTimer = 0f;
        hp = 0;
        money = 0;
        exp = 0;
        propData = new PropData();
        currentProps = new List<PropData>();
        currentWeapons = new List<WeaponData>();
        currentRoleData = null;
        currentDifficulty = null;
    }

    private void InitUnlockRole()
    {
        if (!PlayerPrefs.HasKey("多面手"))
            PlayerPrefs.SetInt("多面手", 0);
        if (!PlayerPrefs.HasKey("公牛"))
            PlayerPrefs.SetInt("公牛", 0);
    }

    /// <summary>选完角色后根据角色特性初始化属性，每局只调用一次。</summary>
    public void InitProp()
    {
        if (currentRoleData == null) return;

        switch (currentRoleData.name)
        {
            case "全能者":
                propData.maxHp += 5;
                propData.speedPer += 0.05f;
                propData.harvest += 8;
                break;
            case "斗士":
                propData.short_attackSpeed += 0.5f;
                propData.long_range -= 0.5f;
                propData.short_range -= 0.5f;
                propData.long_damage -= 0.5f;
                break;
            case "医生":
                propData.revive += 5f;
                propData.short_attackSpeed -= 0.5f;
                propData.long_attackSpeed -= 0.5f;
                break;
            case "公牛":
                propData.maxHp += 20f;
                propData.revive += 15f;
                propData.slot = 0;
                break;
            case "多面手":
                propData.long_damage += 0.2f;
                propData.short_damage += 0.2f;
                propData.slot = 12;
                break;
        }

        hp = propData.maxHp;
        money = 100;
        exp = 0;
    }

    public object RandomOne<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return null;
        var random = new System.Random();
        return list[random.Next(0, list.Count)];
    }

    /// <summary>购买道具后融合属性（累加式，不覆盖）。</summary>
    public void FusionAttr(PropData shopProp)
    {
        propData.maxHp += shopProp.maxHp;
        propData.revive += shopProp.revive;
        if (propData.revive < 0) propData.revive = 0;
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
