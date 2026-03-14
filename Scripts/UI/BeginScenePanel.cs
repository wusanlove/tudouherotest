using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 主菜单面板（View only）：只负责展示和抛出事件。
/// 不直接加载场景；通过 GameFlowController 完成流程跳转。
/// </summary>
public class BeginScenePanel : BasePanel
{
    [SerializeField] private Image imgBkTop;
    private Button btnStart;
    private Button btnSetting;
    private Button btnProcess;
    private Button btnExit;

    void Awake()
    {
        btnStart   = GameObject.Find("btnStart")  ?.GetComponent<Button>();
        btnSetting = GameObject.Find("btnSetting")?.GetComponent<Button>();
        btnProcess = GameObject.Find("btnProcess")?.GetComponent<Button>();
        btnExit    = GameObject.Find("btnExit")   ?.GetComponent<Button>();
    }

    void Start()
    {
        btnStart?.onClick.AddListener(OnClickStart);
        StartBkAnimation();
    }

    private void OnClickStart()
    {
        // 通知流程控制器跳转到选择界面，不在 View 层直接调 SceneManager
        GameFlowController.Instance.GoToLevelSelect();
    }

    private void StartBkAnimation()
    {
        if (imgBkTop == null) return;
        var rt = imgBkTop.rectTransform;
        float canvasWidth = rt.GetComponentInParent<Canvas>()?.rootCanvas
            ?.GetComponent<RectTransform>()?.rect.width ?? Screen.width;
        float moveAmountX = rt.rect.width - canvasWidth;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos   = startPos + new Vector2(moveAmountX, 0f);
        rt.DOAnchorPos(endPos, 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
