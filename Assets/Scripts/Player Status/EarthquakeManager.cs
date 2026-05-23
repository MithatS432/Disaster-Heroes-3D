using UnityEngine;
using TMPro;
using System.Collections;

public class EarthquakeManager : MonoBehaviour
{
    [Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeDuration = 3f;
    public float shakeMagnitude = 0.2f;


    [Header("Audio")]
    public AudioSource earthquakeAudio;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEarthquakeStart += StartEarthquake;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEarthquakeStart -= StartEarthquake;
    }
    public void StartEarthquake()
    {
        Debug.Log("Deprem başladı!");

        if (cameraShake != null)
        {
            cameraShake.Shake(shakeDuration, shakeMagnitude);
        }

        if (earthquakeAudio != null && !earthquakeAudio.isPlaying)
        {
            earthquakeAudio.Play();
        }
    }
}