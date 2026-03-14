using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共 Mono 模块 — 为纯 C# 单例提供 Update 订阅与协程能力。
/// 内部用隐藏的 MonoHelper MonoBehaviour 驱动，外部无感知。
///
/// 未来演进思路（DI）：将 MonoMgr 注册为服务，注入需要协程/Update 的类中。
/// </summary>
public class MonoMgr : BaseMgr<MonoMgr>
{
    private MonoHelper helper;

    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction lateUpdateEvent;

    private MonoMgr()
    {
        // 创建跨场景的隐藏驱动对象
        GameObject go = new GameObject("[MonoHelper]");
        Object.DontDestroyOnLoad(go);
        helper = go.AddComponent<MonoHelper>();
        helper.SetMgr(this);
    }

    public void AddUpdateListener(UnityAction updateFun)       => updateEvent += updateFun;
    public void RemoveUpdateListener(UnityAction updateFun)    => updateEvent -= updateFun;
    public void AddFixedUpdateListener(UnityAction updateFun)  => fixedUpdateEvent += updateFun;
    public void RemoveFixedUpdateListener(UnityAction updateFun) => fixedUpdateEvent -= updateFun;
    public void AddLateUpdateListener(UnityAction updateFun)   => lateUpdateEvent += updateFun;
    public void RemoveLateUpdateListener(UnityAction updateFun) => lateUpdateEvent -= updateFun;

    /// <summary>在隐藏 MonoBehaviour 上启动协程。</summary>
    public Coroutine StartCoroutine(IEnumerator enumerator) => helper.StartCoroutine(enumerator);

    /// <summary>停止由本管理器启动的协程。</summary>
    public void StopCoroutine(Coroutine coroutine) => helper.StopCoroutine(coroutine);

    // 由 MonoHelper 每帧调用
    internal void InvokeUpdate()      => updateEvent?.Invoke();
    internal void InvokeFixedUpdate() => fixedUpdateEvent?.Invoke();
    internal void InvokeLateUpdate()  => lateUpdateEvent?.Invoke();
}

/// <summary>MonoMgr 的隐藏驱动体，不对外暴露。</summary>
internal sealed class MonoHelper : MonoBehaviour
{
    private MonoMgr mgr;
    public void SetMgr(MonoMgr m) => mgr = m;
    private void Update()      => mgr?.InvokeUpdate();
    private void FixedUpdate() => mgr?.InvokeFixedUpdate();
    private void LateUpdate()  => mgr?.InvokeLateUpdate();
}
