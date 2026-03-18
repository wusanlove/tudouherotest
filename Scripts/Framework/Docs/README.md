# TomatoHero2D Framework

> 该文档从外部测试工程迁移进来，用于记录当前项目的框架约定与模块边界。

## 目录建议

- Framework
  - BaseMgr / BaseMgrMono：管理器单例基类
  - EventCenter：事件中心（建议优先用事件解耦）
  - Audio：音频系统（建议通过 AudioMgr + EventCenter 触发）
  - ResourceMgr：资源加载（可扩展 Addressables）
  - PoolMgr：对象池
  - ConfigMgr：配置读取（JSON/表格）
  - PureMvc：MVC 相关

## 约定

- 跨模块通信：优先使用 `EventCenter` 派发/监听事件，避免直接引用彼此。
- 新增音效播放：不要再通过 `GameManager.attackMusic` 等旧字段播放，改为发送音频事件。

