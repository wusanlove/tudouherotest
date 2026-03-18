using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseMgrMono<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    //挂载的基类 注意 1：重复挂载 2：不用考虑多线程 因为 Monobehaviorunity中的物体在主线程中 无法 访问  3 也无法被多个new

    private static T instance;
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    public virtual void Awake()
    {

        if (instance != null) {
            Destroy(this); 
            return;
        }
        instance = this as T;
        //DontDestroyOnLoad(gameObject); // 可选：跨场景持久化
        
    }
}

