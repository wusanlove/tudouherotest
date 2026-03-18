using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频注册表 —— 数据驱动 ScriptableObject。
/// 在 Inspector 中配置每个 <see cref="AudioId"/> 对应的 AudioClip 及播放参数。
/// 路径：Resources/Audio/AudioRegistry（由 AudioMgr 自动加载）。
/// </summary>
[CreateAssetMenu(menuName = "Audio/AudioRegistry", fileName = "AudioRegistry")]
public class AudioRegistry : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public AudioId id;
        public AudioClip clip;
        public AudioCategory category = AudioCategory.Sfx;
        [Range(0f, 1f)] public float defaultVolume = 1f;
        [Range(-3f, 3f)] public float defaultPitch = 1f;
        [Range(0, 256)] public int priority = 128;
        public bool loop;
    }

    public List<Entry> entries = new List<Entry>();

    private Dictionary<AudioId, Entry> lookup;

    public void BuildLookup()
    {
        if (lookup != null) return;
        lookup = new Dictionary<AudioId, Entry>(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry == null || entry.id == AudioId.None)
                continue;
            lookup[entry.id] = entry;
        }
    }

    public bool TryGet(AudioId id, out Entry entry)
    {
        BuildLookup();
        return lookup.TryGetValue(id, out entry);
    }

    /// <summary>编辑器下修改条目后调用，强制重建查找表。</summary>
    private void OnValidate()
    {
        lookup = null;
    }
}
