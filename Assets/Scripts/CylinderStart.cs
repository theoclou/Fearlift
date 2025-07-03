using System.Collections;
using TMPro;
using UnityEngine;

public class CylinderStart : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public float bpm = 76f; // Actuellement chiffre fixe
    public Material emissionMaterial; // Mat�riau d'�mission � appliquer
    public GameObject canvas;

    private void OnTriggerEnter(Collider other)
    {
        canvas.SetActive(true);
        if (other.CompareTag("Player")) // Assure-toi que ta XR Rig est tagu�e "Player"
        {
            StartCoroutine(HandleMeasurementSequence());
        }
    }

    private IEnumerator HandleMeasurementSequence()
    {
        // Allumer la lumi�re ou l�objet
        Renderer rend = GetComponent<Renderer>();

        //Lui apppliquer le mat�riau d'�mission
        rend.material = emissionMaterial;


        // Afficher texte initial
        statusText.text = "Prise des mesures en cours...";
        yield return new WaitForSeconds(2f); // Pause avant affichage BPM

        // Afficher BPM -- TODO : Remplacer par le BPM r�el et dynamique
        statusText.text = $"BPM : {bpm}";
    }
}
