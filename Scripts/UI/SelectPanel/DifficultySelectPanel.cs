using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 难度选择面板 — 展示难度列表与详情。
/// 数据来自 ConfigService（不再自行加载 JSON）。
/// </summary>
public class DifficultySelectPanel : BaseMgrMono<DifficultySelectPanel>
{
    public CanvasGroup _canvasGroup;
    public Transform   _difficultyListTransform;
    public Transform   _difficultyDetailTransform;

    [SerializeField] private GameObject      _difficultyPrefab;
    [SerializeField] private TextMeshProUGUI _difficultyName;
    [SerializeField] private Image           _difficultyImage;
    [SerializeField] private TextMeshProUGUI _difficultyDescription;

    public GameObject _difficultyDetailGameObject;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        List<DifficultyData> difficulties = ConfigService.Instance.Difficulties;
        if (difficulties == null) return;
        foreach (DifficultyData data in difficulties)
        {
            DifficultyUI ui = Instantiate(_difficultyPrefab, _difficultyListTransform).GetComponent<DifficultyUI>();
            ui.SetDifficultyData(data);
        }
    }

    /// <summary>刷新右侧难度详情区。</summary>
    public void RenewUI(DifficultyData difficultyData)
    {
        _difficultyName.SetText(difficultyData.name);
        _difficultyDescription.SetText(difficultyData.describe);
        _difficultyImage.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(difficultyData.name);
    }
}
