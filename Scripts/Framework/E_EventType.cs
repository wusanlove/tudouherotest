public enum E_EventType
{
    // ── 主菜单 ──────────────────────────────────────────────────────────────
    /// <summary>BeginScenePanel 的"开始"按钮点击，通知状态机/UIFlowController 跳转选择场景。</summary>
    UI_StartGame,
    /// <summary>打开设置面板（预留）。</summary>
    UI_OpenSettings,

    // ── 选择场景 ─────────────────────────────────────────────────────────────
    /// <summary>玩家确认角色选择（body: RoleData）。</summary>
    UI_SelectRole,
    /// <summary>玩家确认武器选择（body: WeaponData）。</summary>
    UI_SelectWeapon,
    /// <summary>玩家确认难度选择（body: DifficultyData）。</summary>
    UI_SelectDifficulty,
    /// <summary>玩家点击"确认出发"，所有选择完成，进入游戏场景。</summary>
    UI_ConfirmSelection,

    // ── 游戏 HUD ─────────────────────────────────────────────────────────────
    /// <summary>请求刷新金币显示。</summary>
    HUD_MoneyChanged,
    /// <summary>请求刷新血量显示。</summary>
    HUD_HpChanged,
    /// <summary>请求刷新经验显示。</summary>
    HUD_ExpChanged,

    // ── 商店 ─────────────────────────────────────────────────────────────────
    /// <summary>玩家点击购买按钮（body: ItemData）。</summary>
    Shop_RequestBuy,
    /// <summary>购买结果回调（body: ShopBuyResult）。</summary>
    Shop_BuyResult,
    /// <summary>玩家点击刷新商店按钮。</summary>
    Shop_RequestRefresh,
    /// <summary>刷新结果回调（body: bool）。</summary>
    Shop_RefreshResult,
    /// <summary>玩家点击"下一波"，离开商店进入下一关游戏场景。</summary>
    Shop_ProceedToNextWave,

    // ── 战斗 ─────────────────────────────────────────────────────────────────
    /// <summary>当前波次开始（body: int waveIndex）。</summary>
    Battle_WaveStarted,
    /// <summary>当前波次时间到，即将跳转商店（body: int waveIndex）。</summary>
    Battle_WaveCompleted,
    /// <summary>玩家死亡。</summary>
    Battle_PlayerDied,
    /// <summary>所有波次通关。</summary>
    Battle_AllWavesCompleted,
    /// <summary>某个敌人被消灭（body: EnemyBase）。</summary>
    Battle_EnemyDied,
    /// <summary>金币被拾取（body: float amount）。</summary>
    Battle_MoneyPickedUp,

    // ── 音效（保持原始命名以兼容 AudioMgr）──────────────────────────────────
    Audio_PlaySfx,
    Audio_PlayBgm,
    Audio_StopBgm,
    Audio_SetVolume,

    // ── 旧事件（保留兼容，后续可清理）──────────────────────────────────────
    BeginScene_btn_start,
    BeginScene_btn_exitSetting,
}
