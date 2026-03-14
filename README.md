# tudouherotest

## 项目简介

2D Roguelite 小游戏（Unity），采用 **MVC + 状态机 + 服务分层** 架构。

## 游戏流程

```
主菜单 (01-MainMenu)
  → 选择场景 (02-LevelSelect)：角色 / 武器 / 难度选择
  → 游戏场景 (03-GamePlay)：多波次战斗
  → 商店场景 (04-Shop)：购买道具升级（每波结束后）
  → 回到游戏场景 / 结算后返回主菜单
```

## 架构概览

详见 [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)，包含：

- 模块关系图（Mermaid）
- 状态流转图
- 事件清单
- 关键设计决策

## 核心模块

| 模块 | 文件 | 说明 |
|------|------|------|
| 状态机 | `Scripts/Framework/GameStateMachine.cs` | 统一驱动场景切换 |
| 事件总线 | `Scripts/Framework/EventCenter.cs` | 强类型枚举事件，解耦各层 |
| 事件枚举 | `Scripts/Framework/E_EventType.cs` | 所有游戏事件定义 |
| UI 流程控制 | `Scripts/Framework/UIFlowController.cs` | 面板互斥、输入锁定 |
| 音效服务 | `Scripts/Framework/Services/AudioService.cs` | AudioMgr 的语义化 Facade |
| 配置服务 | `Scripts/Framework/Services/ConfigService.cs` | JSON 统一加载 + 缓存 |
| 存档服务 | `Scripts/Framework/Services/SaveProgressService.cs` | PlayerPrefs 封装 |

## 开发说明

- **Unity 版本**：2022.x LTS 及以上
- **依赖**：Newtonsoft.Json、DOTween、TextMeshPro
- 所有场景跳转通过 `GameStateMachine.Instance.EnterXxx()` 发起，避免直接调用 `SceneManager.LoadScene`
- UI Panel 通过 `EventCenter` 订阅事件更新，不直接引用 Player/Enemy/LevelControl
- 解锁/存档逻辑统一通过 `SaveProgressService` 管理

## 快速上手

1. 打开 `01-MainMenu` 场景运行
2. `GameManager`（DontDestroyOnLoad）会自动初始化所有服务和状态机
3. 点击"开始"按钮进入角色/武器/难度选择
4. 选择完毕后开始游戏
