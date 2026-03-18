using UnityEngine;

public enum AudioCategory
{
    Bgm,
    Sfx,
    Ui
}

/// <summary>
/// 带参数的音效播放请求（用于 AudioMgr 直接调用 API，需要自定义音量/音调时使用）。
/// 通过事件触发时，优先直接传入 <see cref="AudioId"/> 即可，无需构造此结构体。
/// </summary>
public struct AudioPlayRequest
{
    public AudioId id;
    public float volume;    // 0 表示使用 AudioRegistry 中的默认值
    public float pitch;     // 0 表示使用 AudioRegistry 中的默认值
    public Vector3 position;
    public bool usePosition;
    public int priority;
}

/// <summary>
/// 带参数的 BGM 播放请求（用于 AudioMgr 直接调用 API）。
/// 通过事件触发时，优先直接传入 <see cref="AudioId"/> 即可，无需构造此结构体。
/// </summary>
public struct AudioBgmRequest
{
    public AudioId id;
    public float volume;    // 0 表示使用 AudioRegistry 中的默认值
    public bool loop;
}

public struct AudioVolumeRequest
{
    public float master;
    public float bgm;
    public float sfx;
    public float ui;
}
