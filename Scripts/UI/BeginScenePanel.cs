using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 主菜单面板。
/// 按钮事件通过 EventCenter 发布，不直接调用 SceneManager，
/// 保持 UI 层与场景切换逻辑解耦。
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
        btnStart   = GameObject.Find("btnStart").GetComponent<Button>();
        btnSetting = GameObject.Find("btnSetting").GetComponent<Button>();
        btnProcess = GameObject.Find("btnProcess").GetComponent<Button>();
        btnExit    = GameObject.Find("btnExit").GetComponent<Button>();
    }

    void Start()
    {
        // 触发事件，由 GameManager 中的 SceneStateController 统一处理场景切换
        btnStart.onClick.AddListener(() =>
            EventCenter.Instance.EventTrigger(E_EventType.Scene_GoToSelect));

        // 背景视差动画
        if (imgBkTop != null)
        {
            var rt = imgBkTop.rectTransform;
            float canvasWidth = rt.GetComponentInParent<Canvas>()?.rootCanvas?
                .GetComponent<RectTransform>()?.rect.width ?? Screen.width;
            float moveAmountX = rt.rect.width - canvasWidth;
            Vector2 startPos  = rt.anchoredPosition;
            Vector2 endPos    = startPos + new Vector2(moveAmountX, 0f);

            rt.DOAnchorPos(endPos, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}
