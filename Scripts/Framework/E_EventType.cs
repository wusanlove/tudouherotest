public enum E_EventType
{
    // ── 主菜单 ──────────────────────────────────────────────────────────────
    BeginScene_btn_start,
    BeginScene_btn_exitSetting,

    // ── 场景切换请求（由UI触发，由GameManager监听后交给SceneStateController）──
    /// <summary>请求切换到角色/武器/难度选择场景</summary>
    Scene_GoToSelect,
    /// <summary>请求开始/继续游戏场景（首波或商店返回）</summary>
    Scene_GoToGame,
    /// <summary>请求进入商店场景（每波结束后）</summary>
    Scene_GoToShop,
    /// <summary>请求返回主菜单（游戏结束后）</summary>
    Scene_GoToMenu,

    // ── 选择流程（在02-LevelSelect场景内部通信）─────────────────────────────
    /// <summary>玩家点击了某个角色卡，payload: RoleData</summary>
    Select_RoleChosen,
    /// <summary>玩家点击了某个武器卡，payload: WeaponData</summary>
    Select_WeaponChosen,

    // ── 玩家状态变化（让HUD和商店UI保持同步）──────────────────────────────
    /// <summary>玩家HP发生变化，payload: float（当前HP）</summary>
    Game_PlayerHpChanged,
    /// <summary>玩家金币发生变化，payload: float</summary>
    Game_MoneyChanged,
    /// <summary>玩家经验发生变化，payload: float</summary>
    Game_ExpChanged,

    // ── 音效 ────────────────────────────────────────────────────────────────
    Audio_PlaySfx,
    Audio_PlayBgm,
    Audio_StopBgm,
    Audio_SetVolume,
}
