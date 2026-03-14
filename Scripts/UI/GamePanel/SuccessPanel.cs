using UnityEngine;

/// <summary>
/// 胜利面板（View 层）。
/// 监听 <see cref="E_EventType.Battle_AllWavesCompleted"/> 事件后显示自身，
/// 不再需要 LevelControl 直接持有并操作此面板的 CanvasGroup。
/// </summary>
public class SuccessPanel : BaseMgrMono<SuccessPanel>
{
    private CanvasGroup _canvasGroup;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false);
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(E_EventType.Battle_AllWavesCompleted, Show);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(E_EventType.Battle_AllWavesCompleted, Show);
    }

    private void Show()         => SetVisible(true);
    private void SetVisible(bool v)
    {
        _canvasGroup.alpha          = v ? 1 : 0;
        _canvasGroup.interactable   = v;
        _canvasGroup.blocksRaycasts = v;
    }
}
