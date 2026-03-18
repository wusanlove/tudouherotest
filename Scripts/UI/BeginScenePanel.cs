using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 主菜单面板 —— 只负责显示和抛出按钮事件，不直接操作场景跳转。
/// 场景切换由 EventCenter → GameManager → SceneStateController 统一处理。
/// </summary>
public class BeginScenePanel : BasePanel
{
    [SerializeField] private Image imgBkTop;

    private Button btnStart;
    private Button btnSetting;
    private Button btnProcess;
    private Button btnExit;

    public override void Awake()
    {
        base.Awake();  // 初始化 CanvasGroup（BasePanel.Init）
        btnStart   = GameObject.Find("btnStart")  ?.GetComponent<Button>();
        btnSetting = GameObject.Find("btnSetting")?.GetComponent<Button>();
        btnProcess = GameObject.Find("btnProcess")?.GetComponent<Button>();
        btnExit    = GameObject.Find("btnExit")   ?.GetComponent<Button>();
    }

    private void Start()
    {
        // 通过 EventCenter 通知场景控制器切换，不再直接调用 SceneManager
        btnStart?.onClick.AddListener(() =>
            EventCenter.Instance.EventTrigger(E_EventType.Scene_ToLevelSelect));

        btnExit?.onClick.AddListener(() => Application.Quit());

        // 背景横向滚动动画
        if (imgBkTop != null)
        {
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
}
