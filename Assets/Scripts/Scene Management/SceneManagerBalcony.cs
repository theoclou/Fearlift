using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class SceneManagerBalcony : MonoBehaviour
{
    public int measuredBPMValue; // À mettre à jour
    public float checkInterval = 1f; // fréquence de vérification
    public int tolerance = 10; // marge d'écart autorisée
    public float requiredDuration = 30f; // durée à rester stable
    public string sceneToLoad;
    public GameObject LoadingText;
    public GameObject LoadingPlane;
    public GameObject LoadingPlane2; // Optionnel, si vous avez un deuxième plan de chargement
    public GameObject LoadingPlane3;

    private Coroutine checkRoutine;

    public Transform player; // Référence au joueur, si nécessaire

    private void Start()
    {
        Debug.Log("▶️ SceneManagerBalcony started with measuredBPMValue: " + measuredBPMValue);
        StartChecking();
    }


    public void StartChecking()
    {

        Debug.Log("▶️ StartChecking called");

        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            Debug.Log("▶️ STOP");
        }
            
        checkRoutine = StartCoroutine(CheckStability());
    }

    private IEnumerator CheckStability()
    {
        float stableTime = 0f;
        int referenceValue = BLEHeartRateMonitor.Instance.heartRate;

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            int currentValue = BLEHeartRateMonitor.Instance.heartRate;
            int delta = Mathf.Abs(currentValue - referenceValue);

            if (delta <= tolerance)
            {
                stableTime += checkInterval;
                Debug.Log($"Stable for {stableTime} seconds");
                if (stableTime >= requiredDuration)
                {
                    Debug.Log("✅ Value has remained stable for 30 seconds!");
                    LoadingText.SetActive(true);
                    LoadingPlane.SetActive(true);
                    LoadingPlane2.SetActive(true);
                    LoadingPlane3.SetActive(true);

                    //var locomotion = player.GetComponent<ContinuousMoveProvider>();

                    //if (locomotion != null) locomotion.enabled = false; // Désactiver le mouvement avant de charger la scène
                    yield return new WaitForSeconds(2f); // Attendre un peu avant de charger la scène
                    
                    //if (locomotion != null) locomotion.enabled = true; // Réactiver le mouvement après le chargement
                    LoadingText.SetActive(false);
                    LoadingPlane.SetActive(false);
                    LoadingPlane2.SetActive(false);
                    LoadingPlane3.SetActive(false);
                    BLEHeartRateMonitor.Instance.MarkSceneChange(sceneToLoad);
                    SceneManager.LoadScene(sceneToLoad);
                    
                }
            }
            else
            {
                Debug.Log($"❌ Value changed too much: Δ={delta} (resetting)");
                referenceValue = currentValue;
                stableTime = 0f;
            }
        }
    }


    private void OnEnable()
    {
        InputManager.OnResetRequested += HandleResetRequested;
    }

    private void OnDisable()
    {
        InputManager.OnResetRequested -= HandleResetRequested;
    }

    private void HandleResetRequested()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
