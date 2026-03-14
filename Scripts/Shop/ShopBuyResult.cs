/// <summary>
/// 购买结果事件载体，通过 EventCenter 传递给 ItemCardUI 等监听者。
/// </summary>
public struct ShopBuyResult
{
    /// <summary>是否购买成功。</summary>
    public bool success;
    /// <summary>被购买的道具（失败时也携带，方便 UI 做高亮/抖动提示）。</summary>
    public ItemData item;
}
