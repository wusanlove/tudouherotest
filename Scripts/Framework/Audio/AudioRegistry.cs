using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioRegistry", fileName = "AudioRegistry")]
public class AudioRegistry : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public AudioClip clip;
        public AudioCategory category = AudioCategory.Sfx;
        [Range(0f, 1f)] public float defaultVolume = 1f;
        [Range(-3f, 3f)] public float defaultPitch = 1f;
        [Range(0, 256)] public int priority = 128;
        public bool loop;
    }

    public List<Entry> entries = new List<Entry>();

    private Dictionary<string, Entry> lookup;

    public void BuildLookup()
    {
        if (lookup != null) return;
        lookup = new Dictionary<string, Entry>(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.key))
                continue;
            lookup[entry.key] = entry;
        }
    }

    public bool TryGet(string key, out Entry entry)
    {
        BuildLookup();
        return lookup.TryGetValue(key, out entry);
    }
}

