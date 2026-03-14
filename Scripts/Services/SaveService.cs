using UnityEngine;

/// <summary>
/// PlayerPrefs 存档实现。
/// 实现 ISaveService 接口，与 GameManager 解耦；
/// 后续可换成 JSON 文件或云存档，只需实现同一接口并在 GameFlowController 注册即可。
/// </summary>
public class SaveService : BaseMgr<SaveService>, ISaveService
{
    private const string UnlockPrefix  = "Unlock_";
    private const string RecordPrefix  = "Record_";

    private SaveService() { }

    public bool IsRoleUnlocked(string roleName)
        => PlayerPrefs.GetInt(UnlockPrefix + roleName, 0) == 1;

    public void UnlockRole(string roleName)
    {
        PlayerPrefs.SetInt(UnlockPrefix + roleName, 1);
        PlayerPrefs.Save();
    }

    public int GetBestRecord(string roleName)
        => PlayerPrefs.GetInt(RecordPrefix + roleName, -1);

    public void UpdateRecord(string roleName, int difficultyId)
    {
        int current = GetBestRecord(roleName);
        if (difficultyId > current)
        {
            PlayerPrefs.SetInt(RecordPrefix + roleName, difficultyId);
            PlayerPrefs.Save();
        }
    }

    // 占位；当前 PlayerPrefs 自动保存，此处留空供扩展
    public void Save() { PlayerPrefs.Save(); }
    public void Load() { /* PlayerPrefs 自动加载 */ }
}
