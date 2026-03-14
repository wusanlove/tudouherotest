using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public class DifficultySelectPanel:BaseMgrMono<DifficultySelectPanel>
{
    public CanvasGroup _canvasGroup;
    public Transform _difficultyListTransform;
    public Transform _difficultyDetailTransform;
    [SerializeField]private GameObject _difficultyPrefab;
    //难度Detail
    [SerializeField]private TextMeshProUGUI _difficultyName;
    [SerializeField] private Image _difficultyImage;
    [SerializeField] private TextMeshProUGUI _difficultyDescription;
    public GameObject _difficultyDetailGameObject;
    List<DifficultyData> DifficultyDataList;
    public override void Awake()
    {
        base.Awake(); 
        _canvasGroup = GetComponent<CanvasGroup>();
        //给DifficultyDataList赋值
        TextAsset difficultyTextAsset = Resources.Load<TextAsset>("Data/difficulty"); 
        DifficultyDataList = JsonConvert.DeserializeObject<List<DifficultyData>>(difficultyTextAsset.text);
        
        
    }

    private void Start()
    {
        if (DifficultyDataList != null)
        {
            foreach (var it in DifficultyDataList)
            {
                
                DifficultyUI difficultyUI=Instantiate(_difficultyPrefab, _difficultyListTransform).GetComponent<DifficultyUI>();
                difficultyUI.SetDifficultyData(it);

            }
        }
       
        
        
    }

    public void RenewUI(DifficultyData difficultyData)
    {
        this._difficultyName.SetText(difficultyData.name);
        this._difficultyDescription.SetText(difficultyData.describe);
        this._difficultyImage.sprite= Resources.Load<SpriteAtlas>("Image/UI/危险等级").GetSprite(difficultyData.name);

          
    }
}
