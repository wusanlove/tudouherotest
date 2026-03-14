using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 难度列表项 — 只负责展示与交互。
/// 点击后发 UI_OnDifficultySelected 事件，UIFlowController 统一处理数据保存并触发场景跳转。
/// 不再直接调用 SceneManager。
/// </summary>
public class DifficultyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DifficultyData difficultyData;
    public Image _backImage;
    private Image _avatar;
    private Button _button;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
    }

    public void SetDifficultyData(DifficultyData data)
    {
        if (data == null) return;
        difficultyData = data;
        SetBackColor(data.id);
        _avatar.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(data.name);

        _button.onClick.AddListener(() =>
        {
            // 通知 UIFlowController 处理选难度逻辑（保存数据 + 触发 Flow_GoToGamePlay）
            EventCenter.Instance.EventTrigger(E_EventType.UI_OnDifficultySelected, data);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207f / 255f, 207 / 255f, 207 / 255f);
        DifficultySelectPanel.Instance?.RenewUI(difficultyData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetBackColor(difficultyData.id);
    }

    private void SetBackColor(int id)
    {
        switch (id)
        {
            case 1: _backImage.color = new Color(33 / 255f, 33 / 255f,  33 / 255f);  break;
            case 2: _backImage.color = new Color(63 / 255f, 88 / 255f,  104 / 255f); break;
            case 3: _backImage.color = new Color(83 / 255f, 62 / 255f,  103 / 255f); break;
            case 4: _backImage.color = new Color(103 / 255f, 54 / 255f, 54 / 255f);  break;
            case 5: _backImage.color = new Color(103 / 255f, 69 / 255f, 54 / 255f);  break;
            case 6: _backImage.color = new Color(91 / 255f,  87 / 255f, 55 / 255f);  break;
        }
    }
}
