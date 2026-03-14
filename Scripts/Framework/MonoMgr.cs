using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共 Mono 模块管理器（非 MonoBehaviour 单例）。
/// 内部创建一个隐藏 MonoBehaviour helper，用于驱动 Update 回调和协程。
/// </summary>
public class MonoMgr : BaseMgr<MonoMgr>
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction lateUpdateEvent;

    private MonoHelper helper;

    private MonoMgr()
    {
        var go = new GameObject("[MonoMgr]");
        Object.DontDestroyOnLoad(go);
        helper = go.AddComponent<MonoHelper>();
        helper.owner = this;
    }

    // -------- 由 MonoHelper 回调 --------
    internal void InvokeUpdate()       => updateEvent?.Invoke();
    internal void InvokeFixedUpdate()  => fixedUpdateEvent?.Invoke();
    internal void InvokeLateUpdate()   => lateUpdateEvent?.Invoke();

    // -------- 协程 --------
    /// <summary>通过内部 MonoBehaviour 代理执行协程。</summary>
    public void StartCoroutine(IEnumerator enumerator)
    {
        helper.StartCoroutine(enumerator);
    }

    // -------- Update 监听 --------
    public void AddUpdateListener(UnityAction updateFun)       => updateEvent += updateFun;
    public void RemoveUpdateListener(UnityAction updateFun)    => updateEvent -= updateFun;
    public void AddFixedUpdateListener(UnityAction updateFun)  => fixedUpdateEvent += updateFun;
    public void RemoveFixedUpdateListener(UnityAction updateFun) => fixedUpdateEvent -= updateFun;
    public void AddLateUpdateListener(UnityAction updateFun)   => lateUpdateEvent += updateFun;
    public void RemoveLateUpdateListener(UnityAction updateFun) => lateUpdateEvent -= updateFun;
}

/// <summary>MonoMgr 的内部 MonoBehaviour 代理，驱动帧回调与协程。</summary>
public class MonoHelper : MonoBehaviour
{
    public MonoMgr owner;
    private void Update()       => owner?.InvokeUpdate();
    private void FixedUpdate()  => owner?.InvokeFixedUpdate();
    private void LateUpdate()   => owner?.InvokeLateUpdate();
}
