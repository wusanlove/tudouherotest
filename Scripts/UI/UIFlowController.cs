using UnityEngine;

/// <summary>
/// UI 流程控制器 — 统一管理 LevelSelect 场景内三个选择面板的互斥切换。
///
/// 职责：
///   · 监听 Panel 抛出的 UI_OnXxxSelected 事件。
///   · 保存选择结果到 GameManager（单一写入点）。
///   · 决定哪个面板显示/隐藏（互斥逻辑集中在此，Panel 自身不再互相引用）。
///   · 选择完成后触发 Flow_GoToXxx 事件，由 SceneFlowController 切场景。
///
/// 未来演进思路：
///   · 扩展为带栈管理的多层面板系统（支持 Back 键返回上一面板）。
///   · 通过 ServiceLocator/Zenject 注入，替换 Instance 访问。
/// </summary>
public class UIFlowController : BaseMgr<UIFlowController>
{
    private UIFlowController()
    {
        RegisterUIEvents();
    }

    private void RegisterUIEvents()
    {
        // 主菜单 → 进入关卡选择
        EventCenter.Instance.AddEventListener(E_EventType.UI_OnStartGameClicked, OnStartGameClicked);

        // 关卡选择三步流程
        EventCenter.Instance.AddEventListener<RoleData>(E_EventType.UI_OnRoleSelected, OnRoleSelected);
        EventCenter.Instance.AddEventListener<WeaponData>(E_EventType.UI_OnWeaponSelected, OnWeaponSelected);
        EventCenter.Instance.AddEventListener<DifficultyData>(E_EventType.UI_OnDifficultySelected, OnDifficultySelected);

        // 商店 → 下一波
        EventCenter.Instance.AddEventListener(E_EventType.UI_OnNextWaveClicked, OnNextWaveClicked);
    }

    // ── 事件处理 ──────────────────────────────────────────────────

    private void OnStartGameClicked()
    {
        EventCenter.Instance.EventTrigger(E_EventType.Flow_GoToLevelSelect);
    }

    private void OnRoleSelected(RoleData roleData)
    {
        GameManager.Instance.currentRoleData = roleData;

        // 互斥切换：关闭角色面板，打开武器面板
        SetPanelVisible(RoleSelectPanel.Instance?._canvasGroup, false);

        var weaponPanel = WeaponSelectPanel.Instance;
        if (weaponPanel != null)
        {
            SetPanelVisible(weaponPanel._canvasGroup, true);

            // 把角色详情克隆到武器面板的详情区（视觉连续性）
            var rolePanel = RoleSelectPanel.Instance;
            if (rolePanel?._roleDetailGameObject != null && weaponPanel._weaponDetailTransform != null)
            {
                Object.Instantiate(rolePanel._roleDetailGameObject, weaponPanel._weaponDetailTransform);
                if (weaponPanel._weaponDetailGameObject != null)
                    weaponPanel._weaponDetailGameObject.SetActive(true);
            }
        }
    }

    private void OnWeaponSelected(WeaponData weaponData)
    {
        GameManager.Instance.currentWeapons.Clear();
        GameManager.Instance.currentWeapons.Add(weaponData);

        // 互斥切换：关闭武器面板，打开难度面板
        SetPanelVisible(WeaponSelectPanel.Instance?._canvasGroup, false);

        var diffPanel = DifficultySelectPanel.Instance;
        if (diffPanel != null)
        {
            SetPanelVisible(diffPanel._canvasGroup, true);

            // 把角色 & 武器详情克隆到难度面板详情区
            var rolePanel   = RoleSelectPanel.Instance;
            var weaponPanel = WeaponSelectPanel.Instance;
            if (rolePanel?._roleDetailGameObject != null && diffPanel._difficultyDetailTransform != null)
                Object.Instantiate(rolePanel._roleDetailGameObject, diffPanel._difficultyDetailTransform);
            if (weaponPanel?._weaponDetailGameObject != null && diffPanel._difficultyDetailTransform != null)
                Object.Instantiate(weaponPanel._weaponDetailGameObject, diffPanel._difficultyDetailTransform);
            if (diffPanel._difficultyDetailGameObject != null)
                diffPanel._difficultyDetailGameObject.SetActive(true);
        }
    }

    private void OnDifficultySelected(DifficultyData difficultyData)
    {
        GameManager.Instance.currentDifficulty = difficultyData;
        // 选完难度 → 进入 GamePlay（SceneFlowController 处理实际场景加载）
        EventCenter.Instance.EventTrigger(E_EventType.Flow_GoToGamePlay);
    }

    private void OnNextWaveClicked()
    {
        EventCenter.Instance.EventTrigger(E_EventType.Flow_GoToShop);
    }

    // ── 辅助 ─────────────────────────────────────────────────────

    private static void SetPanelVisible(UnityEngine.UI.CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha            = visible ? 1f : 0f;
        cg.interactable     = visible;
        cg.blocksRaycasts   = visible;
    }
}
