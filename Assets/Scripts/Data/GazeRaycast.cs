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

    [Header("Gaze Calibration")]
    [SerializeField] Vector3 gazeOffset = Vector3.zero; // Offset pour corriger le décalage
    [SerializeField] float horizontalMultiplier = 1.0f; // Multiplier horizontal pour ajuster la sensibilité
    [SerializeField] float verticalMultiplier = 1.0f;   // Multiplier vertical pour ajuster la sensibilité
    [SerializeField] bool enableGazeSmoothing = true;   // Lissage du regard
    [SerializeField] float smoothingFactor = 0.8f;      // Facteur de lissage (0-1)

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

        Debug.Log($"GazeRaycast initialized - Layer mask: {videLayer.value}");
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

        // Méthode 1: Utiliser la fonction Focus de SRanipal (recommandée)
        PerformGazeRaycastWithSRanipal();

        // Méthode 2: Raycast manuel (alternative si la méthode 1 ne marche pas)
        // PerformManualGazeRaycast();
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

        // Appliquer d'abord l'offset de calibration AVANT la transformation (pour l'affichage)
        Vector3 correctedGazeDirection = gazeDirection;
        correctedGazeDirection.x += gazeOffset.x;
        correctedGazeDirection.y += gazeOffset.y;
        correctedGazeDirection.z += gazeOffset.z;

        // Appliquer les multipliers pour corriger la sensibilité (pour l'affichage)
        correctedGazeDirection.x *= horizontalMultiplier;
        correctedGazeDirection.y *= verticalMultiplier;

        // Normaliser AVANT la transformation en coordonnées monde
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

        // Appliquer le lissage si activé (sur la direction originale pour la détection)
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

        // Debug visuel : Afficher le rayon CORRIGÉ (pour l'affichage) mais détecter avec l'ORIGINAL
        Debug.DrawRay(worldGazeOrigin, worldCorrectedDirection * gazeRayLength, LooksAtVoid ? Color.green : Color.red);
        // Optionnel: afficher aussi le rayon de détection original en jaune
        Debug.DrawRay(worldGazeOrigin, finalDetectionDirection * gazeRayLength, Color.yellow, 0.1f);
    }

    private void PerformManualGazeRaycast()
    {
        // Alternative: raycast manuel basé sur les données brutes
        Vector3 leftGaze = eyeData.verbose_data.left.gaze_direction_normalized;
        Vector3 rightGaze = eyeData.verbose_data.right.gaze_direction_normalized;

        // Vérifier la validité des données
        bool leftValid = (eyeData.verbose_data.left.eye_data_validata_bit_mask & (int)SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) != 0;
        bool rightValid = (eyeData.verbose_data.right.eye_data_validata_bit_mask & (int)SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) != 0;

        Vector3 combinedGaze = Vector3.zero;
        if (leftValid && rightValid)
        {
            combinedGaze = (leftGaze + rightGaze) * 0.5f;
        }
        else if (leftValid)
        {
            combinedGaze = leftGaze;
        }
        else if (rightValid)
        {
            combinedGaze = rightGaze;
        }
        else
        {
            LooksAtVoid = false;
            if (gazeMarkerInstance != null)
                gazeMarkerInstance.SetActive(false);
            return;
        }

        // Origine du regard (caméra principale)
        Vector3 gazeOrigin = mainCamera != null ? mainCamera.transform.position : transform.position;
        Vector3 gazeDirection = mainCamera != null ? mainCamera.transform.TransformDirection(combinedGaze) : combinedGaze;

        // Raycast
        Ray gazeRay = new Ray(gazeOrigin, gazeDirection);
        RaycastHit hit;

        if (Physics.Raycast(gazeRay, out hit, gazeRayLength, videLayer))
        {
            LooksAtVoid = true;

            if (gazeMarkerInstance != null)
            {
                gazeMarkerInstance.SetActive(true);
                gazeMarkerInstance.transform.position = hit.point;
            }

            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"[ManualGaze] Hit Void layer! Object: {hit.collider.name}");
            }
        }
        else
        {
            LooksAtVoid = false;
            if (gazeMarkerInstance != null)
                gazeMarkerInstance.SetActive(false);
        }

        Debug.DrawRay(gazeOrigin, gazeDirection * gazeRayLength, LooksAtVoid ? Color.blue : Color.yellow);
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

    // Méthodes publiques pour accéder aux informations et calibration
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

    // Méthodes de calibration à appeler depuis l'Inspector ou d'autres scripts
    public void CalibrateGazeOffset(Vector3 targetWorldPosition)
    {
        Vector3 currentGazeOrigin = GetCurrentGazeOrigin();
        Vector3 currentGazeDirection = GetCurrentGazeDirection();

        Vector3 expectedDirection = (targetWorldPosition - currentGazeOrigin).normalized;
        Vector3 actualDirection = currentGazeDirection;

        Vector3 error = expectedDirection - actualDirection;
        gazeOffset += mainCamera.transform.InverseTransformDirection(error);

        Debug.Log($"Gaze calibrated! New offset: {gazeOffset}");
    }

    [ContextMenu("Reset Gaze Calibration")]
    public void ResetGazeCalibration()
    {
        gazeOffset = Vector3.zero;
        horizontalMultiplier = 1.0f;
        verticalMultiplier = 1.0f;
        Debug.Log("Gaze calibration reset!");
    }
}