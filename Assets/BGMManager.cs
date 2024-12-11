using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BGMManager : MonoBehaviour
{
      private static BGMManager instance;
    public AudioSource audioSource;
    [Range(0, 1)] public float volume;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
