using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkyScene : MonoBehaviour
{
    public GameObject LoadingText;                 // Texte à afficher
    public List<GameObject> LoadingObjects;        // Objets à activer en permanence après collision

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadingText.SetActive(true);

            Debug.Log("Player a traversé le trigger ! Activation du texte de chargement.");

            foreach (GameObject obj in LoadingObjects)
            {
                obj.SetActive(true); // Activer les objets de chargement
            }

            // Lancer la coroutine pour désactiver le texte après 5 secondes
            StartCoroutine(DisableLoadingTextAfterDelay(5f));
        }
    }

    private IEnumerator DisableLoadingTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadingText.SetActive(false); // Désactiver le texte après le délai
        Debug.Log("Texte de chargement désactivé après 5 secondes.");

        // Attendre encore 10 secondes avant de changer de scène
        yield return new WaitForSeconds(10f);

        StartCoroutine(GoToMenu());
    }

    private IEnumerator GoToMenu()
    {
        // Désactiver les objets de chargement
        foreach (GameObject obj in LoadingObjects)
        {
            obj.SetActive(false);
        }

        yield return null;

        BLEHeartRateMonitor.Instance.MarkSceneChange("Menu");

        // Stop data log correctement
        if (DatabaseManager.instance != null)
        {
            yield return StartCoroutine(DatabaseManager.instance.StopDataLogCoroutine());
        }

        SceneManager.LoadScene("Menu");

        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
