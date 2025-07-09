using System.Collections;
using TMPro;
using UnityEngine;

public class CylinderStart : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public float bpm = 76f; // Actuellement chiffre fixe
    public Material emissionMaterial; // Matériau d'émission à appliquer
    public GameObject canvas;

    private void OnTriggerEnter(Collider other)
    {
        canvas.SetActive(true);
        if (other.CompareTag("Player")) // Assure-toi que ta XR Rig est taguée "Player"
        {
            StartCoroutine(HandleMeasurementSequence());
        }
    }

    private IEnumerator HandleMeasurementSequence()
    {
        // Allumer la lumière ou l’objet
        Renderer rend = GetComponent<Renderer>();

        //Lui apppliquer le matériau d'émission
        rend.material = emissionMaterial;


        // Afficher texte initial
        statusText.text = "Prise des mesures en cours...";
        yield return new WaitForSeconds(2f); // Pause avant affichage BPM

        // Afficher BPM -- TODO : Remplacer par le BPM réel et dynamique
        statusText.text = $"BPM : {bpm}";
    }
}
