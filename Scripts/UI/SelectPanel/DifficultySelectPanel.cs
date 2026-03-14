using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 难度选择面板（View）：展示难度列表；用户点击后触发 Select_DifficultyChosen 事件。
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
        foreach (DifficultyData it in ConfigService.Instance.Difficulties)
        {
            DifficultyUI ui = Instantiate(_difficultyPrefab, _difficultyListTransform).GetComponent<DifficultyUI>();
            ui.SetDifficultyData(it);
        }
    }

    /// <summary>鼠标悬停时更新详情区域（由 DifficultyUI 调用）</summary>
    public void RenewUI(DifficultyData difficultyData)
    {
        _difficultyName.SetText(difficultyData.name);
        _difficultyDescription.SetText(difficultyData.describe);
        _difficultyImage.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(difficultyData.name);
    }
}
