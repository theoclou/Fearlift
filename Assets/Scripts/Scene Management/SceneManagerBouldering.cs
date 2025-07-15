using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerBouldering : MonoBehaviour
{
    public string sceneToLoad;       // Nom de la scène à charger
    public Transform player;         // Référence au joueur
    public float minAltitude = 3f;   // Altitude minimale requise
    private bool altitudeReached = false;
    public GameObject LoadingText; // Texte de chargement

    void Update()
    {
        if (!altitudeReached && player.position.y >= minAltitude)
        {
            altitudeReached = true;
            Debug.Log("✅ Altitude minimale atteinte !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (altitudeReached && other.CompareTag("Player"))
        {
            Debug.Log("💥 Collision avec l'eau après altitude -> chargement de scène");

            LoadingText.SetActive(true); // Activer le texte de chargement

            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Attendre 2 secondes

        LoadingText.SetActive(false); // Désactiver le texte de chargement
        SceneManager.LoadScene(sceneToLoad);
    }
}
