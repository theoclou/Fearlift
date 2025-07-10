using UnityEngine;

public class DelayedAudio : MonoBehaviour
{
    public float delay = 2f; // Délai avant de jouer le son
    public AudioSource audioSource; // Référence à l'Audio

    void Start()
    {
        Invoke(nameof(PlayAudio), delay);
    }

    void PlayAudio()
    {
        audioSource.Play();
    }
}
