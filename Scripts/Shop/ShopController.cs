using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 商店业务控制器（Use-Case 层）。
///
/// 职责：
/// - 校验购买条件（金币、武器槽位、道具上限、折扣后价格）。
/// - 执行购买并更新 GameManager 状态（currentWeapons / currentProps / propData / money）。
/// - 校验并执行刷新（扣金币）。
///
/// 边界规则：
/// - ShopPanel 只负责 View + 事件抛出，不含业务判断。
/// - 此 Controller 不持有任何 MonoBehaviour，可在单元测试中直接调用。
/// </summary>
public class ShopController : BaseMgr<ShopController>
{
    /// <summary>每次刷新商店的金币消耗。</summary>
    public const float RefreshCost = 3f;

    private ShopController() { }

    // ── 购买 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// 尝试购买道具，返回带结果的 <see cref="ShopBuyResult"/>。
    /// 调用方应通过 EventCenter 将结果广播给 UI，而非直接操作面板。
    /// </summary>
    public ShopBuyResult TryBuy(ItemData itemData)
    {
        var result = new ShopBuyResult { item = itemData, success = false };
        if (itemData == null) return result;

        var gm = GameManager.Instance;

        // ── 武器槽位上限 ────────────────────────────────────────────────────
        if (itemData is WeaponData && gm.currentWeapons.Count >= gm.propData.slot)
        {
            Debug.Log("[ShopController] 武器槽已满");
            return result;
        }

        // ── 道具上限 ────────────────────────────────────────────────────────
        if (itemData is PropData && gm.currentProps.Count >= 20)
        {
            Debug.Log("[ShopController] 道具已满");
            return result;
        }

        // ── 金币不足 ────────────────────────────────────────────────────────
        float effectivePrice = itemData.price * gm.propData.shopDiscount;
        if (gm.money < effectivePrice)
        {
            Debug.Log("[ShopController] 金币不足");
            return result;
        }

        // ── 扣款并写入状态 ───────────────────────────────────────────────────
        gm.money -= effectivePrice;

        if (itemData is WeaponData weapon)
        {
            // 深拷贝：防止多次购买同一武器时共享引用
            WeaponData copy = JsonConvert.DeserializeObject<WeaponData>(JsonConvert.SerializeObject(weapon));
            gm.currentWeapons.Add(copy);
        }
        else if (itemData is PropData prop)
        {
            PropData copy = JsonConvert.DeserializeObject<PropData>(JsonConvert.SerializeObject(prop));
            gm.currentProps.Add(copy);
            gm.FusionAttr(copy);
        }

        result.success = true;
        return result;
    }

    // ── 刷新 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// 尝试消耗金币刷新商店。返回 true 时，ShopPanel 负责重新随机道具列表。
    /// </summary>
    public bool TryRefresh()
    {
        var gm = GameManager.Instance;
        if (gm.money < RefreshCost) return false;
        gm.money -= RefreshCost;
        return true;
    }
}
