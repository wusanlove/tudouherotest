using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 中介者模式（Mediator Pattern）示例
/// 
/// 作用：让商店场景中的多个 UI 对象（WeaponSlot、ItemCardUI 等）
/// 不需要直接互相引用，全部通过 UIShopMediator 协调通信。
/// 
/// 当前问题（参见 Scripts/Weapon/WeaponSlot.cs）：
///   WeaponSlot 直接调用 ShopPanel.Instance.ShowCurrentWeapon()
///   → WeaponSlot 与 ShopPanel 产生直接耦合
///   → 将来增加"出售历史面板"需要修改 WeaponSlot
/// 
/// 解决方案：引入中介者，WeaponSlot 只通知中介者"发生了什么"，
/// 中介者决定通知哪些 UI 更新。
/// 
/// 关键特征：
///   - WeaponSlot / ItemCardUI 只知道中介者，不知道 ShopPanel 的存在
///   - 通信是双向的：A 通知中介者 → 中介者通知 B
///   - 适合多个 UI 面板之间的交互协调
/// 
/// 区别于外观模式：
///   - 外观：对外简化调用，子系统不知道外观
///   - 中介者：各对象都知道中介者，通过它互相间接通信
/// </summary>
public class UIShopMediator : BaseMgrMono<UIShopMediator>
{
    [Header("中介者持有所有参与对象的引用")]
    [SerializeField] private ShopPanel _shopPanel;

    // ────────────────────────────────────────────────────────────────
    // 武器槽右键出售 → 中介者处理 → 通知所有相关 UI 更新
    // WeaponSlot 不需要知道 ShopPanel 是谁
    // ────────────────────────────────────────────────────────────────
    public void OnWeaponSold(WeaponData soldWeapon, int slotIndex)
    {
        if (soldWeapon == null) return;

        // 1. 处理数据
        GameManager.Instance.money += (int)(soldWeapon.price * 0.5f);
        if (slotIndex < GameManager.Instance.currentWeapons.Count)
            GameManager.Instance.currentWeapons.RemoveAt(slotIndex);

        // 2. 通知 UI 更新（通过 ShopPanel 的公开方法，保持封装性）
        _shopPanel.ShowCurrentWeapon();
        _shopPanel.RefreshMoneyText();

        // 将来若添加"出售历史"面板，只在这里加一行，不改 WeaponSlot：
        // _saleHistoryPanel.AddRecord(soldWeapon);
    }

    // ────────────────────────────────────────────────────────────────
    // 武器槽左键合成 → 中介者处理 → 通知 UI 更新
    // ────────────────────────────────────────────────────────────────
    public void OnWeaponMergeRequested(WeaponData weaponData, int slotIndex)
    {
        if (weaponData == null) return;

        for (int i = 0; i < GameManager.Instance.currentWeapons.Count; i++)
        {
            if (i == slotIndex) continue;

            if (weaponData.id == GameManager.Instance.currentWeapons[i].id &&
                weaponData.grade == GameManager.Instance.currentWeapons[i].grade)
            {
                GameManager.Instance.currentWeapons[slotIndex].grade += 1;
                GameManager.Instance.currentWeapons[slotIndex].price *= 2;
                GameManager.Instance.currentWeapons.RemoveAt(i);

                // 通知 UI 更新
                _shopPanel.ShowCurrentWeapon();
                break;
            }
        }
    }
}

// ────────────────────────────────────────────────────────────────
// 重构后的 WeaponSlot（对比 Scripts/Weapon/WeaponSlot.cs）
// 只依赖中介者，不直接调用 ShopPanel
// ────────────────────────────────────────────────────────────────
// public class WeaponSlot_WithMediator : MonoBehaviour, IPointerClickHandler
// {
//     public WeaponData weaponData;
//     public Image _weaponIcon;
//     public Image _weaponBG;
//     public int slotCount;
//
//     // 只需要中介者引用，不再需要 ShopPanel 引用
//     private UIShopMediator _mediator => UIShopMediator.Instance;
//
//     public void OnPointerClick(PointerEventData eventData)
//     {
//         if (eventData.button == PointerEventData.InputButton.Right)
//         {
//             // 通知中介者"我被右键了"，具体如何协调交给中介者
//             _mediator.OnWeaponSold(weaponData, slotCount);
//             weaponData = null;
//             _weaponIcon.enabled = false;
//         }
//
//         if (eventData.button == PointerEventData.InputButton.Left)
//         {
//             // 通知中介者"我被左键了，请尝试合成"
//             _mediator.OnWeaponMergeRequested(weaponData, slotCount);
//         }
//     }
// }
