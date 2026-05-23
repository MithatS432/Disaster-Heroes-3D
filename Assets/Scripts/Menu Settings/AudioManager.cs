using UnityEngine;
using System;

public static class AudioManager
{
    public static float MasterVolume = 1f;
    public static event Action<float> OnVolumeChanged;

    public static void SetVolume(float value)
    {
        MasterVolume = value;
        OnVolumeChanged?.Invoke(value);
    }
}