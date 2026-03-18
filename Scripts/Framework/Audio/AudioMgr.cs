using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        EventCenter.Instance.AddEventListener<AudioBgmRequest>(E_EventType.Audio_PlayBgm, OnPlayBgm);
        EventCenter.Instance.AddEventListener(E_EventType.Audio_StopBgm, StopBgm);

        EventCenter.Instance.AddEventListener<AudioPlayRequest>(E_EventType.Audio_PlaySfx, OnPlaySfx);
        EventCenter.Instance.AddEventListener<AudioVolumeRequest>(E_EventType.Audio_SetVolume, SetVolume);
    }

    private bool TryGetEntry(string key, out AudioRegistry.Entry entry)
    {
        entry = null;
        if (registry == null || string.IsNullOrEmpty(key)) return false;
        return registry.TryGet(key, out entry);
    }

    private float ResolveVolume(AudioCategory category, float baseVolume)
    {
        float cat = category == AudioCategory.Bgm ? bgmVolume : (category == AudioCategory.Ui ? uiVolume : sfxVolume);
        return Mathf.Clamp01(baseVolume) * masterVolume * cat;
    }

    // ── Public API（可直接调用，也可走 EventCenter）──────────────────────

    public void PlayBgm(string key, float volume = 1f, bool loop = true)
        => OnPlayBgm(new AudioBgmRequest { key = key, volume = volume, loop = loop });

    public void StopBgm()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySfx(string key, float volume = 1f, float pitch = 1f, AudioCategory forceCategory = AudioCategory.Sfx)
    {
        OnPlaySfx(new AudioPlayRequest
        {
            key = key,
            volume = volume,
            pitch = pitch,
            // 2D：不需要位置信息
            usePosition = false,
            position = Vector3.zero,
            priority = 0
        });
    }

    public void SetVolume(float master, float bgm, float sfx, float ui)
    {
        var req = new AudioVolumeRequest { master = master, bgm = bgm, sfx = sfx, ui = ui };
        SetVolume(req);
    }

    // ── Event handlers ──────────────────────────────────────────────────

    private void OnPlayBgm(AudioBgmRequest req)
    {
        if (!TryGetEntry(req.key, out var entry) || entry.clip == null)
            return;

        bgmSource.clip = entry.clip;
        bgmSource.loop = req.loop || entry.loop;

        bgmBaseVolume = (req.volume <= 0f ? entry.defaultVolume : req.volume);
        bgmSource.volume = ResolveVolume(AudioCategory.Bgm, bgmBaseVolume);
        bgmSource.Play();
    }

    private void OnPlaySfx(AudioPlayRequest req)
    {
        if (!TryGetEntry(req.key, out var entry) || entry.clip == null)
            return;

        // 统一：BGM 走 BGM 通道
        if (entry.category == AudioCategory.Bgm)
        {
            OnPlayBgm(new AudioBgmRequest { key = req.key, volume = req.volume, loop = entry.loop });
            return;
        }

        var category = entry.category;
        if (!CanPlay(category)) return;

        GameObject go = PoolMgr.Instance.GetObj(SfxPoolPrefabPath);
        if (go == null) return;

        var pooled = go.GetComponent<AudioSourcePooled>();
        if (pooled == null) return;

        float baseVol = (req.volume <= 0f ? entry.defaultVolume : req.volume);
        float pitch = (Mathf.Abs(req.pitch) > 0.001f ? req.pitch : entry.defaultPitch);

        // 2D：spatialBlend = 0，不做距离衰减
        pooled.Play(entry.clip, SfxPoolPrefabPath, ResolveVolume(category, baseVol), pitch, entry.loop, 0f);

        // 计数回收（loop 的情况下不递减，避免计数错误）
        if (!entry.loop)
            MonoMgr.Instance.StartCoroutine(ReleaseVoiceLater(category, entry.clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch))));
    }

    private void SetVolume(AudioVolumeRequest req)
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
