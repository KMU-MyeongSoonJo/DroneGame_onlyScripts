using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class SoundEffect : MonoBehaviour
{
    public static SoundEffect instance;
    [HideInInspector] public AudioSource audioSource;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        audioSource = this.GetComponent<AudioSource>();
    }
}
