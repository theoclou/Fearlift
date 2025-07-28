using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CylinderStart : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public float bpm;
    public Material emissionMaterial; // Matériau d'émission à appliquer
    public GameObject canvas;
    [SerializeField] private int timeBeforeLaunch;

    private void OnTriggerEnter(Collider other)
    {
        canvas.SetActive(true);
        bpm = BLEHeartRateMonitor.Instance.heartRate;
        if (other.CompareTag("Player"))
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
        statusText.text = "Taking measurements in progress...";
        yield return new WaitForSeconds(2f); // Pause avant affichage BPM

        // Afficher BPM -- TODO : Remplacer par le BPM réel et dynamique
        statusText.text = $"BPM : {bpm}";

        int timer = 0;
        //Remplacer dynamiquement le BPM
        while (timer<7)
        {
            bpm = BLEHeartRateMonitor.Instance.heartRate; // Mettre à jour le BPM
            statusText.text = $"BPM : {bpm}"; // Afficher le BPM mis à jour
            yield return new WaitForSeconds(1f); // Mettre à jour toutes les secondes
            timer++;
        }

        statusText.text = "Launching next scene...";

        BLEHeartRateMonitor.Instance.MarkSceneChange("ClosedBalcony");
        SceneManager.LoadScene("ClosedBalcony");
    }
}
