using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;

public class GazeRaycast : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask videLayer;
    [SerializeField] float gazeRayLength = 20f;
    [SerializeField] float gazeRadius = 0.1f;
    [SerializeField] GameObject gazeMarkerPrefab;

    [Header("Calibration (Auto-applied from Manager)")]
    [SerializeField] Vector3 gazeOffset = Vector3.zero;
    [SerializeField] float horizontalMultiplier = 1.0f;
    [SerializeField] float verticalMultiplier = 1.0f;
    [SerializeField] bool enableGazeSmoothing = true;
    [SerializeField] float smoothingFactor = 0.8f;

    // Utiliser EyeData_v2 comme dans vos autres scripts
    private static EyeData_v2 eyeData = new EyeData_v2();
    private GameObject gazeMarkerInstance;
    private bool eye_callback_registered = false;

    // Variables pour le lissage
    private Vector3 smoothedGazeDirection = Vector3.forward;
    private Vector3 lastValidGazeDirection = Vector3.forward;

    public static bool LooksAtVoid { get; private set; } = false;

    void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            Debug.LogError("SRanipal eye tracking not enabled!");
            enabled = false;
            return;
        }

        if (gazeMarkerPrefab != null)
        {
            gazeMarkerInstance = Instantiate(gazeMarkerPrefab);
            gazeMarkerInstance.SetActive(false);
        }

        // Appliquer automatiquement la calibration sauvegardée
        ApplyCalibrationFromManager();

        Debug.Log($"GazeRaycast initialized - Layer mask: {videLayer.value}");
    }

    private void ApplyCalibrationFromManager()
    {
        // Si le manager existe, récupérer les paramètres
        if (GazeCalibrationManager.Instance != null)
        {
            GazeCalibrationManager.Instance.ApplyCalibrationTo(this);
        }
    }

    public void SetCalibrationData(Vector3 offset, float hMult, float vMult, bool smoothing, float smoothFactor)
    {
        gazeOffset = offset;
        horizontalMultiplier = hMult;
        verticalMultiplier = vMult;
        enableGazeSmoothing = smoothing;
        smoothingFactor = smoothFactor;

        Debug.Log($"Calibration applied: Offset={gazeOffset}, HMult={horizontalMultiplier}, VMult={verticalMultiplier}");
    }

    void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
            return;

        // Gestion des callbacks comme dans DataManager
        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false)
        {
            SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
        }

        // Vérifier si un utilisateur est détecté
        bool userDetected = false;
        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            userDetected = eyeData.no_user; // Logique du sample SRanipal
        }
        else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
        {
            userDetected = true; // Mode fallback
        }

        if (!userDetected)
        {
            LooksAtVoid = false;
            if (gazeMarkerInstance != null)
                gazeMarkerInstance.SetActive(false);
            return;
        }

        // Effectuer le raycast de regard
        PerformGazeRaycastWithSRanipal();
    }

    private void PerformGazeRaycastWithSRanipal()
    {
        Vector3 gazeOrigin, gazeDirection;

        // Récupérer le rayon de regard
        bool gazeRayValid = false;
        if (eye_callback_registered)
        {
            gazeRayValid = SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out gazeDirection, eyeData) ||
                          SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out gazeOrigin, out gazeDirection, eyeData) ||
                          SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out gazeDirection, eyeData);
        }
        else
        {
            gazeRayValid = SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out gazeDirection) ||
                          SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out gazeOrigin, out gazeDirection) ||
                          SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out gazeDirection);
        }

        if (!gazeRayValid)
        {
            LooksAtVoid = false;
            if (gazeMarkerInstance != null)
                gazeMarkerInstance.SetActive(false);
            return;
        }

        // === CORRECTION DU DÉCALAGE ===
        // Garder la direction originale pour la détection
        Vector3 originalGazeDirection = gazeDirection;

        // Appliquer l'offset de calibration pour l'affichage
        Vector3 correctedGazeDirection = gazeDirection;
        correctedGazeDirection.x += gazeOffset.x;
        correctedGazeDirection.y += gazeOffset.y;
        correctedGazeDirection.z += gazeOffset.z;

        // Appliquer les multipliers pour l'affichage
        correctedGazeDirection.x *= horizontalMultiplier;
        correctedGazeDirection.y *= verticalMultiplier;
        correctedGazeDirection = correctedGazeDirection.normalized;

        // Convertir en coordonnées monde
        Vector3 worldGazeOrigin, worldOriginalDirection, worldCorrectedDirection;
        if (mainCamera != null)
        {
            worldGazeOrigin = mainCamera.transform.TransformPoint(gazeOrigin);
            worldOriginalDirection = mainCamera.transform.TransformDirection(originalGazeDirection);
            worldCorrectedDirection = mainCamera.transform.TransformDirection(correctedGazeDirection);
        }
        else
        {
            worldGazeOrigin = gazeOrigin;
            worldOriginalDirection = originalGazeDirection;
            worldCorrectedDirection = correctedGazeDirection;
        }

        // Appliquer le lissage sur la direction originale pour la détection
        Vector3 finalDetectionDirection = worldOriginalDirection;
        if (enableGazeSmoothing)
        {
            if (worldOriginalDirection != Vector3.zero)
            {
                lastValidGazeDirection = worldOriginalDirection;
            }
            smoothedGazeDirection = Vector3.Slerp(smoothedGazeDirection, lastValidGazeDirection, 1f - smoothingFactor);
            finalDetectionDirection = smoothedGazeDirection;
        }

        // Effectuer le raycast avec la direction ORIGINALE (non-corrigée)
        Ray gazeRay = new Ray(worldGazeOrigin, finalDetectionDirection);
        RaycastHit hit;

        if (Physics.Raycast(gazeRay, out hit, gazeRayLength, videLayer))
        {
            LooksAtVoid = true;

            if (gazeMarkerInstance != null)
            {
                gazeMarkerInstance.SetActive(true);
                gazeMarkerInstance.transform.position = hit.point;
            }

            // Log occasionnel pour debug
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"[GazeRaycast] Hit Void layer! Object: {hit.collider.name}, Point: {hit.point}");
            }
        }
        else
        {
            LooksAtVoid = false;
            if (gazeMarkerInstance != null)
                gazeMarkerInstance.SetActive(false);
        }

        // Debug visuel : Afficher le rayon CORRIGÉ (pour l'affichage)
        Debug.DrawRay(worldGazeOrigin, worldCorrectedDirection * gazeRayLength, LooksAtVoid ? Color.green : Color.red);
        // Optionnel: afficher aussi le rayon de détection original en jaune
        Debug.DrawRay(worldGazeOrigin, finalDetectionDirection * gazeRayLength, Color.yellow, 0.1f);
    }

    private void OnDestroy()
    {
        if (eye_callback_registered)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }

    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
    }

    // Méthodes publiques pour la calibration
    public Vector3 GetCurrentGazeDirection()
    {
        Vector3 gazeOrigin, gazeDirection;
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out gazeDirection))
        {
            return mainCamera != null ? mainCamera.transform.TransformDirection(gazeDirection) : gazeDirection;
        }
        return Vector3.zero;
    }

    public Vector3 GetCurrentGazeOrigin()
    {
        Vector3 gazeOrigin, gazeDirection;
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out gazeDirection))
        {
            return mainCamera != null ? mainCamera.transform.TransformPoint(gazeOrigin) : gazeOrigin;
        }
        return mainCamera != null ? mainCamera.transform.position : transform.position;
    }
}