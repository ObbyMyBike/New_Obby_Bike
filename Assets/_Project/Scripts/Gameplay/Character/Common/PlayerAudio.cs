using UnityEngine;

public class PlayerAudio
{
    private readonly AudioSource jumpSource;
    private readonly AudioSource pushSource;

    public PlayerAudio(GameObject host, float spatialBlend = 0f)
    {
        jumpSource = CreateSource(host, spatialBlend);
        pushSource = CreateSource(host, spatialBlend);
    }

    private AudioSource CreateSource(GameObject host, float spatialBlend)
    {
        AudioSource source = host.AddComponent<AudioSource>();
        
        source.playOnAwake = false;
        source.loop = false;
        source.clip = null;
        source.spatialBlend = spatialBlend;
        
        return source;
    }

    public void PlayJump(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
            return;
        
        jumpSource.pitch = pitch;
        
        jumpSource.PlayOneShot(clip, volume);
    }

    public void PlayPush(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
            return;
        
        pushSource.pitch = pitch;
        
        pushSource.PlayOneShot(clip, volume);
    }
}