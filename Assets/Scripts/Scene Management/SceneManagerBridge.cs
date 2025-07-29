using System.Collections;
using System.Collections.Generic; //  Nécessaire pour utiliser List<>
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;


public class SceneManagerBridge : MonoBehaviour
{
    public string sceneToLoad;             // Nom de la scène à charger
    public GameObject LoadingText;         // Texte à afficher pendant le chargement

    public List<GameObject> LoadingObjects; // Liste des objets à activer pendant le chargement

    public Transform player;               // Référence au joueur, si nécessaire

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Pont traversé ! Changement de scène en cours...");
            LoadingText.SetActive(true);

            foreach (GameObject obj in LoadingObjects)
            {
                obj.SetActive(true); // Activer les objets de chargement
            }
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        //var locomotion = player.GetComponent<ContinuousMoveProvider>();
        //if (locomotion != null) locomotion.enabled = false;
        yield return new WaitForSeconds(2f);
        
        //if (locomotion != null) locomotion.enabled = true;

        LoadingText.SetActive(false);

        // Désactiver les objets après chargement
        
        foreach (GameObject obj in LoadingObjects)
        {
            obj.SetActive(false);
        }

        BLEHeartRateMonitor.Instance.MarkSceneChange(sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
