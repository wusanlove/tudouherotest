using System.Collections.Generic;
using UnityEngine;

public class UIMgr : BaseMgr<UIMgr>
{
    
    public Dictionary<string, BasePanel> dicUI = new Dictionary<string, BasePanel>();
    private UIMgr()
    {
        
    }
    public T ShowPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (dicUI.ContainsKey(name))
        {
            Debug.Log("show yes");
            dicUI[name].ShowPanel();
            return dicUI[name] as T;
        }
        else
        {
            Debug.Log("show no");
            GameObject gameObjectUI = GameObject.Instantiate(Resources.Load<GameObject>($"Prefabs/UI/{name}"));
            gameObjectUI.transform.SetParent(GameObject.Find("Canvas").transform, false);
            T panel = gameObjectUI.GetComponent<T>();
            dicUI.Add(name, panel);

            return panel;
        }
    }
    public void HidePanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (dicUI.ContainsKey(name))
        {
            dicUI[name].HidePanel(null);
        }

    }
    public T GetPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (dicUI.ContainsKey(name))
        {
            return dicUI[name] as T;
        }
        return null;
    }
    public void DestroyDic()
    {
        dicUI.Clear();
    }
    //teset
}
