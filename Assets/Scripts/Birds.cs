using UnityEngine;

public class Birds : MonoBehaviour
{
    public AudioClip audioClip; // Clip à jouer (défini dans l’inspecteur)
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (audioClip != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
