/// <summary>
/// 全局事件类型枚举。
///
/// 约定：
///   - UI_Xxx     → Panel 抛出，UIFlowController 监听（不允许 Panel 直接切面板/切场景）。
///   - Flow_Xxx   → UIFlowController/游戏逻辑抛出，SceneFlowController 监听（唯一切场景入口）。
///   - Game_Xxx   → 游戏状态变化广播，UI/其他系统订阅更新自己（解耦直接方法调用）。
///   - Audio_Xxx  → AudioMgr 监听（已有实现）。
/// </summary>
public enum E_EventType
{
    #region UI 流程（UIFlowController 监听，Panel 只发这些事件）
    UI_OnStartGameClicked,      // 无参：主菜单点击"开始"
    UI_OnRoleSelected,          // RoleData：选择角色
    UI_OnWeaponSelected,        // WeaponData：选择武器
    UI_OnDifficultySelected,    // DifficultyData：选择难度
    UI_OnNextWaveClicked,       // 无参：商店点击"进入下一波"
    UI_OnShopRefreshClicked,    // 无参：商店点击"刷新"
    UI_OnShopBuyClicked,        // ItemData：购买道具/武器
    #endregion

    #region 场景流程（SceneFlowController 监听，禁止 UI 直接调 SceneManager）
    Flow_GoToMainMenu,          // 无参
    Flow_GoToLevelSelect,       // 无参
    Flow_GoToGamePlay,          // 无参
    Flow_GoToShop,              // 无参
    #endregion

    #region 游戏状态广播（UI 订阅来刷新自身，不再被直接调用）
    Game_PlayerHpChanged,       // float：当前 HP
    Game_PlayerMoneyChanged,    // float：当前金币
    Game_PlayerExpChanged,      // float：当前经验值
    Game_WaveTimerChanged,      // float：当前波次倒计时
    Game_WaveCompleted,         // int：完成的波次序号
    Game_PlayerDead,            // 无参
    Game_EnemyDead,             // EnemyData
    #endregion

    #region 主菜单界面（保留旧事件以兼容已有场景绑定）
    BeginScene_btn_start,
    BeginScene_btn_exitSetting,
    #endregion

    #region 音效（AudioMgr 监听，已有实现）
    Audio_PlaySfx,
    Audio_PlayBgm,
    Audio_StopBgm,
    Audio_SetVolume,
    #endregion
}
