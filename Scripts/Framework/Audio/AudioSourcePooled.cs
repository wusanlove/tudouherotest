using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourcePooled : MonoBehaviour
{
    private AudioSource source;
    private Coroutine recycleRoutine;

    public AudioSource Source
    {
        get
        {
            if (source == null) source = GetComponent<AudioSource>();
            return source;
        }
    }

    public void Play(AudioClip clip, string poolName, float volume, float pitch, bool loop, float spatialBlend)
    {
        AudioSource s = Source;
        s.clip = clip;
        s.volume = volume;
        s.pitch = pitch;
        s.loop = loop;

        // 2D：不做空间音效
        s.spatialBlend = 0f;
        s.rolloffMode = AudioRolloffMode.Linear;
        s.playOnAwake = false;

        s.Play();

        if (recycleRoutine != null)
            StopCoroutine(recycleRoutine);

        if (!loop)
            recycleRoutine = StartCoroutine(AutoRecycle(Mathf.Max(0.01f, clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)))));
    }

    private IEnumerator AutoRecycle(float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolMgr.Instance.PushObj(gameObject);
    }

    private void OnDisable()
    {
        if (recycleRoutine != null)
            StopCoroutine(recycleRoutine);
        recycleRoutine = null;
    }
}
