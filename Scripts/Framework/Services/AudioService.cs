/// <summary>
/// 音效服务 – AudioMgr 的简化 Facade，提供语义清晰的 API。
/// 内部通过 EventCenter 向 AudioMgr 发送请求，调用方无需了解事件参数结构。
/// </summary>
public class AudioService : BaseMgr<AudioService>
{
    private AudioService() { }

    // ──────────────── BGM ────────────────

    /// <summary>播放背景音乐。key 对应 AudioRegistry 中的条目键名。</summary>
    public void PlayBgm(string key, float volume = 0f, bool loop = true)
    {
        EventCenter.Instance.EventTrigger(E_EventType.Audio_PlayBgm,
            new AudioBgmRequest { key = key, volume = volume, loop = loop });
    }

    /// <summary>停止当前背景音乐。</summary>
    public void StopBgm()
    {
        EventCenter.Instance.EventTrigger(E_EventType.Audio_StopBgm);
    }

    // ──────────────── SFX ────────────────

    /// <summary>播放音效。key 对应 AudioRegistry 中的条目键名。</summary>
    public void PlaySfx(string key, float volume = 0f, float pitch = 0f)
    {
        EventCenter.Instance.EventTrigger(E_EventType.Audio_PlaySfx,
            new AudioPlayRequest { key = key, volume = volume, pitch = pitch });
    }

    // ──────────────── 常用音效快捷方式 ────────────────

    public void PlayMenuBgm()   => PlayBgm("menu_bgm");
    public void PlayAttackSfx() => PlaySfx("attack_sfx");
    public void PlayShootSfx()  => PlaySfx("shoot_sfx");
    public void PlayHurtSfx()   => PlaySfx("hurt_sfx");

    // ──────────────── 音量控制 ────────────────

    /// <summary>设置全局音量（master/bgm/sfx/ui 均为 0~1）。</summary>
    public void SetVolume(float master, float bgm = 1f, float sfx = 1f, float ui = 1f)
    {
        EventCenter.Instance.EventTrigger(E_EventType.Audio_SetVolume,
            new AudioVolumeRequest { master = master, bgm = bgm, sfx = sfx, ui = ui });
    }
}
