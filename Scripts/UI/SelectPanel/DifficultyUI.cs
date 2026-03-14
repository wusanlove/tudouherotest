using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 单个难度 Item（View）：点击后触发 Select_DifficultyChosen 事件。
/// </summary>
public class DifficultyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DifficultyData difficultyData;
    public Image _backImage;
    private Image  _avatar;
    private Button _button;

    private void Awake()
    {
        _backImage = GetComponent<Image>();
        _avatar    = transform.GetChild(0).GetComponent<Image>();
        _button    = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    public void SetDifficultyData(DifficultyData data)
    {
        this.difficultyData = data;
        if (data != null)
        {
            SetBackColor(data.id);
            _avatar.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(data.name);
        }
    }

    private void OnClick()
        => EventCenter.Instance.EventTrigger(E_EventType.Select_DifficultyChosen, difficultyData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207f/255f, 207/255f, 207/255f);
        DifficultySelectPanel.Instance.RenewUI(difficultyData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetBackColor(difficultyData.id);
    }

    private void SetBackColor(int id)
    {
        switch (id)
        {
            case 1: _backImage.color = new Color(33/255f,  33/255f,  33/255f);  break;
            case 2: _backImage.color = new Color(63/255f,  88/255f,  104/255f); break;
            case 3: _backImage.color = new Color(83/255f,  62/255f,  103/255f); break;
            case 4: _backImage.color = new Color(103/255f, 54/255f,  54/255f);  break;
            case 5: _backImage.color = new Color(103/255f, 69/255f,  54/255f);  break;
            case 6: _backImage.color = new Color(91/255f,  87/255f,  55/255f);  break;
        }
    }
}

    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207f/255f, 207/255f, 207/255f);
        DifficultySelectPanel.Instance.RenewUI(difficultyData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetBackColor(difficultyData.id);
    }

    private void SetBackColor(int id)
    {
        switch (id)
        {
            case 1: _backImage.color = new Color(33/255f,  33/255f,  33/255f);  break;
            case 2: _backImage.color = new Color(63/255f,  88/255f,  104/255f); break;
            case 3: _backImage.color = new Color(83/255f,  62/255f,  103/255f); break;
            case 4: _backImage.color = new Color(103/255f, 54/255f,  54/255f);  break;
            case 5: _backImage.color = new Color(103/255f, 69/255f,  54/255f);  break;
            case 6: _backImage.color = new Color(91/255f,  87/255f,  55/255f);  break;
        }
    }
}
