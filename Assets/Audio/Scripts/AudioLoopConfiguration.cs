using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Loop")]

public class AudioLoopConfiguration : ScriptableObject
{

    // Start is called before the first frame update
    public AudioClip clip;

    public AudioMixerGroup mixer;

    [Range(0, 1)]
    public float vol_min = 0.5f;
    [Range(0, 1)]
    public float vol_max = 1f;

    [Range(-3, 3)]
    public float pitch_min = 0f;
    [Range(-3, 3)]
    public float pitch_max = 1f;

}
