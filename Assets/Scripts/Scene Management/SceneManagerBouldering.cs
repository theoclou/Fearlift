using NUnit.Framework;
using System.Collections;
using System.Collections.Generic; // Nécessaire pour utiliser List<>
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;


public class SceneManagerBouldering : MonoBehaviour
{
    public string sceneToLoad;       // Nom de la scène à charger
    public Transform player;         // Référence au joueur
    public float minAltitude = 3f;   // Altitude minimale requise
    private bool altitudeReached = false;
    public GameObject LoadingText; // Texte de chargement
    public List<GameObject> objectsToHide; // Liste des objets à cacher

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

            // Cacher les objets spécifiés
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        //var locomotion = player.GetComponent<ContinuousMoveProvider>();
        //if (locomotion != null) locomotion.enabled = false; // Désactiver le locomotion pour éviter les mouvements pendant le chargement

        yield return new WaitForSeconds(2f); // Attendre 2 secondes

        //if (locomotion != null) locomotion.enabled = true; // Réactiver le locomotion


        LoadingText.SetActive(false); // Désactiver le texte de chargement
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false); // Cacher les objets après le chargement
            }
        }
        BLEHeartRateMonitor.Instance.MarkSceneChange(sceneToLoad); // Marquer le changement de scène
        SceneManager.LoadScene(sceneToLoad);
    }
}
