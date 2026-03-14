using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 难度选择面板（在 02-LevelSelect 场景中）。
/// 使用 GameManager 中已加载的 difficultyDatas，不再重复读 JSON。
/// </summary>
public class DifficultySelectPanel : BaseMgrMono<DifficultySelectPanel>
{
    public  CanvasGroup _canvasGroup;
    public  Transform   _difficultyListTransform;
    public  Transform   _difficultyDetailTransform;
    public  GameObject  _difficultyDetailGameObject;

    [SerializeField] private GameObject      _difficultyPrefab;
    [SerializeField] private TextMeshProUGUI _difficultyName;
    [SerializeField] private Image           _difficultyImage;
    [SerializeField] private TextMeshProUGUI _difficultyDescription;

    public override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();

        // 使用 GameManager 中已集中加载的难度数据
        foreach (DifficultyData data in GameManager.Instance.difficultyDatas)
        {
            DifficultyUI ui = Instantiate(_difficultyPrefab, _difficultyListTransform).GetComponent<DifficultyUI>();
            ui.SetDifficultyData(data);
        }
    }

    /// <summary>悬停时刷新右侧难度详情区。</summary>
    public void RenewUI(DifficultyData difficultyData)
    {
        _difficultyName.SetText(difficultyData.name);
        _difficultyDescription.SetText(difficultyData.describe);
        _difficultyImage.sprite = Resources.Load<SpriteAtlas>("Image/UI/危险等级")
            .GetSprite(difficultyData.name);
    }
}
