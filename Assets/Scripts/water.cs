using UnityEngine;

public class DelayedAudio : MonoBehaviour
{
    public float delay = 2f; // D�lai avant de jouer le son
    public AudioSource audioSource; // R�f�rence � l'Audio

    void Start()
    {
        Invoke(nameof(PlayAudio), delay);
    }

    void PlayAudio()
    {
        audioSource.Play();
    }
}
