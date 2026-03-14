# 土豆英雄 (TudouHero) — 代码库详细阅读报告

> **项目类型**：Unity 2D Roguelite / 生存射击游戏  
> **脚本语言**：C#  
> **主要目录**：`Scripts/`

---

## 目录

1. [高层架构与关键模块](#1-高层架构与关键模块)
2. [重要入口点与运行时流程](#2-重要入口点与运行时流程)
3. [外部依赖与第三方包](#3-外部依赖与第三方包)
4. [编码规范与设计模式](#4-编码规范与设计模式)
5. [潜在风险与技术债务](#5-潜在风险与技术债务)
6. [扩展建议](#6-扩展建议)
7. [外观模式 vs 中介者模式 — 详细对比与实战指南](#7-外观模式-vs-中介者模式--详细对比与实战指南)

---

## 1. 高层架构与关键模块

```
Scripts/
├── Framework/          ← 游戏框架层（单例、事件、对象池、资源、UI、音效、PureMVC）
│   ├── BaseMgr.cs          ← 非 Mono 线程安全泛型单例基类
│   ├── BaseMgrMono.cs      ← MonoBehaviour 泛型单例基类
│   ├── GameManager.cs      ← 中央数据枢纽（角色/武器/难度/波次/玩家属性）
│   ├── EventCenter.cs      ← 观察者/事件总线（泛型 + 无参两种监听）
│   ├── MonoMgr.cs          ← 为非 Mono 类提供 Update/FixedUpdate/LateUpdate
│   ├── PoolMgr.cs          ← GameObject 与纯 C# 对象双池
│   ├── ResourceMgr.cs      ← Resources 同步/异步加载封装
│   ├── UIMgr.cs            ← UI 面板生命周期管理（Show/Hide/Get）
│   ├── E_EventType.cs      ← 事件枚举（Audio_PlaySfx / BeginScene 等）
│   ├── Audio/
│   │   ├── AudioMgr.cs         ← 音效系统（BGM/SFX/UI 三通道 + 声音限制 + 对象池）
│   │   ├── AudioRegistry.cs    ← ScriptableObject 音频注册表
│   │   ├── AudioEvent.cs       ← 音频事件结构体与 AudioCategory 枚举
│   │   └── AudioSourcePooled.cs← 可复用的 AudioSource 组件
│   └── PureMVC/            ← 完整 PureMVC C# Standard（已引入，尚未全部接入业务）
│       ├── Core/           ← Controller / Model / View
│       ├── Interfaces/     ← IFacade / IMediator / IProxy / ICommand 等
│       └── Patterns/       ← Facade / Mediator / Proxy / Command / Observer
│
├── SceneState/         ← 场景状态机（State Pattern）
│   ├── ISceneState.cs          ← 状态基类（SceneName + StateStart/Update/End）
│   ├── SceneStateController.cs ← 状态控制器（SetState + StateUpdate + 异步场景加载）
│   ├── StartScene.cs           ← 状态：主菜单（01-MainMenu）
│   ├── SelectSecene.cs         ← 状态：选择界面（02-LevelSelect）
│   ├── GameScene.cs            ← 状态：游戏关卡（03-GamePlay）
│   └── ShopScene.cs            ← 状态：商店（04-Shop）
│
├── Player/
│   └── Player.cs           ← 玩家控制器（移动/受伤/死亡/拾取金币/生命再生）
│
├── Enemy/
│   ├── EnemyBase.cs        ← 敌人基类（移动/攻击/受伤/死亡/技能框架）
│   └── Enemy1~5.cs         ← 具体敌人类型（目前多数为空，继承 EnemyBase）
│
├── Weapon/
│   ├── WeaponBase.cs       ← 武器基类（自动瞄准/冷却/暴击计算）
│   ├── WeaponShort.cs      ← 近战武器（碰撞器触发攻击）
│   ├── WeaponLong.cs       ← 远程武器基类（生成子弹）
│   ├── WeaponSlot.cs       ← 商店武器槽（左键合成 / 右键出售）
│   ├── FistWeapon.cs       ← 拳头
│   ├── PistolWeapon.cs     ← 手枪
│   ├── CrossbowWeapon.cs   ← 弩
│   ├── BranchWeapon.cs     ← 树枝
│   └── MedicalGunWeapon.cs ← 医疗枪
│
├── Bullet/
│   ├── Bullet.cs           ← 子弹基类（速度/伤害/方向/碰撞销毁）
│   ├── ArrowBullet.cs      ← 箭矢子弹
│   ├── PistolBullet.cs     ← 手枪子弹
│   ├── MedicalBullet.cs    ← 医疗子弹
│   └── EnemyBullet.cs      ← 敌人子弹
│
├── Control/
│   └── LevelControl.cs     ← 关卡控制器（波次计时/敌人生成/胜负判定/场景跳转）
│
├── UI/
│   ├── BasePanel.cs        ← UI 面板基类（CanvasGroup 透明度控制 Show/Hide）
│   ├── BeginScenePanel.cs  ← 主菜单面板（DOTween 背景动画 + 场景跳转）
│   ├── GamePanel/
│   │   ├── GamePanel.cs    ← 游戏 HUD（血条/经验条/金币/倒计时/波次）
│   │   ├── ShopPanel.cs    ← 商店面板（道具/武器购买、属性展示、刷新）
│   │   ├── ItemCardUI.cs   ← 商品卡片 UI
│   │   ├── FailPanel.cs    ← 失败面板
│   │   └── SuccessPanel.cs ← 成功面板
│   └── SelectPanel/
│       ├── RoleSelectPanel.cs       ← 角色选择面板
│       ├── WeaponSelectPanel.cs     ← 武器选择面板
│       ├── DifficultySelectPanel.cs ← 难度选择面板
│       └── RoleUI / WeaponUI / DifficultyUI / RondomXxxUI.cs ← 列表项 UI
│
├── Data/               ← 纯数据类（[Serializable]，对应 JSON）
│   ├── ItemData.cs         ← 商品基类（id/name/price/avatar/describe）
│   ├── RoleData.cs         ← 角色数据
│   ├── WeaponData.cs       ← 武器数据（继承 ItemData）
│   ├── PropData.cs         ← 道具/属性数据（继承 ItemData）
│   ├── EnemyData.cs        ← 敌人数据（含 Clone() 深拷贝）
│   ├── LevelData.cs        ← 关卡数据（含 WaveData 波次信息）
│   └── DifficultyData.cs   ← 难度数据
│
├── Model/
│   └── PlayerModel.cs      ← 玩家模型（未完成，与 GameManager 中的字段重叠）
│
├── StartScene/
│   └── Hover.cs (UIHoverScaleTween) ← 鼠标悬停缩放动画（DOTween）
│
└── Number.cs           ← 浮动伤害数字（1 秒后自动销毁）
```

---

## 2. 重要入口点与运行时流程

```
启动
 │
 ▼
[01-MainMenu 场景]
 GameManager.Awake()           — 加载所有 JSON 数据 (enemy/role/weapon/prop)
                               — 加载音效预制体
                               — DontDestroyOnLoad 持久化
 BeginScenePanel.Start()       — 绑定按钮事件，播放 DOTween 背景动画
 点击"开始"
 │
 ▼
[02-LevelSelect 场景]
 RoleSelectPanel / WeaponSelectPanel / DifficultySelectPanel
 — 从 GameManager.roleDatas 渲染角色列表
 — 点击确认，将选择写入 GameManager.currentRoleData / currentWeapons / currentDifficulty
 │
 ▼
[03-GamePlay 场景]
 LevelControl.Awake()          — 读取关卡 JSON → levelDatas
 Player.Awake()                — 初始化角色外观、WeaponsPos
 Player.Start() / Awake()      — GameManager.InitProp() 初始化属性（仅第一波）
 LevelControl.Start()          — GenerateEnemy()（协程延时生成）
                               — GenerateWeapon()（实例化武器挂载到 weaponsPos 下）
 每帧 Update()
   Player.Update()             — Move() / Revive() / EatMoney()
   WeaponBase.Update()         — Aiming() → Fire() → 子弹/碰撞伤害
   EnemyBase.Update()          — Move() / Attack() / UpdateSkill()
   LevelControl.Update()       — waveTimer 倒计时
                                 ↓ 时间到
                               NextWave() → 加金币 → 跳转 [04-Shop]
                                 ↓ 全部波次完成
                               GoodGame() → 显示成功面板 → 3秒后返回 [01-MainMenu]
   Player 死亡 → Injured() → Dead() → BadGame() → 显示失败面板 → 返回主菜单
 │
 ▼
[04-Shop 场景]
 ShopPanel.Start()             — 显示属性/道具/武器 UI
                               — RandomProps() 随机 2-4 道具 + 武器
 玩家购买道具                  — FusionAttr() 融合属性
 玩家购买武器                  — 加入 currentWeapons
 点击"出发"                    — 返回 [03-GamePlay]（波次 +1）
```

---

## 3. 外部依赖与第三方包

| 包 | 用途 | 关键文件 |
|---|---|---|
| **Newtonsoft.Json (Json.NET)** | 读取 Resources/Data/*.json 关卡/角色/武器/敌人数据 | `GameManager.cs`, `LevelControl.cs`, `ShopPanel.cs` |
| **DOTween** | UI 动画（背景平移循环、按钮悬停缩放） | `BeginScenePanel.cs`, `Hover.cs` |
| **TextMeshPro** | 游戏内所有文字显示 | `GamePanel.cs`, `ShopPanel.cs` 等 |
| **Unity.VisualScripting** | `AddComponent<>` 扩展方法 | `BaseMgrMono.cs` |
| **Unity SpriteAtlas** | 道具图标图集 | `GameManager.cs` |
| **PureMVC C# Standard** | MVC 框架（已集成，业务层尚未全部接入） | `Scripts/Framework/PureMVC/` |

**JSON 数据文件**（存放于 `Resources/Data/`）：
- `enemy.json` — 敌人数据
- `role.json` — 角色数据
- `weapon.json` — 武器数据
- `prop.json` — 道具数据
- `{difficulty.levelName}.json` — 关卡数据（每个难度对应一个文件）

---

## 4. 编码规范与设计模式

### 4.1 使用的设计模式

| 模式 | 实现位置 | 说明 |
|---|---|---|
| **单例模式** | `BaseMgr<T>`, `BaseMgrMono<T>` | 全局唯一访问；双重检查锁（非 Mono）；Mono 版用 Awake 保护 |
| **状态模式** | `SceneStateController` + `ISceneState` 子类 | 场景切换流程，解耦场景逻辑 |
| **观察者/事件总线** | `EventCenter` | 泛型 + 无参两路，基于 `UnityAction`，通过 `E_EventType` 枚举索引 |
| **对象池** | `PoolMgr` | 支持 GameObject 池（Stack + usedList 上限复用）和纯 C# 对象池（Queue） |
| **模板方法** | `WeaponBase.Fire()` | 基类定义流程，`WeaponShort` / `WeaponLong` 各自重写 `Fire()` / `GenerateBullet()` |
| **继承层次** | `EnemyBase→Enemy1-5`，`WeaponBase→WeaponShort/Long→具体武器` | 多态扩展 |
| **外观模式** | `GameManager`（非正式 Facade）、`PureMVC/Facade.cs` | GameManager 相当于整个游戏系统的入口门面 |
| **中介者模式** | `PureMVC/Mediator.cs`（框架层），`ShopPanel`/`LevelControl`（非正式中介） | ShopPanel 协调 UI + GameManager + WeaponSlot |
| **代理模式** | `PureMVC/Proxy.cs` | 框架已提供，业务层尚未使用 |
| **命令模式** | `PureMVC/SimpleCommand`, `MacroCommand` | 框架已提供，业务层尚未使用 |
| **数据深拷贝** | `EnemyData.Clone()`, `JsonConvert.SerializeObject/Deserialize` | 防止共享引用导致状态污染 |

### 4.2 编码惯例

- **中文注释** — 所有业务注释均为中文
- **TODO 注释** — 标记待完成功能（优化/事件系统接入/音效/动画等）
- **泛型单例** — 凡"全局管理器"皆继承 `BaseMgr<T>` 或 `BaseMgrMono<T>`
- **`[SerializeField]`** — Inspector 可见但外部私有
- **`[Serializable]`** — 所有 Data 类标注，配合 JSON 反序列化
- **命名风格** — 字段小驼峰（`_btnStart`, `waveTimer`），类名大驼峰

---

## 5. 潜在风险与技术债务

| 序号 | 问题 | 风险等级 | 建议 |
|---|---|---|---|
| 1 | **`GameManager` 是上帝类** — 同时持有角色数据、武器列表、玩家属性、波次状态、所有预制体引用 | 🔴 高 | 拆分为 `PlayerDataModel`、`WaveModel`、`ItemModel`；或用 PureMVC Proxy |
| 2 | **`MonoMgr.StartCoroutine()` 抛 `NotImplementedException`** — `ResourceMgr` 的异步加载依赖它，当前会崩溃 | 🔴 高 | 让 `MonoMgr` 继承 `BaseMgrMono` 或通过一个 `MonoBehaviour` 委托运行协程 |
| 3 | **`LevelControl.SwawnEnemies()` 中 `SetElite()` 被调用两次** — 精英敌人血量和伤害变为原来 4 倍 | 🔴 高 | 删除第二次调用 |
| 4 | **敌人数据在 `SwawnEnemies` 中直接赋值覆盖 `Init()` 的深拷贝结果** — 导致多个敌人共享同一个 `EnemyData` 引用 | 🔴 高 | 统一使用 `enemy.Init(e)` 方式，删除直接赋值 `enemy.enemyData = e` |
| 5 | **`Factory.cs` 为空占位** — 没有实际功能 | 🟡 中 | 实现为武器/敌人工厂，替代 LevelControl 中的 `Instantiate` + 字典查找 |
| 6 | **`PlayerModel.cs` 与 `GameManager` 字段重复** — 两套玩家属性并存，实际未使用 `PlayerModel` | 🟡 中 | 整合到 `GameManager` 或迁移到 PureMVC Proxy 统一管理 |
| 7 | **PureMVC 框架引入但几乎未使用** — 与自定义 `BaseMgr` 单例并存，架构混乱 | 🟡 中 | 选定一种架构：要么全面接入 PureMVC，要么移除 |
| 8 | **`SceneState` 所有状态体为空** — `StateStart/Update/End` 无实际逻辑 | 🟡 中 | 将 `BeginScenePanel` 初始化、`UIMgr.ShowPanel` 等逻辑迁移进去 |
| 9 | **UI `SetAttrUI()` 使用 `GetChild(index)` 硬编码索引** — 调整 Inspector 层级会出 bug | 🟡 中 | 改用 `[SerializeField]` 直接引用或 `Find` 命名查找 |
| 10 | **`EventCenter.Claer()` 方法名拼写错误** — 应为 `Clear` | 🟢 低 | 重命名为 `Clear(E_EventType)` |
| 11 | **音效使用 `Instantiate` 创建音频对象** — 未使用已实现的 `AudioMgr`/`PoolMgr` | 🟡 中 | 用 `EventCenter.EventTrigger(E_EventType.Audio_PlaySfx, req)` 替代 |
| 12 | **`Revive()` 中 `reviveTimer = 0` 在 `if` 块外** — 每帧都重置，生命再生永远只触发一次 | 🔴 高 | 将 `reviveTimer = 0` 移入 `if (reviveTimer >= 1f)` 块内 |

---

## 6. 扩展建议

### 6.1 安全扩展新角色
1. 在 `Resources/Data/role.json` 添加条目
2. 在 `GameManager.InitProp()` 添加角色特定属性初始化 case
3. 添加解锁条件判断（仿照 `Player.Start()` 中的"公牛"逻辑）

### 6.2 安全扩展新武器
1. 继承 `WeaponShort`（近战）或 `WeaponLong`（远程），重写 `Fire()` / `GenerateBullet()`
2. 在 `Resources/Data/weapon.json` 添加 JSON 条目
3. 创建预制体挂载新脚本，放入 `Resources/Prefabs/武器/`

### 6.3 安全扩展新敌人
1. 继承 `EnemyBase`，重写 `LaunchSkill(Vector2 dir)` 实现技能
2. 在 `Resources/Data/enemy.json` 添加条目（注意 `skillTime = -1` 表示无技能）
3. 在 `LevelControl` 中添加预制体引用，在 `enemyPrefabDic` 中注册

### 6.4 添加新场景/流程
1. 继承 `ISceneState`，实现 `StateStart/Update/End`
2. 在 `SceneStateController.SetState()` 调用新状态

### 6.5 接入 EventCenter 解耦
```csharp
// 替代直接调用 GamePanel.Instance.RenewHp()
EventCenter.Instance.EventTrigger(E_EventType.Player_HpChanged);
// 在 GamePanel 中监听
EventCenter.Instance.AddEventListener(E_EventType.Player_HpChanged, RenewHp);
```

---

## 7. 外观模式 vs 中介者模式 — 详细对比与实战指南

> 本仓库同时包含这两种模式的代码（`PureMVC/Patterns/Facade/Facade.cs`、`PureMVC/Patterns/Mediator/Mediator.cs`），下面结合项目实际讲解。

---

### 7.1 一句话区别

| | 外观模式 (Facade) | 中介者模式 (Mediator) |
|---|---|---|
| **核心目的** | 为复杂子系统提供**简洁的统一入口** | 让多个对象**不直接互相引用**，通过中介协调 |
| **通信方向** | 外部 → 外观 → 内部子系统（**单向简化**） | 对象 A ↔ 中介者 ↔ 对象 B（**双向协调**） |
| **子系统关系** | 子系统**不知道外观的存在** | 各对象**知道中介者**，持有它的引用 |
| **解决问题** | 接口太复杂，调用方要知道太多细节 | 对象之间耦合太多，网状依赖 |
| **类比** | 前台/接待（你只和前台说，前台协调内部） | 指挥官（士兵都向指挥官汇报，指挥官分配任务） |

---

### 7.2 当前项目中的体现

#### GameManager 是一个"非正式外观"
```csharp
// 当前：外部各处直接调用 GameManager 的字段
// Player.cs:
GameManager.Instance.hp -= attack;
GameManager.Instance.isDead = true;

// LevelControl.cs:
GameManager.Instance.money += GameManager.Instance.propData.harvest;
GameManager.Instance.currentWave += 1;

// WeaponBase.cs:
data.damage *= GameManager.Instance.propData.short_damage * data.grade;
```
**问题**：GameManager 已经成为上帝类，它不是在"隐藏"子系统，而是直接存储了所有数据。

#### ShopPanel 是一个"非正式中介者"
```csharp
// ShopPanel 协调：UI 按钮 → 扣钱 → 更新 currentWeapons → 刷新 UI
public bool Shopping(ItemData itemData)
{
    GameManager.Instance.money -= itemData.price;        // 数据层
    _moneyText.text = GameManager.Instance.money.ToString(); // UI 层
    GameManager.Instance.currentWeapons.Add(tempItem);  // 数据层
    ShowCurrentWeapon();                                  // UI 层
    GameManager.Instance.FusionAttr(tempItem);           // 属性层
    SetAttrUI();                                          // UI 层
}
```
**问题**：ShopPanel 既是 UI 又是业务中介，职责混杂。

---

### 7.3 标准外观模式示例（结合本项目）

外观模式的作用：**把复杂的初始化/跳转逻辑藏起来，对外只暴露简单方法**。

```csharp
// Scripts/Framework/GameFacade.cs
// 外观：对外隐藏"启动游戏"的复杂步骤
public class GameFacade
{
    // 子系统（外观持有它们，但它们不知道外观）
    private static GameManager _gm => GameManager.Instance;
    private static AudioMgr _audio => AudioMgr.Instance;
    private static UIMgr _ui => UIMgr.Instance;

    // 简单接口1：开始一局游戏（内部协调多个子系统）
    public static void StartGame(RoleData role, List<WeaponData> weapons, DifficultyData difficulty)
    {
        // 内部复杂逻辑对调用者透明
        _gm.currentRoleData = role;
        _gm.currentWeapons = new List<WeaponData>(weapons);
        _gm.currentDifficulty = difficulty;
        _gm.currentWave = 1;
        _gm.waveCount = 1;
        _gm.isDead = false;
        _gm.InitProp();
        
        // 停止菜单音乐，播放游戏音乐
        EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
        EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, 
            new AudioBgmRequest { key = "bgm_game", loop = true });
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("03-GamePlay");
    }

    // 简单接口2：结算游戏
    public static void EndGame(bool success)
    {
        _gm.isDead = !success;
        if (success)
            _ui.ShowPanel<SuccessPanel>();
        else
            _ui.ShowPanel<FailPanel>();
    }

    // 简单接口3：重置所有状态回到主菜单
    public static void ReturnToMainMenu()
    {
        EventCenter.Instance.Clear();  // 清除所有事件监听
        PoolMgr.Instance.ClearPool();  // 清空对象池
        _ui.DestroyDic();              // 清空 UI 字典
        UnityEngine.SceneManagement.SceneManager.LoadScene("01-MainMenu");
    }
}

// 调用方（非常简单，不需要知道内部细节）:
// GameFacade.StartGame(selectedRole, selectedWeapons, selectedDifficulty);
// GameFacade.EndGame(true);
```

**外观模式的关键特征**：
- 子系统（`GameManager`, `AudioMgr`, `UIMgr`）不知道 `GameFacade` 的存在
- 对调用方隐藏复杂性
- 通常是**单向**的：外部 → 外观 → 子系统

---

### 7.4 标准中介者模式示例（结合本项目）

中介者模式的作用：**UI 面板之间不直接互相引用，全部通过中介协调**。

```csharp
// Scripts/UI/UISceneMediator.cs
// 中介者：协调商店场景中多个 UI 面板之间的交互
public class UISceneMediator : BaseMgrMono<UISceneMediator>
{
    // 中介者"知道"所有参与者
    [SerializeField] private ShopPanel _shopPanel;
    [SerializeField] private WeaponSlot[] _weaponSlots;

    // 协调：武器槽右键出售 → 中介者处理 → 通知金币 UI 和武器列表 UI 更新
    public void OnWeaponSold(WeaponData soldWeapon, int slotIndex)
    {
        // 1. 修改数据
        GameManager.Instance.money += (int)(soldWeapon.price * 0.5f);
        GameManager.Instance.currentWeapons.RemoveAt(slotIndex);

        // 2. 通知所有相关 UI 更新（它们互相不认识）
        _shopPanel.UpdateMoneyUI();
        _shopPanel.ShowCurrentWeapon();
        
        // 若将来有"出售历史"面板，也在这里通知，不改 WeaponSlot
        // _saleHistoryPanel.AddRecord(soldWeapon);
    }

    // 协调：购买道具 → 中介者处理 → 通知属性面板 + 道具栏 + 金币 UI
    public void OnPropBought(PropData prop)
    {
        GameManager.Instance.money -= prop.price;
        GameManager.Instance.FusionAttr(prop);
        GameManager.Instance.currentProps.Add(prop);

        _shopPanel.UpdateMoneyUI();
        _shopPanel.UpdateAttrUI();
        _shopPanel.ShowCurrentProp();
    }
}

// WeaponSlot 只需要知道中介者，不需要知道 ShopPanel
public class WeaponSlot_Refactored : MonoBehaviour, IPointerClickHandler
{
    public UISceneMediator mediator;  // 只依赖中介者

    private void OnRightClick()
    {
        if (weaponData == null) return;
        mediator.OnWeaponSold(weaponData, slotCount);  // 告诉中介者发生了什么
        weaponData = null;
        _weaponIcon.enabled = false;
    }
}
```

**中介者模式的关键特征**：
- `WeaponSlot` 不直接调用 `ShopPanel`（当前代码中有直接调用 `ShopPanel.Instance.ShowCurrentWeapon()`）
- 各对象"知道中介者"，通过中介者和其他对象通信
- 通常是**双向**的：A通知中介者 → 中介者通知B

---

### 7.5 外观 vs 中介者 核心区别总结

```
外观模式（Facade）:
┌─────────────┐        ┌─────────────┐
│  调用方      │──────▶│   外观      │──▶ GameManager
│ (Player.cs) │        │ GameFacade  │──▶ AudioMgr
└─────────────┘        └─────────────┘──▶ UIMgr
                        ↑ 调用方只看外观，外观协调子系统
                        ↑ 子系统不知道外观的存在

中介者模式（Mediator）:
┌──────────┐    通知    ┌─────────────┐    通知    ┌──────────┐
│WeaponSlot│──────────▶│UISceneMediator│──────────▶│ShopPanel │
└──────────┘           └─────────────┘            └──────────┘
                        ↑ 各对象持有中介者引用
                        ↑ 对象间不直接互相引用
```

| 场景 | 用外观 | 用中介者 |
|---|---|---|
| 子系统 API 很复杂，调用方只需简单操作 | ✅ | ❌ |
| 多个 UI 面板/对象需要协调交互 | ❌ | ✅ |
| 隐藏初始化/清理的复杂步骤 | ✅ | ❌ |
| 减少对象间的直接引用（网状依赖） | ❌ | ✅ |
| 系统入口/API 设计 | ✅ | ❌ |
| 游戏流程控制（战斗/UI切换） | ❌ | ✅ |

### 7.6 本项目的 PureMVC 中的 Facade 和 Mediator

本项目引入的 PureMVC 框架中，两者有明确分工：
- **`Facade`** (`Scripts/Framework/PureMVC/Patterns/Facade/Facade.cs`)：
  - 作为整个 MVC 的**统一入口**
  - 注册 Command、Mediator、Proxy
  - 发送 Notification（通知）
  - 类比：`GameFacade.StartGame()` 这种统一入口
  
- **`Mediator`** (`Scripts/Framework/PureMVC/Patterns/Mediator/Mediator.cs`)：
  - 包裹具体的 **View 组件**（UI 面板）
  - 监听感兴趣的 Notification
  - 响应通知更新 UI
  - 类比：`ShopPanel` 协调商店内所有交互

```
PureMVC 中的分工:
Facade（外观）
  ├── 注册 → Proxy（代理，管理数据/Model）
  ├── 注册 → Mediator（中介者，管理 UI/View）
  └── 注册 → Command（命令，处理业务逻辑）

通知流:
View（UI点击）→ Mediator → sendNotification → Facade → Command（执行逻辑）→ Proxy（更新数据）→ Facade → Mediator（更新 UI）
```

---

## 最重要的文件速查

| 要了解... | 看这里 |
|---|---|
| 全局数据入口 | `Scripts/Framework/GameManager.cs` |
| 事件系统 | `Scripts/Framework/EventCenter.cs` + `E_EventType.cs` |
| 场景切换流程 | `Scripts/SceneState/SceneStateController.cs` |
| 关卡/波次逻辑 | `Scripts/Control/LevelControl.cs` |
| 玩家控制 | `Scripts/Player/Player.cs` |
| 武器系统 | `Scripts/Weapon/WeaponBase.cs` |
| 商店系统 | `Scripts/UI/GamePanel/ShopPanel.cs` |
| 音效系统 | `Scripts/Framework/Audio/AudioMgr.cs` |
| 对象池 | `Scripts/Framework/PoolMgr.cs` |
| PureMVC 外观 | `Scripts/Framework/PureMVC/Patterns/Facade/Facade.cs` |
| PureMVC 中介者 | `Scripts/Framework/PureMVC/Patterns/Mediator/Mediator.cs` |
| 数据结构 | `Scripts/Data/*.cs` |