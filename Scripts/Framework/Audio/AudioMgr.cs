using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频管理器 —— 统一提供播放、停止、设置音量等操作。
/// <para>
/// 事件驱动（推荐，外部无需关心音频名称）：
/// <code>
/// // 播放 SFX
/// EventCenter.Instance.EventTrigger(E_EventType.Audio_PlaySfx, AudioId.SFX_Attack);
/// // 播放 BGM
/// EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm, AudioId.BGM_Menu);
/// // 停止 BGM
/// EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
/// // 设置音量
/// EventCenter.Instance.EventTrigger(E_EventType.Audio_SetVolume, new AudioVolumeRequest { master=1f, bgm=0.8f, sfx=1f, ui=1f });
/// </code>
/// </para>
/// <para>
/// 直接调用（可指定音量/音调）：
/// <code>
/// AudioMgr.Instance.PlaySfx(AudioId.SFX_Attack, volume: 0.8f, pitch: 1.2f);
/// AudioMgr.Instance.PlayBgm(AudioId.BGM_Game);
/// AudioMgr.Instance.StopBgm();
/// AudioMgr.Instance.SetVolume(master: 1f, bgm: 0.8f, sfx: 1f, ui: 1f);
/// </code>
/// </para>
/// <para>音频配置通过 <see cref="AudioRegistry"/> ScriptableObject 数据驱动，路径：Resources/Audio/AudioRegistry。</para>
/// <para>短音效使用对象池（<see cref="PoolMgr"/>）高效复用 AudioSource。</para>
/// </summary>
public class AudioMgr : BaseMgr<AudioMgr>
{
    private const string RegistryPath = "Audio/AudioRegistry";
    private const string SfxPoolPrefabPath = "Audio/AudioSourcePooled";

    private AudioRegistry registry;

    private GameObject bgmObj;
    private AudioSource bgmSource;
    private float bgmBaseVolume = 1f;

    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float uiVolume = 1f;

    // 简单限音（避免同一帧大量音效轰炸）
    private readonly Dictionary<AudioCategory, int> maxVoices = new Dictionary<AudioCategory, int>
    {
        { AudioCategory.Sfx, 8 },
        { AudioCategory.Ui, 4 }
    };

    private readonly Dictionary<AudioCategory, int> activeVoices = new Dictionary<AudioCategory, int>
    {
        { AudioCategory.Sfx, 0 },
        { AudioCategory.Ui, 0 }
    };

    private AudioMgr()
    {
        EnsureBgmSource();
        LoadRegistry();
        RegisterEvents();
    }

    private void EnsureBgmSource()
    {
        if (bgmObj != null) return;
        bgmObj = new GameObject("AudioMgr_BGM");
        bgmSource = bgmObj.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        Object.DontDestroyOnLoad(bgmObj);
    }

    private void LoadRegistry()
    {
        if (registry != null) return;
        registry = ResMgr.Instance.Load<AudioRegistry>(RegistryPath);
        if (registry == null)
            Debug.LogWarning($"[AudioMgr] AudioRegistry not found at Resources/{RegistryPath}");
    }

    private void RegisterEvents()
    {
        // 事件驱动：外部只需传入 AudioId，无需构造请求结构体
        EventCenter.Instance.AddEventListener<AudioId>(E_EventType.Audio_PlayBgm, id => PlayBgm(id));
        EventCenter.Instance.AddEventListener(E_EventType.Audio_StopBgm, StopBgm);
        EventCenter.Instance.AddEventListener<AudioId>(E_EventType.Audio_PlaySfx, id => PlaySfx(id));
        EventCenter.Instance.AddEventListener<AudioVolumeRequest>(E_EventType.Audio_SetVolume, ApplyVolumeRequest);
    }

    private bool TryGetEntry(AudioId id, out AudioRegistry.Entry entry)
    {
        entry = null;
        if (registry == null || id == AudioId.None) return false;
        return registry.TryGet(id, out entry);
    }

