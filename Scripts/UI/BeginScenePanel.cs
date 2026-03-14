using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 主菜单面板（View 层）。
/// 职责：
/// - 控制自身 UI 动画（背景滚动）。
/// - 将按钮点击转换为 EventCenter 事件，不直接操作其他系统。
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
        btnStart   = GameObject.Find("btnStart")?.GetComponent<Button>();
        btnSetting = GameObject.Find("btnSetting")?.GetComponent<Button>();
        btnProcess = GameObject.Find("btnProcess")?.GetComponent<Button>();
        btnExit    = GameObject.Find("btnExit")?.GetComponent<Button>();
    }

    void Start()
    {
        // 点击"开始游戏"：通过 EventCenter 通知 UIFlowController 切换场景
        btnStart?.onClick.AddListener(() =>
            EventCenter.Instance.EventTrigger(E_EventType.UI_StartGame));

        // 点击"设置"（预留）
        btnSetting?.onClick.AddListener(() =>
            EventCenter.Instance.EventTrigger(E_EventType.UI_OpenSettings));

        // 背景图片横向循环滚动动画
        if (imgBkTop != null)
        {
            var rt = imgBkTop.rectTransform;
            float canvasWidth = rt.GetComponentInParent<Canvas>()?.rootCanvas
                                   ?.GetComponent<RectTransform>()?.rect.width
                                ?? Screen.width;
            float moveAmountX = rt.rect.width - canvasWidth;
            Vector2 startPos  = rt.anchoredPosition;
            Vector2 endPos    = startPos + new Vector2(moveAmountX, 0f);
            rt.DOAnchorPos(endPos, 2f)
              .SetLoops(-1, LoopType.Yoyo)
              .SetEase(Ease.InOutSine);
        }
    }
}
