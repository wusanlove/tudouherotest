using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 主菜单面板 — 只负责视觉展示与用户交互。
/// 点击事件通过 EventCenter 抛出，由 UIFlowController → SceneFlowController 统一处理场景跳转。
/// 面板自身不调用 SceneManager，不引用其他 Panel。
/// </summary>
public class BeginScenePanel : BasePanel
{
    [SerializeField] private Image imgBkTop;

    private Button btnStart;
    private Button btnSetting;
    private Button btnProcess;
    private Button btnExit;

    private void Awake()
    {
        btnStart   = GameObject.Find("btnStart")?.GetComponent<Button>();
        btnSetting = GameObject.Find("btnSetting")?.GetComponent<Button>();
        btnProcess = GameObject.Find("btnProcess")?.GetComponent<Button>();
        btnExit    = GameObject.Find("btnExit")?.GetComponent<Button>();
    }

    private void Start()
    {
        // 点击开始：发事件 → UIFlowController.OnStartGameClicked → Flow_GoToLevelSelect
        // 不直接调用 SceneManager，保持 UI 层与场景流程解耦
        btnStart?.onClick.AddListener(() =>
            EventCenter.Instance.EventTrigger(E_EventType.UI_OnStartGameClicked));

        // 背景图片左右循环动画
        if (imgBkTop != null)
        {
            RectTransform rt = imgBkTop.rectTransform;
            float canvasWidth = rt.GetComponentInParent<Canvas>()?.rootCanvas
                ?.GetComponent<RectTransform>()?.rect.width ?? Screen.width;
            float moveAmountX = rt.rect.width - canvasWidth;
            Vector2 startPos  = rt.anchoredPosition;
            Vector2 endPos    = startPos + new Vector2(moveAmountX, 0f);
            rt.DOAnchorPos(endPos, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}
