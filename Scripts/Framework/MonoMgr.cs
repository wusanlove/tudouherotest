using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共Mono模块管理器（非MonoBehaviour单例）。
/// 通过内部 MonoHelper 桥接 Unity 生命周期，支持 StartCoroutine 与帧更新事件委托。
/// </summary>
public class MonoMgr : BaseMgr<MonoMgr>
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction lateUpdateEvent;

    /// <summary>Unity MonoBehaviour 桥接体，创建时自动 DontDestroyOnLoad。</summary>
    private MonoHelper helper;

    private MonoMgr()
    {
        // 创建持久化 GameObject 作为生命周期载体
        GameObject go = new GameObject("[MonoMgr]");
        Object.DontDestroyOnLoad(go);
        helper = go.AddComponent<MonoHelper>();
        helper.Init(this);
    }

    // ── Update 监听 ─────────────────────────────────────────────────────────
    public void AddUpdateListener(UnityAction fun)    => updateEvent += fun;
    public void RemoveUpdateListener(UnityAction fun) => updateEvent -= fun;

    // ── FixedUpdate 监听 ────────────────────────────────────────────────────
    public void AddFixedUpdateListener(UnityAction fun)    => fixedUpdateEvent += fun;
    public void RemoveFixedUpdateListener(UnityAction fun) => fixedUpdateEvent -= fun;

    // ── LateUpdate 监听 ─────────────────────────────────────────────────────
    public void AddLateUpdateListener(UnityAction fun)    => lateUpdateEvent += fun;
    public void RemoveLateUpdateListener(UnityAction fun) => lateUpdateEvent -= fun;

    // ── 协程 ────────────────────────────────────────────────────────────────
    /// <summary>通过 MonoHelper 启动协程。</summary>
    public Coroutine StartCoroutine(IEnumerator enumerator)
        => helper.StartCoroutine(enumerator);

    /// <summary>停止一个已启动的协程。</summary>
    public void StopCoroutine(Coroutine coroutine)
        => helper.StopCoroutine(coroutine);

    // ── 由 MonoHelper 在每帧回调 ────────────────────────────────────────────
    internal void OnUpdate()       => updateEvent?.Invoke();
    internal void OnFixedUpdate()  => fixedUpdateEvent?.Invoke();
    internal void OnLateUpdate()   => lateUpdateEvent?.Invoke();
}

/// <summary>
/// MonoMgr 的 MonoBehaviour 生命周期桥接体，由 MonoMgr 构造时自动创建。
/// 不要手动挂载此组件。
/// </summary>
public class MonoHelper : MonoBehaviour
{
    private MonoMgr owner;

    internal void Init(MonoMgr mgr) => owner = mgr;

    private void Update()      => owner?.OnUpdate();
    private void FixedUpdate() => owner?.OnFixedUpdate();
    private void LateUpdate()  => owner?.OnLateUpdate();
}
