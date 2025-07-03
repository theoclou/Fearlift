using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AltitudeVolumeControl : MonoBehaviour
{
    public float maxAltitude = 5f; // Y haut = volume minimum
    public float minAltitude = -10f; // Y bas = volume max
    public float maxVolume = 1f;
    public float minVolume = 0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float y = transform.position.y;

        // Interpolation inverse selon altitude
        float t = Mathf.InverseLerp(maxAltitude, minAltitude, y);
        float volume = Mathf.Lerp(minVolume, maxVolume, t);

        audioSource.volume = volume;
    }
}
