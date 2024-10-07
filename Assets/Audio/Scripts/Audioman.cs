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
    private Dictionary<string, AudioOneShotClipConfiguration> configs = new();


    public static Audioman getInstance()
    {
        return FindAnyObjectByType<Audioman>();
    }

    void Awake()
    {
        Debug.Log("AUDIO AWAKE");
        if (sfx_mixer == null) {
            Debug.Log("CREATE QUEUE");

            sfx_queue = new Queue<AudioSource>();
            SpawnAudioSources(pool_start_size);
        }

    }

    private void Start()
    {
        PlayLoop(Resources.Load<AudioLoopConfiguration>("object/music"), this.transform.position, true);
        PlayLoop(Resources.Load<AudioLoopConfiguration>("object/ambiance"), this.transform.position, true);

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
            audioSource.spatialBlend = 1.0f;
            sfx_queue.Enqueue(audioSource);
        }
    }
    public LoopHolder PlayLoop(AudioLoopConfiguration loop, Vector3 world_pos) {
        return PlayLoop(loop, world_pos, false);

    }
    public LoopHolder PlayLoop(AudioLoopConfiguration loop, Vector3 world_pos, bool two_d)
    {
        AudioSource audioSource = GetAudioSourceFromQueue();
        audioSource.clip = loop.clip;
        audioSource.volume = UnityEngine.Random.Range(loop.vol_min, loop.vol_max);
        audioSource.pitch = UnityEngine.Random.Range(loop.pitch_min, loop.pitch_max);
        audioSource.loop = true;
        audioSource.outputAudioMixerGroup = loop.mixer;
        audioSource.transform.position = world_pos;
        audioSource.spatialBlend = two_d ? 0 : 1;
        audioSource.Play();
        return new LoopHolder(() =>
        {
            if (audioSource)
            {
                audioSource.Stop();
                sfx_queue.Enqueue(audioSource);
            }
          
        }, (volume) =>
        {
            audioSource.volume = volume;
        }, (Vector3 worl_pos) =>
        {
            audioSource.transform.position = worl_pos;
        });
    }

    public class LoopHolder
    {
        private Action onStop;
        Action<float> changeVolume;
        Action<Vector3> setPosition;
        public LoopHolder(Action onStop, Action<float> changeVolume, Action<Vector3> setPosition)
        {
            this.onStop = onStop;
            this.changeVolume = changeVolume;
            this.setPosition = setPosition;
        }

        public void Stop()
        {
            onStop();
        }

        public void setVolume(float volume) { 
            changeVolume(volume);
        }
        public void setWorldPosition(Vector3 worldPos)
        {
            setPosition(worldPos);
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
    public void PlaySound(string sound, Vector3 worldPosition)
    {
        if (!configs.ContainsKey(sound))
        {
            var config = Resources.Load<AudioOneShotClipConfiguration>($"object/{sound}");
            configs.Add(sound, config);
        }

        PlaySound(configs[sound], worldPosition);
    }
    public void PlaySound(AudioOneShotClipConfiguration conf, Vector3 world_pos)
    {
        PlaySound(conf, world_pos, false);
    }

    public void PlaySound(AudioOneShotClipConfiguration conf, Vector3 world_pos, bool two_d)
    {
        AudioSource audioSource = GetAudioSourceFromQueue();
        audioSource.transform.position = world_pos;
        audioSource.volume = UnityEngine.Random.Range(conf.vol_min, conf.vol_max);
        audioSource.pitch = UnityEngine.Random.Range(conf.pitch_min, conf.pitch_max);
        audioSource.loop = false;
        audioSource.outputAudioMixerGroup = conf.mixer;
        audioSource.spatialBlend = two_d ? 0 : 1;
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
