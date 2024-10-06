using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Audioman : MonoBehaviour
{
    // Start is called before the first frame update

    public int pool_start_size;

    AudioMixerGroup sfx_mixer;

    private Queue<AudioSource> sfx_queue;

    public AudioLoopConfiguration loop_config_test;

    public AudioOneShotClipConfiguration clip_config_test;

    public static Audioman getInstance()
    {
        return FindAnyObjectByType<Audioman>();
    }

    void Awake()
    {
        sfx_queue = new Queue<AudioSource>();
        SpawnAudioSources(pool_start_size);
    }

    private void SpawnAudioSources(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject g = new GameObject();
            g.transform.parent = transform;
            AudioSource audioSource = g.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfx_mixer;
            audioSource.playOnAwake = false;
            sfx_queue.Enqueue(audioSource);
        }
    }

    public LoopHolder PlayLoop(AudioLoopConfiguration loop, Vector3 world_pos)
    {
        AudioSource audioSource = GetAudioSourceFromQueue();
        audioSource.clip = loop.clip;
        audioSource.volume = UnityEngine.Random.Range(loop.vol_min, loop.vol_max);
        audioSource.pitch = UnityEngine.Random.Range(loop.pitch_min, loop.pitch_max);
        audioSource.loop = true;
        audioSource.outputAudioMixerGroup = loop.mixer;
        audioSource.transform.position = world_pos;

        audioSource.Play();
        return new LoopHolder(() =>
        {
            audioSource.Stop();
            sfx_queue.Enqueue(audioSource);
          
        }, (volume) =>
        {
            audioSource.volume = volume;
        });
    }

    public class LoopHolder
    {
        private Action onStop;
        Action<float> changeVolume;

        public LoopHolder(Action onStop, Action<float> changeVolume)
        {
            this.onStop = onStop;
            this.changeVolume = changeVolume;
        }

        public void Stop()
        {
            onStop();
        }

        public void setVolume(float volume) { 
            changeVolume(volume);
        }
    }

    private AudioSource GetAudioSourceFromQueue()
    {
        if (sfx_queue.Count == 0)
        {
            SpawnAudioSources(5);
        }

        return sfx_queue.Dequeue();
    }

    public void PlaySound(AudioOneShotClipConfiguration conf, Vector3 world_pos)
    {
        AudioSource audioSource = GetAudioSourceFromQueue();
        audioSource.transform.position = world_pos;
        audioSource.volume = UnityEngine.Random.Range(conf.vol_min, conf.vol_max);
        audioSource.pitch = UnityEngine.Random.Range(conf.pitch_min, conf.pitch_max);
        audioSource.loop = false;
        audioSource.outputAudioMixerGroup = conf.mixer;

        StartCoroutine(PlayAndWaitForFinish(audioSource, conf.clips[UnityEngine.Random.Range(0, conf.clips.Length)]));
    }

    private IEnumerator PlayAndWaitForFinish(AudioSource source, AudioClip clip)
    {
        float time = 0;
        source.PlayOneShot(clip);
        while (time < clip.length)
        {
            time += Time.deltaTime;
            yield return null;
        }
        sfx_queue.Enqueue(source);
    }
}
