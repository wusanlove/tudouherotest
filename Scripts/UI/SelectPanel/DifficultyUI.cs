using UnityEngine;using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
public class DifficultyUI:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public DifficultyData difficultyData;
    public Image _backImage;
    private Image _avatar;
    private Button _button;
    private void Awake()
    {
        
        _backImage = GetComponent<Image>();
        _avatar = transform.GetChild(0).GetComponent<Image>();
        _button = GetComponent<Button>();
    }


    public void SetDifficultyData(DifficultyData difficultyData)
    {
        if (difficultyData != null)
        {
            this.difficultyData = difficultyData;
            SetBackColor(difficultyData.id);
            
            _avatar.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(difficultyData.name);
            
        }
        _button.onClick.AddListener(() =>
        {
            GameManager.Instance.currentDifficulty = difficultyData;
            DifficultySelectPanel.Instance._canvasGroup.alpha = 0f;
            // 通过 EventCenter 触发场景切换，由 GameManager → SceneStateController 处理
            EventCenter.Instance.EventTrigger(E_EventType.Scene_ToGamePlay);
        });
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _backImage.color = new Color(207f / 255f, 207 / 255f, 207 / 255f);
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
            case 1:
                _backImage.color = new Color(33 / 255f, 33 / 255f, 33 / 255f);
                break;
            case 2 :
                _backImage.color = new Color(63 / 255f, 88 / 255f, 104 / 255f);
                break; 
            case 3 :
                _backImage.color = new Color(83 / 255f, 62 / 255f, 103 / 255f);
                break;
            case 4 :
                _backImage.color = new Color(103 / 255f, 54 / 255f, 54 / 255f);
                break;
            case 5 :
                _backImage.color = new Color(103 / 255f, 69 / 255f, 54 / 255f);
                break;
            case 6 :
                _backImage.color = new Color(91 / 255f, 87 / 255f, 55 / 255f);
                break;
        }
    }
}
