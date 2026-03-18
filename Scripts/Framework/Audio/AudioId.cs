/// <summary>
/// 音效枚举 —— 所有音频的唯一标识。
/// AudioRegistry ScriptableObject 中的每个条目均对应此处一个值。
/// 其他系统触发音效时，只需传入此枚举，无需关心资源路径或字符串名称。
/// </summary>
public enum AudioId
{
    None = 0,

    // ── BGM ─────────────────────────────────────────────────────────────
    BGM_Menu,       // 主菜单背景音乐
    BGM_Game,       // 游戏关卡背景音乐
    BGM_Shop,       // 商店背景音乐

    // ── UI 音效 ──────────────────────────────────────────────────────────
    SFX_UI_Hover,   // 鼠标悬停
    SFX_UI_Click,   // 按钮点击
    SFX_UI_Open,    // 面板打开
    SFX_UI_Close,   // 面板关闭
    SFX_UI_Buy,     // 购买道具

    // ── 玩家/战斗音效 ────────────────────────────────────────────────────
    SFX_Attack,     // 近战攻击
    SFX_Shoot,      // 远程射击
    SFX_PlayerHurt, // 玩家受伤
    SFX_PlayerDead, // 玩家死亡

    // ── 敌人音效 ────────────────────────────────────────────────────────
    SFX_EnemyHurt,  // 敌人受伤
    SFX_EnemyDead,  // 敌人死亡

    // ── 关卡/波次音效 ────────────────────────────────────────────────────
    SFX_WaveStart,  // 波次开始
    SFX_WaveEnd,    // 波次结束
    SFX_Victory,    // 通关胜利
    SFX_GameOver,   // 游戏失败
}
