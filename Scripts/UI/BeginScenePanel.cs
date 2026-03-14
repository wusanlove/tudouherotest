using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BeginScenePanel :BasePanel
{
    [SerializeField]private Image imgBkTop;
    private Button btnStart;
    private Button btnSetting;
    private Button btnProcess;
    private Button btnExit;
    void Awake()
    {
        btnStart=GameObject.Find("btnStart").GetComponent<Button>();
        btnSetting=GameObject.Find("btnSetting").GetComponent<Button>();
        btnProcess=GameObject.Find("btnProcess").GetComponent<Button>();
        btnExit=GameObject.Find("btnExit").GetComponent<Button>();
    }
    void Start()
    {
        //通过状态机跳转到选择场景，避免直接调用 SceneManager
        btnStart.onClick.AddListener(() => GameStateMachine.Instance.EnterSelect());

        if (imgBkTop != null)
        {
            var rt = imgBkTop.rectTransform;
            float canvasWidth = rt.GetComponentInParent<Canvas>()?.rootCanvas?.GetComponent<RectTransform>()?.rect.width ?? Screen.width;
            float moveAmountX = rt.rect.width - canvasWidth;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(moveAmountX, 0f);
            rt.DOAnchorPos(endPos, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
        }
    }
}
