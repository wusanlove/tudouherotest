using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店业务控制器（UseCase）：管理商品列表生成、购买逻辑。
/// ShopPanel 只负责 View；购买结果通过返回值 + EventCenter 通知。
/// 
/// 依赖注入演进：将 ConfigService/GameManager 接口化后，可通过构造注入，
/// 便于单元测试（当前用单例）。
/// </summary>
public class ShopController : BaseMgr<ShopController>
{
    private ShopController() { }

    // ── 生成本轮商品列表 ──────────────────────────────────
    /// <summary>随机从 Weapon + Prop 池中抽取 count 件商品</summary>
    public List<ItemData> GenerateShopItems(int count = 4)
    {
        var allItems = new List<ItemData>();
        allItems.AddRange(ConfigService.Instance.Weapons);
        allItems.AddRange(ConfigService.Instance.Props);

        var result  = new List<ItemData>();
        var indices = new HashSet<int>();
        int max     = allItems.Count;

        while (result.Count < count && result.Count < max)
        {
            int idx = Random.Range(0, max);
            if (indices.Add(idx))
                result.Add(allItems[idx]);
        }

        return result;
    }

    // ── 购买 ─────────────────────────────────────────────
    /// <summary>
    /// 尝试购买一件物品。
    /// 返回 true 表示购买成功；false 表示金币不足。
    /// </summary>
    public bool TryBuy(ItemData item)
    {
        float actualPrice = item.price * GameManager.Instance.propData.shopDiscount;

        if (GameManager.Instance.money < actualPrice)
            return false;

        GameManager.Instance.money -= actualPrice;
        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_MoneyChanged, GameManager.Instance.money);

        Apply(item);
        EventCenter.Instance.EventTrigger(E_EventType.Shop_ItemBought, item);
        return true;
    }

    private void Apply(ItemData item)
    {
        if (item is WeaponData weapon)
        {
            if (GameManager.Instance.currentWeapons.Count < GameManager.Instance.propData.slot)
                GameManager.Instance.currentWeapons.Add(weapon);
            else
                Debug.LogWarning("[ShopController] 武器格已满");
        }
        else if (item is PropData prop)
        {
            GameManager.Instance.currentProps.Add(prop);
            GameManager.Instance.FusionAttr(prop);
        }
    }

    // ── 结束商店，进入下一波 ──────────────────────────────
    public void ProceedToNextWave()
    {
        GameManager.Instance.waveCount += 1;
        EventCenter.Instance.EventTrigger(E_EventType.Shop_GoNextWave);
        GameFlowController.Instance.GoToGamePlay();
    }
}
