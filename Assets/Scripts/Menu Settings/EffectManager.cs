using UnityEngine;
using System;

public static class EffectManager
{
    public static float EffectQuality = 1f;

    public static event Action<float> OnEffectQualityChanged;

    public static void SetEffectQuality(float value)
    {
        EffectQuality = value;
        OnEffectQualityChanged?.Invoke(value);
    }
}