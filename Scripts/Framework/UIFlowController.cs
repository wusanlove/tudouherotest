using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 流程控制器 – 管理面板的互斥打开/关闭、输入锁定。
/// Panel 只需调用 UIFlowController.Instance.OpenPanel / ClosePanel，
/// 无需直接操作其他面板。
///
/// 设计约定：
///   - 同一时刻只有一个"互斥"面板处于打开状态（可叠加非互斥弹窗）。
///   - 输入锁定时 Player.Update、Weapon.Update 中的 isDead 检查会阻止行为。
/// </summary>
public class UIFlowController : BaseMgr<UIFlowController>
{
    private readonly Dictionary<string, BasePanel> _panels = new Dictionary<string, BasePanel>();
    private readonly Stack<string> _panelStack = new Stack<string>();

    private bool _inputLocked;

    private UIFlowController() { }

    // ──────────────── 注册/注销 ────────────────

    /// <summary>注册一个面板，供后续通过名称管理。</summary>
    public void Register(string panelName, BasePanel panel)
    {
        if (!_panels.ContainsKey(panelName))
            _panels[panelName] = panel;
    }

    public void Unregister(string panelName) => _panels.Remove(panelName);

    // ──────────────── 打开/关闭 ────────────────

    /// <summary>打开指定面板（互斥：先关闭栈顶面板）。</summary>
    public void OpenPanel(string panelName, bool exclusive = true)
    {
        if (!_panels.TryGetValue(panelName, out BasePanel panel)) return;

        if (exclusive && _panelStack.Count > 0)
        {
            string top = _panelStack.Peek();
            if (_panels.TryGetValue(top, out BasePanel topPanel))
                topPanel.HidePanel(null);
        }

        panel.ShowPanel();
        _panelStack.Push(panelName);
    }

    /// <summary>关闭栈顶面板，恢复下一个面板。</summary>
    public void CloseTopPanel()
    {
        if (_panelStack.Count == 0) return;
        string top = _panelStack.Pop();
        if (_panels.TryGetValue(top, out BasePanel topPanel))
            topPanel.HidePanel(null);

        if (_panelStack.Count > 0)
        {
            string next = _panelStack.Peek();
            if (_panels.TryGetValue(next, out BasePanel nextPanel))
                nextPanel.ShowPanel();
        }
    }

    /// <summary>直接关闭指定面板（不影响栈）。</summary>
    public void ClosePanel(string panelName)
    {
        if (_panels.TryGetValue(panelName, out BasePanel panel))
            panel.HidePanel(null);
    }

    /// <summary>清空所有面板与栈（场景切换时调用）。</summary>
    public void ClearAll()
    {
        _panelStack.Clear();
        _panels.Clear();
        _inputLocked = false;
    }

    // ──────────────── 输入锁定 ────────────────

    public void LockInput()   => _inputLocked = true;
    public void UnlockInput() => _inputLocked = false;
    public bool IsInputLocked => _inputLocked;
}