    private float ResolveVolume(AudioCategory category, float baseVolume)
    {
        float cat = category == AudioCategory.Bgm ? bgmVolume : (category == AudioCategory.Ui ? uiVolume : sfxVolume);
        return Mathf.Clamp01(baseVolume) * masterVolume * cat;
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>播放 BGM，音量/循环使用 AudioRegistry 配置。</summary>
    public void PlayBgm(AudioId id)
        => PlayBgm(id, 0f, true);

    /// <summary>播放 BGM，可覆盖音量与循环设置（volume=0 使用注册表默认值）。</summary>
    public void PlayBgm(AudioId id, float volume, bool loop)
    {
        if (!TryGetEntry(id, out var entry) || entry.clip == null) return;

        bgmSource.clip = entry.clip;
        bgmSource.loop = loop || entry.loop;
        bgmBaseVolume = (volume <= 0f ? entry.defaultVolume : volume);
        bgmSource.volume = ResolveVolume(AudioCategory.Bgm, bgmBaseVolume);
        bgmSource.Play();
    }

    /// <summary>停止 BGM。</summary>
    public void StopBgm()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    /// <summary>
    /// 播放音效，音量/音调使用 AudioRegistry 配置。
    /// 内部使用对象池高效复用 AudioSource。
    /// </summary>
    public void PlaySfx(AudioId id)
        => PlaySfx(id, 0f, 0f);

    /// <summary>
    /// 播放音效，可覆盖音量与音调（传 0 表示使用注册表默认值）。
    /// 内部使用对象池高效复用 AudioSource。
    /// </summary>
    public void PlaySfx(AudioId id, float volume, float pitch = 0f)
    {
        if (!TryGetEntry(id, out var entry) || entry.clip == null) return;

        // 若注册为 BGM，走 BGM 通道
        if (entry.category == AudioCategory.Bgm)
        {
            PlayBgm(id, volume, entry.loop);
            return;
        }

        var category = entry.category;
        if (!CanPlay(category)) return;

        // 从对象池获取 AudioSource 节点
        GameObject go = PoolMgr.Instance.GetObj(SfxPoolPrefabPath);
        if (go == null) return;

        var pooled = go.GetComponent<AudioSourcePooled>();
        if (pooled == null) return;

        float baseVol = (volume <= 0f ? entry.defaultVolume : volume);
        float finalPitch = (pitch <= 0f ? entry.defaultPitch : pitch);

        pooled.Play(entry.clip, SfxPoolPrefabPath, ResolveVolume(category, baseVol), finalPitch, entry.loop, 0f);

        // 限音计数回收（loop 时不递减，避免计数错误）
        if (!entry.loop)
            MonoMgr.Instance.StartCoroutine(ReleaseVoiceLater(category, entry.clip.length / Mathf.Max(0.01f, Mathf.Abs(finalPitch))));
    }

    /// <summary>统一设置各通道音量（范围 0~1）。</summary>
    public void SetVolume(float master, float bgm, float sfx, float ui)
        => ApplyVolumeRequest(new AudioVolumeRequest { master = master, bgm = bgm, sfx = sfx, ui = ui });

    // ── 内部辅助 ──────────────────────────────────────────────────────────

    private void ApplyVolumeRequest(AudioVolumeRequest req)
    {
        masterVolume = Mathf.Clamp01(req.master);
        bgmVolume = Mathf.Clamp01(req.bgm);
        sfxVolume = Mathf.Clamp01(req.sfx);
        uiVolume = Mathf.Clamp01(req.ui);

        if (bgmSource != null && bgmSource.clip != null)
            bgmSource.volume = ResolveVolume(AudioCategory.Bgm, bgmBaseVolume);
    }

    private bool CanPlay(AudioCategory category)
    {
        if (!activeVoices.ContainsKey(category)) return true;
        int current = activeVoices[category];
        int limit = maxVoices[category];
        if (current >= limit) return false;
        activeVoices[category] = current + 1;
        return true;
    }

    private IEnumerator ReleaseVoiceLater(AudioCategory category, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeVoices.ContainsKey(category))
            activeVoices[category] = Mathf.Max(0, activeVoices[category] - 1);
    }
}
