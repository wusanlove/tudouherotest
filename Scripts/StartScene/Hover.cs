using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverScaleTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tween")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float duration = 0.12f;
    [SerializeField] private Ease easeEnter = Ease.OutBack;
    [SerializeField] private Ease easeExit = Ease.OutQuad;

    private Vector3 _baseScale;
    private Tween _tween;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 这里 eventData 通常可不用；需要时可用 eventData.position 做 tooltip 跟随等
        PlayScale(_baseScale * hoverScale, easeEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScale(_baseScale, easeExit);
    }

    private void OnDisable()
    {
        // 防止对象被禁用时卡在放大状态
        _tween?.Kill();
        transform.localScale = _baseScale;
    }

    private void PlayScale(Vector3 target, Ease ease)
    {
        _tween?.Kill(); // 避免快速进出导致叠加/抖动
        _tween = transform.DOScale(target, duration).SetEase(ease);
        // 通过 AudioMgr 播放悬停音效（key "hover" 在 AudioRegistry 中配置）
        EventCenter.Instance.EventTrigger<AudioPlayRequest>(
            E_EventType.Audio_PlaySfx, new AudioPlayRequest { key = "hover" });
    }
}
