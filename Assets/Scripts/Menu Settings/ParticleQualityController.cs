using UnityEngine;

public class ParticleQualityController : MonoBehaviour
{
    private ParticleSystem ps;

    private float originalRate;
    private int originalMaxParticles;

    private ParticleSystem.Burst[] originalBursts;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        originalMaxParticles = main.maxParticles;

        var emission = ps.emission;

        originalRate = emission.rateOverTime.constant;

        originalBursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(originalBursts);

        ApplyQuality(EffectManager.EffectQuality);

        EffectManager.OnEffectQualityChanged += ApplyQuality;
    }

    void OnDestroy()
    {
        EffectManager.OnEffectQualityChanged -= ApplyQuality;
    }

    private void ApplyQuality(float quality)
    {
        var main = ps.main;
        main.maxParticles = Mathf.RoundToInt(originalMaxParticles * quality);

        var emission = ps.emission;

        emission.rateOverTime = originalRate * quality;

        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[originalBursts.Length];

        for (int i = 0; i < originalBursts.Length; i++)
        {
            bursts[i] = originalBursts[i];

            float burstCount = originalBursts[i].count.constant * quality;

            bursts[i].count = new ParticleSystem.MinMaxCurve(
                Mathf.Max(1, Mathf.RoundToInt(burstCount))
            );
        }

        emission.SetBursts(bursts);
    }
}