using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum E_EventType
{
    #region 主菜单
    BeginScene_btn_start,
    BeginScene_btn_exitSetting,
    #endregion

    #region 音效
    Audio_PlaySfx,
    Audio_PlayBgm,
    Audio_StopBgm,
    Audio_SetVolume,
    #endregion

    #region 场景流程 (GameFlowController 使用)
    Flow_GoToMainMenu,
    Flow_GoToLevelSelect,
    Flow_GoToGamePlay,
    Flow_GoToShop,
    #endregion

    #region 选择界面 (SelectSceneMediator 使用)
    Select_OpenRole,
    Select_OpenWeapon,
    Select_OpenDifficulty,
    Select_RoleChosen,      // param: RoleData
    Select_WeaponChosen,    // param: WeaponData
    Select_DifficultyChosen,// param: DifficultyData
    #endregion

    #region 战斗 — 玩家/UI 解耦
    GamePlay_HpChanged,     // param: float (current hp)
    GamePlay_ExpChanged,    // param: float (current exp)
    GamePlay_MoneyChanged,  // param: float (current money)
    GamePlay_WaveChanged,   // param: int  (current wave)
    GamePlay_CountDown,     // param: float (remaining seconds)
    GamePlay_PlayerDead,
    GamePlay_WaveEnd,       // param: int (wave that just ended)
    GamePlay_AllWavesClear,
    #endregion

    #region 商店
    Shop_Refreshed,
    Shop_ItemBought,        // param: ItemData
    Shop_GoNextWave,
    #endregion
}
