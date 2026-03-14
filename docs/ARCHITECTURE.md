# 架构文档 · TudouHeroTest

## 概览

```
Assets/Scripts/
├── Framework/          # 全局服务单例（跨场景持久）
│   ├── GameManager     # 游戏状态 + 场景流程驱动
│   ├── ConfigMgr       # JSON 配置统一加载入口
│   ├── EventCenter     # 跨系统事件广播总线
│   ├── UIMgr           # UI 面板生命周期管理
│   ├── AudioMgr        # 音效服务（通过 EventCenter 触发）
│   ├── PoolMgr         # 对象池
│   ├── ResMgr          # Resources 异步加载封装
│   ├── MonoMgr         # 非 MonoBehaviour 管理器的协程/Update 代理
│   └── BaseMgr / BaseMgrMono  # 单例基类
├── SceneState/         # 场景状态机
│   ├── ISceneState
│   ├── SceneStateController
│   ├── StartScene      → 01-MainMenu
│   ├── SelectSecene    → 02-LevelSelect
│   ├── GameScene       → 03-GamePlay
│   └── ShopScene       → 04-Shop
├── UI/
│   ├── BasePanel
│   ├── BeginScenePanel
│   ├── SelectPanel/    # RoleSelectPanel / WeaponSelectPanel / DifficultySelectPanel
│   └── GamePanel/      # GamePanel / ShopPanel
├── Player/             # Player（移动/受伤/死亡）
├── Enemy/              # EnemyBase 及派生类
├── Weapon/             # WeaponBase 及派生类
├── Control/            # LevelControl（波次/胜负判定）
├── Data/               # 数据结构（RoleData / WeaponData / PropData …）
└── Model/              # PlayerModel（预留，待 DI 改造时替代 GameManager 的状态部分）
```

---

## 核心原则

| 层次 | 职责 | 不应做的事 |
|------|------|-----------|
| **EventCenter** | 跨系统广播（血量/金币变化、波次事件） | 驱动场景切换或 UI 打开/关闭 |
| **SceneStateController** | 异步加载场景，协调 StateStart/StateEnd | 持有业务数据 |
| **GameManager** | 持有运行时状态；监听 Scene_* 事件驱动状态机 | 直接调用 SceneManager.LoadScene |
| **UIPanel** | 显示数据；抛出按钮点击事件 | 直接引用其他 Panel 或调用场景跳转 |
| **ConfigMgr** | 统一 JSON 加载，返回独立副本 | 持有可变游戏状态 |

---

## 场景流转状态图

```mermaid
stateDiagram-v2
    [*] --> MainMenu : 启动（GameManager.Awake）

    MainMenu : 01-MainMenu\nBeginScenePanel
    LevelSelect : 02-LevelSelect\nRole/Weapon/Difficulty 选择
    GamePlay : 03-GamePlay\nLevelControl 驱动波次
    Shop : 04-Shop\nShopPanel 购买

    MainMenu --> LevelSelect : Scene_ToLevelSelect\n(btnStart 点击)
    LevelSelect --> GamePlay : Scene_ToGamePlay\n(确认选择)
    GamePlay --> Shop : Scene_ToShop\n(波次结束 & 非最终波)
    Shop --> GamePlay : Scene_ToGamePlay\n(出发按钮)
    GamePlay --> MainMenu : Scene_ToMainMenu\n(胜利/失败后 3s)
    Shop --> MainMenu : Scene_ToMainMenu\n(放弃)
```

---

## 事件流（以"玩家受伤"为例）

```mermaid
sequenceDiagram
    participant Enemy as EnemyBase
    participant Player
    participant GM as GameManager
    participant EC as EventCenter
    participant GP as GamePanel

    Enemy->>Player: Injured(damage)
    Player->>GM: hp -= damage
    Player->>EC: EventTrigger(Player_HpChanged, hp)
    EC-->>GP: RenewHp()
    Note over Player: hp <= 0 → Dead()
    Player->>EC: EventTrigger(Player_Dead)
    EC-->>LevelControl: BadGame()
    LevelControl->>EC: EventTrigger(Scene_ToMainMenu) [3s后]
    EC-->>GM: OnReturnToMainMenu() → ResetGameState() + SetState(StartScene)
```

---

## 单例服务关系

```mermaid
graph TD
    GM[GameManager\nBaseMgrMono] -->|驱动| SSC[SceneStateController]
    GM -->|读取配置| CM[ConfigMgr]
    GM -->|注册/触发| EC[EventCenter]
    LC[LevelControl] -->|触发| EC
    BP[BeginScenePanel] -->|触发| EC
    SP[ShopPanel] -->|触发| EC
    EC -->|回调| GM
    AM[AudioMgr] -->|订阅| EC
    UI[UIMgr] -.->|管理| Panels[BasePanel 子类]
    Pool[PoolMgr] -.->|复用| GO[GameObject]
    Mono[MonoMgr\nMonoBehaviour] -.->|协程/Update代理| AM
    Mono -.->|协程/Update代理| ResMgr
```

---

## DI 演进路线（备注）

```csharp
// 当前：静态单例直接访问
GameManager.Instance.money -= price;

// TODO 演进步骤：
// 1. 提取接口：IGameState, IConfigService, IEventBus
// 2. 用 ServiceLocator 注册（比全局单例好测试）：
//    ServiceLocator.Register<IGameState>(new GameState());
//    ServiceLocator.Get<IGameState>().money -= price;
// 3. 最终目标：构造函数注入（Zenject / VContainer），
//    彻底消除静态依赖，便于单元测试和热重载
```

---

## 配置数据路径约定

所有 JSON 配置存放在 `Assets/Resources/Data/`，通过 `ConfigMgr.LoadList<T>(key)` 加载：

| key | 类型 | 说明 |
|-----|------|------|
| `enemy` | `List<EnemyData>` | 全部敌人基础属性 |
| `role` | `List<RoleData>` | 可选角色（含解锁状态） |
| `weapon` | `List<WeaponData>` | 全部武器 |
| `prop` | `List<PropData>` | 全部道具 |
| `difficulty` | `List<DifficultyData>` | 难度列表 |
| `{difficulty.levelName}` | `List<LevelData>` | 对应难度的关卡波次数据 |
