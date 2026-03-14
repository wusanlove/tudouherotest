using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Resources 资源加载模块管理器
/// </summary>
public class ResMgr : BaseMgr<ResMgr>
{
    private ResMgr() { }

    /// <summary>
    /// 同步加载Resources下资源的方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 异步加载资源的方法
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（Resources下的）</param>
    /// <param name="callBack">加载结束后的回调函数 当异步加载资源结束后才会调用</param>
    public void LoadAsync<T>(string path, UnityAction<T> callBack) where T : UnityEngine.Object
    {
        //要通过协同程序去异步加载资源
        MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path, callBack));
    }

    private IEnumerator ReallyLoadAsync<T>(string path, UnityAction<T> callBack) where T : UnityEngine.Object
    {
        //异步加载资源
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        //等待资源加载结束后 才会继续执行yield return后面的代码
        yield return rq;
        //资源加载结束 将资源传到外部的委托函数去进行使用
        callBack(rq.asset as T);
    }

  
    private IEnumerator ReallyLoadAsync(string path, Type type, UnityAction<UnityEngine.Object> callBack)
    {
        //异步加载资源
        ResourceRequest rq = Resources.LoadAsync(path, type);
        //等待资源加载结束后 才会继续执行yield return后面的代码
        yield return rq;
        //资源加载结束 将资源传到外部的委托函数去进行使用
        callBack(rq.asset);
    }

    /// <summary>
    /// 指定卸载一个资源
    /// </summary>
    /// <param name="assetToUnload"></param>
    public void UnloadAsset(UnityEngine.Object assetToUnload)
    {
        Resources.UnloadAsset(assetToUnload);
    }

    /// <summary>
    /// 异步卸载对应没有使用的Resources相关的资源
    /// </summary>
    /// <param name="callBack">回调函数</param>
    public void UnloadUnusedAssets(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callBack));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callBack)
    {
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //卸载完毕后 通知外部
        callBack();
    }

}
