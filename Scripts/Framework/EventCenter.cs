using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用于里氏替换原则，装载子类的父类。
/// </summary>
public abstract class EventInfoBase
{
    /// <summary>注册时的 payload 类型，用于运行时类型冲突检测。</summary>
    public abstract Type PayloadType { get; }
}

/// <summary>带泛型参数的事件容器。</summary>
public class EventInfo<T> : EventInfoBase
{
    public UnityAction<T> actions;
    public override Type PayloadType => typeof(T);
    public EventInfo(UnityAction<T> action) { actions += action; }
}

/// <summary>无参事件容器。</summary>
public class EventInfo : EventInfoBase
{
    public UnityAction actions;
    public override Type PayloadType => typeof(void);
    public EventInfo(UnityAction action) { actions += action; }
}

/// <summary>
/// 事件中心 — 全局消息总线，负责跨系统"广播"。
/// 只做事件广播（血量变化/金币变化/Wave 结束等），
/// 不负责场景跳转或 UI 流程控制（那是 SceneFlowController / UIFlowController 的职责）。
///
/// 改进点（相比旧版）：
///   1. 类型一致性保护：同一 E_EventType 不允许混用不同 payload 类型，Editor 下明确报错。
///   2. 自动清理：移除监听后若该事件无订阅者，则从字典删除。
///   3. AddOnceListener：一次性监听，触发后自动移除，减少泄漏风险。
///   4. ClearEvent：清除指定事件；Claer 保留但标记为过时。
///
/// 未来演进思路（DI）：将 EventCenter 注册到 ServiceLocator/Zenject，
/// 通过构造注入替代 Instance 访问，方便单元测试与模块解耦。
/// </summary>
public class EventCenter : BaseMgr<EventCenter>
{
    private EventCenter() { }

    private readonly Dictionary<E_EventType, EventInfoBase> eventDic =
        new Dictionary<E_EventType, EventInfoBase>();

    // ──────────────────────────────────────────────────────────────
    //  添加监听
    // ──────────────────────────────────────────────────────────────

    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase existing))
        {
            if (existing is EventInfo<T> typedInfo)
                typedInfo.actions += func;
            else
                LogTypeMismatch(eventName, existing.PayloadType, typeof(T));
        }
        else
        {
            eventDic[eventName] = new EventInfo<T>(func);
        }
    }

    public void AddEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase existing))
        {
            if (existing is EventInfo typedInfo)
                typedInfo.actions += func;
            else
                LogTypeMismatch(eventName, existing.PayloadType, typeof(void));
        }
        else
        {
            eventDic[eventName] = new EventInfo(func);
        }
    }

    /// <summary>注册一次性监听，触发后自动移除，减少忘记取消订阅的泄漏风险。</summary>
    public void AddOnceListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        // wrapper 先赋 null 是为了让 lambda 能在闭包中捕获自身引用，
        // 从而在触发后通过 RemoveEventListener 将自己移除（自引用闭包惯用写法）。
        UnityAction<T> wrapper = null;
        wrapper = arg =>
        {
            func(arg);
            RemoveEventListener(eventName, wrapper);
        };
        AddEventListener(eventName, wrapper);
    }

    /// <summary>注册无参一次性监听。</summary>
    public void AddOnceListener(E_EventType eventName, UnityAction func)
    {
        // 同上：null 先占位，lambda 通过闭包捕获 wrapper 后自行注销。
        UnityAction wrapper = null;
        wrapper = () =>
        {
            func();
            RemoveEventListener(eventName, wrapper);
        };
        AddEventListener(eventName, wrapper);
    }

    // ──────────────────────────────────────────────────────────────
    //  移除监听（移除后无订阅者则自动清理字典，防止内存泄漏）
    // ──────────────────────────────────────────────────────────────

    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (!eventDic.TryGetValue(eventName, out EventInfoBase existing)) return;
        if (existing is EventInfo<T> typedInfo)
        {
            typedInfo.actions -= func;
            if (typedInfo.actions == null)
                eventDic.Remove(eventName);
        }
    }

    public void RemoveEventListener(E_EventType eventName, UnityAction func)
    {
        if (!eventDic.TryGetValue(eventName, out EventInfoBase existing)) return;
        if (existing is EventInfo typedInfo)
        {
            typedInfo.actions -= func;
            if (typedInfo.actions == null)
                eventDic.Remove(eventName);
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  触发事件
    // ──────────────────────────────────────────────────────────────

    public void EventTrigger<T>(E_EventType eventName, T info)
    {
        if (!eventDic.TryGetValue(eventName, out EventInfoBase existing)) return;
        if (existing is EventInfo<T> typedInfo)
            typedInfo.actions?.Invoke(info);
#if UNITY_EDITOR
        else
            Debug.LogWarning($"[EventCenter] 触发 {eventName} 类型不匹配：" +
                             $"期望 {existing.PayloadType.Name}，传入 {typeof(T).Name}");
#endif
    }

    public void EventTrigger(E_EventType eventName)
    {
        if (!eventDic.TryGetValue(eventName, out EventInfoBase existing)) return;
        if (existing is EventInfo typedInfo)
            typedInfo.actions?.Invoke();
#if UNITY_EDITOR
        else
            Debug.LogWarning($"[EventCenter] 事件 {eventName} 需要带参数触发，却以无参方式调用。");
#endif
    }

    // ──────────────────────────────────────────────────────────────
    //  清理
    // ──────────────────────────────────────────────────────────────

    /// <summary>清除指定事件的所有监听。</summary>
    public void ClearEvent(E_EventType eventName) => eventDic.Remove(eventName);

    /// <summary>清空所有事件监听（场景销毁/重置时调用）。</summary>
    public void Clear() => eventDic.Clear();

    /// <summary>已过时，请改用 <see cref="ClearEvent"/>（原拼写错误）。</summary>
    [Obsolete("拼写错误，请改用 ClearEvent(E_EventType)")]
    public void Claer(E_EventType eventName) => ClearEvent(eventName);

    // ──────────────────────────────────────────────────────────────
    //  内部辅助
    // ──────────────────────────────────────────────────────────────

    private static void LogTypeMismatch(E_EventType name, Type existing, Type incoming)
    {
#if UNITY_EDITOR
        Debug.LogError($"[EventCenter] 类型冲突！事件 {name} 已注册为 {existing.Name}，" +
                       $"不能再注册为 {incoming.Name}。请统一同一事件的 payload 类型。");
#endif
    }
}
