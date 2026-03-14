using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
public class BeginScenePanel :BasePanel
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        btnStart.onClick.AddListener(()=>
        {
            //试着有异步加载场景
            SceneManager.LoadScene("02-LevelSelect");
        });
        btnStart.onClick.AddListener(()=>
        {
            
        });
        btnStart.onClick.AddListener(()=>
        {
            
        });
        btnStart.onClick.AddListener(()=>
        {
            
        });
        if (imgBkTop != null)
        {
            var rt = imgBkTop.rectTransform;

            // Move by delta X (relative), not move to a target position.
            // Use Canvas width (UI units) instead of Screen.width (pixels).
            float canvasWidth = rt.GetComponentInParent<Canvas>()?.rootCanvas?.GetComponent<RectTransform>()?.rect.width ?? Screen.width;
            float moveAmountX = rt.rect.width - canvasWidth;

            // If your DOTween version doesn't include DOAnchorPosX, use DOAnchorPos with (start + delta).
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(moveAmountX, 0f);
            rt.DOAnchorPos(endPos, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
        }
    }


}
