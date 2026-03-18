using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Mono 生命周期代理 —— 为纯 C# 管理器提供 Update 事件与协程能力。
/// 自动在首次访问时创建隐藏 GameObject，跨场景持久。
/// TODO: 演进方向 → 用 PlayerLoopSystem 注入自定义 loop，彻底去除 GameObject 依赖。
/// </summary>
public class MonoMgr : MonoBehaviour
{
    private static MonoMgr instance;

    public static MonoMgr Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("[MonoMgr]");
                instance = go.AddComponent<MonoMgr>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction lateUpdateEvent;

    public void AddUpdateListener(UnityAction fn) => updateEvent += fn;
    public void RemoveUpdateListener(UnityAction fn) => updateEvent -= fn;

    public void AddFixedUpdateListener(UnityAction fn) => fixedUpdateEvent += fn;
    public void RemoveFixedUpdateListener(UnityAction fn) => fixedUpdateEvent -= fn;

    public void AddLateUpdateListener(UnityAction fn) => lateUpdateEvent += fn;
    public void RemoveLateUpdateListener(UnityAction fn) => lateUpdateEvent -= fn;

    // StartCoroutine is inherited from MonoBehaviour — no override needed.

    private void Update() => updateEvent?.Invoke();
    private void FixedUpdate() => fixedUpdateEvent?.Invoke();
    private void LateUpdate() => lateUpdateEvent?.Invoke();
}
