using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;

public class DifficultySelectPanel : BaseMgrMono<DifficultySelectPanel>
{
    public CanvasGroup _canvasGroup;
    public Transform _difficultyListTransform;
    public Transform _difficultyDetailTransform;
    [SerializeField] private GameObject _difficultyPrefab;

    [SerializeField] private TextMeshProUGUI _difficultyName;
    [SerializeField] private Image _difficultyImage;
    [SerializeField] private TextMeshProUGUI _difficultyDescription;

    public GameObject _difficultyDetailGameObject;
    private List<DifficultyData> difficultyDataList;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
        // 难度配置数据不在 GameManager 里，通过 ConfigMgr 统一加载
        difficultyDataList = ConfigMgr.Instance.LoadList<DifficultyData>("difficulty");
    }

    private void Start()
    {
        foreach (var it in difficultyDataList)
        {
            DifficultyUI difficultyUI = Instantiate(_difficultyPrefab, _difficultyListTransform).GetComponent<DifficultyUI>();
            difficultyUI.SetDifficultyData(it);
        }
    }

    public void RenewUI(DifficultyData difficultyData)
    {
        _difficultyName.SetText(difficultyData.name);
        _difficultyDescription.SetText(difficultyData.describe);
        _difficultyImage.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(difficultyData.name);
    }
}
