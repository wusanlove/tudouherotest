using UnityEngine;

public enum AudioCategory
{
    Bgm,
    Sfx,
    Ui
}

public struct AudioPlayRequest
{
    public string key;
    public Vector3 position;
    public bool usePosition;
    public float volume;
    public float pitch;
    public int priority;
}

public struct AudioBgmRequest
{
    public string key;
    public float volume;
    public bool loop;
}

public struct AudioVolumeRequest
{
    public float master;
    public float bgm;
    public float sfx;
    public float ui;
}
