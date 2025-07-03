using UnityEngine;

public class AltitudeAudioController : MonoBehaviour
{
    public Transform playerTransform;  // XR Origin ou tête du joueur
    public AudioSource villeAudio;
    public AudioSource ventAudio;

    public float minHeight = 0f;    // Niveau du sol
    public float maxHeight = 5.18f;   // Sommet du bâtiment

    void Update()
    {
        float height = playerTransform.position.y;
        float t = Mathf.InverseLerp(minHeight, maxHeight, height); // Valeur entre 0 et 1

        // Ville : volume diminue avec la hauteur
        villeAudio.volume = Mathf.Lerp(1f, 0.6f, t);

        // Vent : volume augmente avec la hauteur
        ventAudio.volume = Mathf.Lerp(0f, 1f, t);
    }
}
