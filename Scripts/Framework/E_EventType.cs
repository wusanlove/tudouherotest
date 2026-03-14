using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum E_EventType
{
    #region 开始游戏界面
    BeginScene_btn_start,
    BeginScene_btn_exitSetting,
    #endregion

    #region 玩家状态事件
    /// <summary>玩家血量变化 (float newHp)</summary>
    Player_HpChanged,
    /// <summary>玩家金钱变化 (float newMoney)</summary>
    Player_MoneyChanged,
    /// <summary>玩家经验变化 (float newExp)</summary>
    Player_ExpChanged,
    /// <summary>玩家死亡</summary>
    Player_Dead,
    /// <summary>玩家受伤 (float damage)</summary>
    Player_Injured,
    #endregion

    #region 波次/关卡事件
    /// <summary>波次开始 (int waveIndex)</summary>
    Wave_Started,
    /// <summary>波次结束</summary>
    Wave_Ended,
    /// <summary>倒计时更新 (float remaining)</summary>
    Wave_TimerUpdated,
    /// <summary>波次信息更新</summary>
    Wave_InfoUpdated,
    #endregion

    #region 游戏结果事件
    /// <summary>游戏胜利</summary>
    Game_Win,
    /// <summary>游戏失败</summary>
    Game_Lose,
    #endregion

    #region 商店事件
    /// <summary>物品购买成功 (ItemData item)</summary>
    Shop_ItemPurchased,
    /// <summary>商店刷新</summary>
    Shop_Refreshed,
    #endregion

    #region 状态机/场景事件
    /// <summary>请求进入主菜单场景</summary>
    Flow_EnterMainMenu,
    /// <summary>请求进入选择场景</summary>
    Flow_EnterSelect,
    /// <summary>请求进入游戏场景</summary>
    Flow_EnterGame,
    /// <summary>请求进入商店场景</summary>
    Flow_EnterShop,
    #endregion

    #region 音效
    Audio_PlaySfx,
    Audio_PlayBgm,
    Audio_StopBgm,
    Audio_SetVolume,
    #endregion
}
