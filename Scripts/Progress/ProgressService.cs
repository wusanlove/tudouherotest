/// <summary>
/// 进度服务门面（Facade）：提供全局唯一入口，内部持有 <see cref="IProgressSystem"/> 实现。
/// 调用方通过 ProgressService.Instance.xxx() 访问，无需关心具体实现类。
///
/// 切换实现示例（如需云存档）：
///   ProgressService.Instance.SetImplementation(new CloudProgressService());
/// </summary>
public class ProgressService : BaseMgr<ProgressService>
{
    private IProgressSystem _impl;

    private ProgressService()
    {
        _impl = new PlayerPrefsProgressService();
    }

    /// <summary>替换底层实现（如从 PlayerPrefs 切换到云存档）。</summary>
    public void SetImplementation(IProgressSystem impl)
    {
        _impl = impl ?? new PlayerPrefsProgressService();
    }

    // ── 委托到实现 ───────────────────────────────────────────────────────────
    public void Load()                                                       => _impl.Load();
    public void Save()                                                       => _impl.Save();
    public bool IsRoleUnlocked(string roleName)                              => _impl.IsRoleUnlocked(roleName);
    public void UnlockRole(string roleName)                                  => _impl.UnlockRole(roleName);
    public void OnGameCompleted(string role, string diff, int wavesCleared)  => _impl.OnGameCompleted(role, diff, wavesCleared);
}
