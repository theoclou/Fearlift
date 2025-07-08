using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AltitudeVolumeControl : MonoBehaviour
{
    public float maxAltitude = 5f;   // Y haut = volume minimum
    public float minAltitude = -10f; // Y bas = volume maximum
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

        float t = Mathf.InverseLerp(minAltitude, maxAltitude, y);
        float volume = Mathf.Lerp(maxVolume, minVolume, t);

        audioSource.volume = volume;
    }
}
