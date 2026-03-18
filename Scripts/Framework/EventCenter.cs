using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>里式替换载体基类（无泛型参数）。</summary>
public abstract class EventInfoBase { }

/// <summary>带泛型参数的事件包装，持有带参委托列表。</summary>
public class EventInfo<T> : EventInfoBase
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action) => actions += action;
}

/// <summary>无参数事件包装。</summary>
public class EventInfo : EventInfoBase
{
    public UnityAction actions;
    public EventInfo(UnityAction action) => actions += action;
}

/// <summary>
/// 事件中心 —— 跨系统广播用（金币/血量变化、波次开始/结束等）。
/// 不应在此驱动"打开哪个面板/切哪个场景"，那是 SceneStateController 的职责。
/// TODO: 演进方向 → 泛型 IEventBus 接口 + DI 注入，彻底去除静态单例。
/// </summary>
public class EventCenter : BaseMgr<EventCenter>
{
    private EventCenter() { }

    private readonly Dictionary<E_EventType, EventInfoBase> eventDic =
        new Dictionary<E_EventType, EventInfoBase>();

    // ── 触发 ──────────────────────────────────────────────────────────────

    public void EventTrigger<T>(E_EventType eventName, T info)
    {
        if (!eventDic.TryGetValue(eventName, out var raw)) return;

        var typed = raw as EventInfo<T>;
        if (typed == null)
        {
            // 类型不匹配：常见于同一事件用不同 T 注册，编辑器下明确提示
#if UNITY_EDITOR
            Debug.LogError($"[EventCenter] 事件 {eventName} 类型不匹配，" +
                           $"期望 EventInfo<{typeof(T).Name}>，实际 {raw.GetType().Name}");
#endif
            return;
        }
        typed.actions?.Invoke(info);
    }

    public void EventTrigger(E_EventType eventName)
    {
        if (!eventDic.TryGetValue(eventName, out var raw)) return;

        var typed = raw as EventInfo;
        if (typed == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[EventCenter] 事件 {eventName} 类型不匹配，期望无参 EventInfo，实际 {raw.GetType().Name}");
#endif
            return;
        }
        typed.actions?.Invoke();
    }

    // ── 订阅 ──────────────────────────────────────────────────────────────

    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.TryGetValue(eventName, out var raw))
        {
            var typed = raw as EventInfo<T>;
            if (typed == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[EventCenter] AddEventListener 类型不匹配：{eventName}");
#endif
                return;
            }
            typed.actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T>(func));
        }
    }

    public void AddEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.TryGetValue(eventName, out var raw))
        {
            var typed = raw as EventInfo;
            if (typed == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[EventCenter] AddEventListener 类型不匹配：{eventName}");
#endif
                return;
            }
            typed.actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(func));
        }
    }

    // ── 取消订阅 ──────────────────────────────────────────────────────────

    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.TryGetValue(eventName, out var raw) && raw is EventInfo<T> typed)
            typed.actions -= func;
    }

    public void RemoveEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.TryGetValue(eventName, out var raw) && raw is EventInfo typed)
            typed.actions -= func;
    }

    // ── 清理 ──────────────────────────────────────────────────────────────

    /// <summary>清除指定事件的全部监听（常用于切场景前清理）。</summary>
    public void ClearEvent(E_EventType eventName)
    {
        eventDic.Remove(eventName);
    }

    /// <summary>清空所有事件（谨慎使用，会断开所有系统的监听）。</summary>
    public void Clear()
    {
        eventDic.Clear();
    }

    /// <summary>已废弃，请改用 ClearEvent。</summary>
    [System.Obsolete("拼写错误遗留，请使用 ClearEvent(eventName)")]
    public void Claer(E_EventType eventName) => ClearEvent(eventName);
}
