using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public bool playing = false;
    public bool playOnAwake = false;
    public AudioSource source;
    public AudioClip[] clips;

    public float soundPitch = 1.0f;

    public string SoundType = "sound";
    public bool canPlay = true;

    bool isDestroying = false;
    bool playedAtDestroy = false;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = clips[0];

        if (playOnAwake) playing = true;

        if (!GameObject.Find("Canvas").GetComponent<InGameMenuScript>().canMusicPlay && SoundType == "music") canPlay = false;
        if (!GameObject.Find("Canvas").GetComponent<InGameMenuScript>().canSoundPlay && SoundType == "sound")
        {
            canPlay = false;
            source.Stop();
        }
    }

    void Update()
    {
        source.pitch = Time.timeScale * soundPitch;

        // Normal Loop
        if(playing && !source.isPlaying)
        {
            if(clips.Length > 1) source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();

            if (playOnAwake) playing = false;
        }

        // If isDestroying play source if not playing yet
        if(isDestroying && !source.isPlaying && !playedAtDestroy)
        {
            source.Play();
            playedAtDestroy = true;
        }

        source.enabled = canPlay;
    }

    // Play Single Audio
    public void PlaySingle()
    {
        if (clips.Length > 1) source.clip = clips[Random.Range(0, clips.Length)];
        source.Play();
    }

    public void PlaySingle(int clip)
    {
        source.clip = clips[clip];
        source.Play();
    }

    public void PlaySingleAndDestroy()
    {
        if (clips.Length > 1) source.clip = clips[Random.Range(0, clips.Length)];
        isDestroying = true;
        Destroy(gameObject, source.clip.length);
    }

    public void PlaySingleAndDestroy(int clip)
    {
        source.clip = clips[clip];
        isDestroying = true;
        Destroy(gameObject, clips[clip].length);
    }


    public void FadeInAndStartLoop(int clip, float fadeTimeSec)
    {
        source.clip = clips[clip];
        playing = true;

        float oldVolume = source.volume;
        source.volume = 0;
        StartCoroutine(FadeProcessor(oldVolume, true, 1 / fadeTimeSec));
    }
    public void FadeOutAndStopLoop(int clip, float fadeTimeSec)
    {
        source.clip = clips[clip];

        StartCoroutine(FadeProcessor(source.volume, false, 1 / fadeTimeSec));
    }
    IEnumerator FadeProcessor(float originalVolume, bool isGoUp, float stepPerFrame)
    {
        // Change Volume
        if(isGoUp)
        {
            source.volume += stepPerFrame * Time.deltaTime;
        }
        else
        {
            source.volume -= stepPerFrame * Time.deltaTime;
        }

        yield return new WaitForEndOfFrame();

        // Check if goal is reached
        if(isGoUp && source.volume < originalVolume)
        {
            StartCoroutine(FadeProcessor(originalVolume, isGoUp, stepPerFrame));
        }
        else if(!isGoUp && source.volume > 0)
        {
            StartCoroutine(FadeProcessor(originalVolume, isGoUp, stepPerFrame));
        }
        else
        {
            source.volume = originalVolume;
            if (!isGoUp)
            {
                playing = false;
                Stop();
            }
        }
    }

    // Stop
    public void Stop()
    {
        source.Stop();
    }
}
