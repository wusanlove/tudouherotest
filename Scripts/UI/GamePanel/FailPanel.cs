using UnityEngine;

public class FailPanel : BaseMgrMono<FailPanel>
{
    public CanvasGroup _canvasGroup;
   
   public override void Awake()
   {
      base.Awake();
      this.GetComponent<CanvasGroup>().alpha = 0;
      this.GetComponent<CanvasGroup>().interactable = false;
      this.GetComponent<CanvasGroup>().blocksRaycasts = false;
   }
}