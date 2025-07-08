using UnityEngine;
using UnityEngine.Audio;

public class WindClimbingSound : MonoBehaviour
{
    public float maxHeight = 20f; // Maximum height for sound volume adjustment
    public Transform playerTransform;
    private AudioSource audioSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Augmenter le volume du son de vent en fonction de la hauteur du joueur, avec une hauteur max de 20 sur l'axe Y
        float playerHeight = playerTransform.position.y; // Assuming this script is attached to the player object
        float volume = Mathf.Clamp(playerHeight / maxHeight, 0f, 1f); // Normalize height to a value between 0 and 1

        audioSource.volume = volume; // Set the volume based on player height
    }
}
