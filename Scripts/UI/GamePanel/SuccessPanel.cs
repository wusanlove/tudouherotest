using UnityEngine;

public class SuccessPanel : BaseMgrMono<SuccessPanel>
{
    //TODO:使用BasePanel基类
    public CanvasGroup _canvasGroup;
    public override void Awake()
    {
        base.Awake();
        this.GetComponent<CanvasGroup>().alpha = 0;
        this.GetComponent<CanvasGroup>().interactable = false;
        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
