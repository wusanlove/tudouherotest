using UnityEngine;
public enum E_EventType
{
    #region 开始场景 UI
    BeginScene_btn_start,
    BeginScene_btn_exitSetting,
    #endregion

    #region 场景流转（由 GameManager 监听，驱动 SceneStateController）
    Scene_ToLevelSelect,    // 跳转角色/难度选择场景
    Scene_ToGamePlay,       // 开始/继续游戏
    Scene_ToShop,           // 进入商店
    Scene_ToMainMenu,       // 回主菜单（胜利/失败后）
    #endregion

    #region 玩家状态（float payload）
    Player_HpChanged,       // 当前血量变化
    Player_MoneyChanged,    // 金币变化
    Player_ExpChanged,      // 经验值变化
    Player_Dead,            // 玩家死亡（无参）
    #endregion

    #region 关卡/波次
    Wave_Started,           // int：当前波次编号
    Wave_Ended,             // int：刚结束的波次编号
    Wave_AllCleared,        // 全部波次通关（无参）
    #endregion

    #region 音效服务
    Audio_PlaySfx,
    Audio_PlayBgm,
    Audio_StopBgm,
    Audio_SetVolume,
    #endregion
}
