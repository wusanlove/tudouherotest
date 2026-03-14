using UnityEngine;

/// <summary>
/// 选择界面中介者（MonoBehaviour，挂在 02-LevelSelect 场景的根对象上）。
/// 
/// 职责：协调 RoleSelectPanel / WeaponSelectPanel / DifficultySelectPanel 的显示互斥。
/// 各面板和 Item（RoleUI/WeaponUI/DifficultyUI）只和 SelectSceneMediator 通信，
/// 不互相直接引用。
/// 
/// 依赖注入演进：若用 DI，将三个 Panel 从 Inspector 注入而非 Find；其余不变。
/// </summary>
public class SelectSceneMediator : BaseMgrMono<SelectSceneMediator>
{
    [Header("三个选择面板（Inspector 拖入）")]
    [SerializeField] private RoleSelectPanel     rolePanel;
    [SerializeField] private WeaponSelectPanel   weaponPanel;
    [SerializeField] private DifficultySelectPanel difficultyPanel;

    public override void Awake()
    {
        base.Awake();

        // 订阅来自 SelectScene 状态的事件
        EventCenter.Instance.AddEventListener(E_EventType.Select_OpenRole,      OnOpenRole);
        EventCenter.Instance.AddEventListener<RoleData>(E_EventType.Select_RoleChosen,     OnRoleChosen);
        EventCenter.Instance.AddEventListener<WeaponData>(E_EventType.Select_WeaponChosen, OnWeaponChosen);
        EventCenter.Instance.AddEventListener<DifficultyData>(E_EventType.Select_DifficultyChosen, OnDifficultyChosen);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener(E_EventType.Select_OpenRole,      OnOpenRole);
        EventCenter.Instance.RemoveEventListener<RoleData>(E_EventType.Select_RoleChosen,     OnRoleChosen);
        EventCenter.Instance.RemoveEventListener<WeaponData>(E_EventType.Select_WeaponChosen, OnWeaponChosen);
        EventCenter.Instance.RemoveEventListener<DifficultyData>(E_EventType.Select_DifficultyChosen, OnDifficultyChosen);
    }

    // ── 步骤一：打开角色选择 ─────────────────────────────
    private void OnOpenRole()
    {
        Show(rolePanel._canvasGroup);
        Hide(weaponPanel._canvasGroup);
        Hide(difficultyPanel._canvasGroup);
    }

    // ── 步骤二：角色已选，打开武器选择 ─────────────────────
    private void OnRoleChosen(RoleData data)
    {
        GameManager.Instance.currentRoleData = data;

        Hide(rolePanel._canvasGroup);
        Show(weaponPanel._canvasGroup);
        Hide(difficultyPanel._canvasGroup);

        CloneDetail(rolePanel._roleDetailGameObject, weaponPanel._weaponDetailTransform);
        weaponPanel._weaponDetailGameObject?.SetActive(true);
    }

    // ── 步骤三：武器已选，打开难度选择 ─────────────────────
    private void OnWeaponChosen(WeaponData data)
    {
        GameManager.Instance.currentWeapons.Add(data);

        Hide(rolePanel._canvasGroup);
        Hide(weaponPanel._canvasGroup);
        Show(difficultyPanel._canvasGroup);

        CloneDetail(rolePanel._roleDetailGameObject,  difficultyPanel._difficultyDetailTransform);
        CloneDetail(weaponPanel._weaponDetailGameObject, difficultyPanel._difficultyDetailTransform);
        difficultyPanel._difficultyDetailGameObject?.SetActive(true);
    }

    // ── 步骤四：难度已选，进入游戏 ─────────────────────────
    private void OnDifficultyChosen(DifficultyData data)
    {
        GameManager.Instance.currentDifficulty = data;
        Hide(difficultyPanel._canvasGroup);

        // 通过 GameFlowController 跳转，View 层不接触 SceneManager
        GameFlowController.Instance.GoToGamePlay();
    }

    // ── 工具方法 ─────────────────────────────────────────
    /// <summary>将 source 克隆并挂在 target 下（空安全）</summary>
    private static void CloneDetail(GameObject source, Transform target)
    {
        if (source != null && target != null)
            Object.Instantiate(source, target);
    }

    private static void Show(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 1; cg.interactable = true; cg.blocksRaycasts = true;
    }

    private static void Hide(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0; cg.interactable = false; cg.blocksRaycasts = false;
    }
}
