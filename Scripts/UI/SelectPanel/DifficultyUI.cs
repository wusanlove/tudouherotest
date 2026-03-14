using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 难度列表中的单个难度卡。
/// 点击后存储难度数据并触发 Scene_GoToGame 事件，
/// 由 GameManager 中的 SceneStateController 统一执行场景加载。
/// </summary>
public class DifficultyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DifficultyData difficultyData;
    public Image          _backImage;
    private Image         _avatar;
    private Button        _button;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
    }

    public void SetDifficultyData(DifficultyData data)
    {
        difficultyData = data;
        SetBackColor(data.id);
        _avatar.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(data.name);

        _button.onClick.AddListener(() =>
        {
            GameManager.Instance.currentDifficulty = difficultyData;
            // 不直接加载场景，通过 EventCenter 让 SceneStateController 处理
            EventCenter.Instance.EventTrigger(E_EventType.Scene_GoToGame);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207 / 255f, 207 / 255f, 207 / 255f);
        DifficultySelectPanel.Instance.RenewUI(difficultyData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetBackColor(difficultyData.id);
    }

    private void SetBackColor(int id)
    {
        _backImage.color = id switch
        {
            1 => new Color(33  / 255f, 33  / 255f, 33  / 255f),
            2 => new Color(63  / 255f, 88  / 255f, 104 / 255f),
            3 => new Color(83  / 255f, 62  / 255f, 103 / 255f),
            4 => new Color(103 / 255f, 54  / 255f, 54  / 255f),
            5 => new Color(103 / 255f, 69  / 255f, 54  / 255f),
            6 => new Color(91  / 255f, 87  / 255f, 55  / 255f),
            _ => _backImage.color,
        };
    }
}
