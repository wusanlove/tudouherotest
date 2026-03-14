using System.Collections.Generic;
using UnityEngine;

public class AudioMgr : BaseMgr<AudioMgr>
{
    private const string RegistryPath = "Audio/AudioRegistry";
    private const string SfxPoolPrefabPath = "Audio/AudioSourcePooled";

    private AudioRegistry registry;

    private AudioSource bgmSource;
    private GameObject bgmObj;

    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float uiVolume = 1f;

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
        bgmObj = new GameObject("BGM_Source");
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
            Debug.LogWarning($"AudioRegistry not found at Resources/{RegistryPath}");
    }

    private void RegisterEvents()
    {
        EventCenter.Instance.AddEventListener<AudioPlayRequest>(E_EventType.Audio_PlaySfx, OnPlaySfx);
        EventCenter.Instance.AddEventListener<AudioBgmRequest>(E_EventType.Audio_PlayBgm, OnPlayBgm);
        EventCenter.Instance.AddEventListener(E_EventType.Audio_StopBgm, OnStopBgm);
        EventCenter.Instance.AddEventListener<AudioVolumeRequest>(E_EventType.Audio_SetVolume, OnSetVolume);
    }

    private bool TryGetEntry(string key, out AudioRegistry.Entry entry)
    {
        entry = null;
        if (registry == null) return false;
        return registry.TryGet(key, out entry);
    }

    private float ResolveVolume(AudioCategory category, float baseVolume)
    {
        float categoryVol = category == AudioCategory.Bgm ? bgmVolume : (category == AudioCategory.Ui ? uiVolume : sfxVolume);
        return Mathf.Clamp01(baseVolume) * masterVolume * categoryVol;
    }

    private void OnPlayBgm(AudioBgmRequest req)
    {
        if (!TryGetEntry(req.key, out AudioRegistry.Entry entry) || entry.clip == null)
            return;

        bgmSource.clip = entry.clip;
        bgmSource.loop = req.loop || entry.loop;
        bgmSource.volume = ResolveVolume(AudioCategory.Bgm, req.volume <= 0f ? entry.defaultVolume : req.volume);
        bgmSource.Play();
    }

    private void OnStopBgm()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    private void OnSetVolume(AudioVolumeRequest req)
    {
        masterVolume = Mathf.Clamp01(req.master);
        bgmVolume = Mathf.Clamp01(req.bgm);
        sfxVolume = Mathf.Clamp01(req.sfx);
        uiVolume = Mathf.Clamp01(req.ui);

        if (bgmSource != null && bgmSource.clip != null)
            bgmSource.volume = ResolveVolume(AudioCategory.Bgm, bgmSource.volume);
    }

    private void OnPlaySfx(AudioPlayRequest req)
    {
        if (!TryGetEntry(req.key, out AudioRegistry.Entry entry) || entry.clip == null)
            return;

        AudioCategory category = entry.category;
        if (category == AudioCategory.Bgm)
        {
            OnPlayBgm(new AudioBgmRequest { key = req.key, volume = req.volume, loop = entry.loop });
            return;
        }

        if (!CanPlay(category))
            return;

        GameObject go = PoolMgr.Instance.GetObj(SfxPoolPrefabPath);
        AudioSourcePooled pooled = go.GetComponent<AudioSourcePooled>();
        if (pooled == null)
            return;

        if (req.usePosition)
            go.transform.position = req.position;

        float vol = req.volume <= 0f ? entry.defaultVolume : req.volume;
        float pitch = Mathf.Abs(req.pitch) > 0.001f ? req.pitch : entry.defaultPitch;

        pooled.Play(entry.clip, SfxPoolPrefabPath, ResolveVolume(category, vol), pitch, entry.loop, req.usePosition ? 1f : 0f);
        MonoMgr.Instance.StartCoroutine(ReleaseVoiceLater(category, entry.clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch))));
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

    private System.Collections.IEnumerator ReleaseVoiceLater(AudioCategory category, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeVoices.ContainsKey(category))
            activeVoices[category] = Mathf.Max(0, activeVoices[category] - 1);
    }
}

