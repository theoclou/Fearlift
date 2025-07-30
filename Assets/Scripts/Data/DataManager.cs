using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;

public class DataManager : MonoBehaviour
{
    private static EyeData_v2 eyeData = new EyeData_v2();
    private DataStruct log;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private bool isTestMode = false;
    [SerializeField] private bool startLoggingOnStart = true; // Nouveau paramètre

    private bool eye_callback_registered = false;
    private float sessionStartTime = 0f;

    void Start()
    {
        Debug.Log("DataManager: Démarrage du DataManager");

        // Vérifier si l'eye tracking est activé
        if (!isTestMode && !SRanipal_Eye_Framework.Instance.EnableEye)
        {
            Debug.LogWarning("DataManager: Eye tracking désactivé, passage en mode test");
            isTestMode = true;
        }

        // S'assurer que le DatabaseManager existe
        if (DatabaseManager.instance == null)
        {
            Debug.LogError("DataManager: DatabaseManager non trouvé !");
            enabled = false;
            return;
        }

        // Démarrer le logging si configuré pour le faire (seulement si pas déjà démarré)
        if (startLoggingOnStart && !DatabaseManager.instance.isTaskStart)
        {
            Debug.Log("DataManager: Démarrage automatique du logging");
            DatabaseManager.instance.StartDataLog();
            sessionStartTime = Time.time;
        }
        else if (DatabaseManager.instance.isTaskStart)
        {
            Debug.Log("DataManager: Le logging était déjà démarré");
            // Récupérer le temps de session depuis le DatabaseManager si possible
            sessionStartTime = Time.time; // Approximation, idéalement on stockerait ça dans DatabaseManager
        }
    }

    void Update()
    {
        // Vérifier que le DatabaseManager existe et que le logging est actif
        if (DatabaseManager.instance == null || !DatabaseManager.instance.isTaskStart)
        {
            return;
        }

        // Calculer le temps relatif à la session de logging
        log.time = Time.time - sessionStartTime;

        if (!isTestMode)
        {
            // Vérifier le statut du framework comme dans le sample
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                return;

            // Gestion du callback comme dans le sample
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
                // Polling manuel si pas de callback
                SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
            }

            // Déterminer si les yeux sont actifs (logique du sample)
            bool isLeftEyeActive = false;
            bool isRightEyeActive = false;

            if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
            {
                // Debug occasionnel (toutes les 60 frames)
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log(
                    $"Eye Data : " +
                    $"LeftGazeOrigin={eyeData.verbose_data.left.gaze_origin_mm}, " +
                    $"LeftGazeDirection={eyeData.verbose_data.left.gaze_direction_normalized}, " +
                    $"LeftPupilDiameter={eyeData.verbose_data.left.pupil_diameter_mm}, " +
                    $"LeftEyeOpenness={eyeData.verbose_data.left.eye_openness}, " +
                    $"no_user={eyeData.no_user}");
                }

                // Vérifier d'abord la validité des données avant de les utiliser
                bool leftDataValid = (eyeData.verbose_data.left.eye_data_validata_bit_mask & (int)SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) != 0;
                bool rightDataValid = (eyeData.verbose_data.right.eye_data_validata_bit_mask & (int)SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) != 0;

                isLeftEyeActive = !eyeData.no_user && leftDataValid;
                isRightEyeActive = !eyeData.no_user && rightDataValid;
            }
            else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
            {
                // Mode fallback si eye tracking non supporté
                isLeftEyeActive = true;
                isRightEyeActive = true;
            }

            // Extraire les données si les yeux sont actifs
            if (isLeftEyeActive || isRightEyeActive)
            {
                // Données de position des pupilles
                log.leftPosition = eyeData.verbose_data.left.pupil_position_in_sensor_area;
                log.rightPosition = eyeData.verbose_data.right.pupil_position_in_sensor_area;

                // Données d'origine du regard
                log.leftGazeOrigin = eyeData.verbose_data.left.gaze_origin_mm;
                log.rightGazeOrigin = eyeData.verbose_data.right.gaze_origin_mm;

                // Direction du regard normalisée
                log.leftGazeDirection = eyeData.verbose_data.left.gaze_direction_normalized;
                log.rightGazeDirection = eyeData.verbose_data.right.gaze_direction_normalized;

                // Diamètre des pupilles
                log.leftPupilDiameter = eyeData.verbose_data.left.pupil_diameter_mm;
                log.rightPupilDiameter = eyeData.verbose_data.right.pupil_diameter_mm;

                // Ouverture des yeux
                log.leftOpenness = eyeData.verbose_data.left.eye_openness;
                log.rightOpenness = eyeData.verbose_data.right.eye_openness;

                // LooksAtVoid (vérifier que GazeRaycast existe)
                try
                {
                    log.looksAtVoid = GazeRaycast.LooksAtVoid;
                }
                catch
                {
                    log.looksAtVoid = false; // Valeur par défaut si GazeRaycast n'existe pas
                }
            }
            else
            {
                // Réinitialiser les données si pas d'utilisateur détecté
                log.leftPosition = Vector2.zero;
                log.rightPosition = Vector2.zero;
                log.leftGazeOrigin = Vector3.zero;
                log.rightGazeOrigin = Vector3.zero;
                log.leftGazeDirection = Vector3.zero;
                log.rightGazeDirection = Vector3.zero;
                log.leftPupilDiameter = 0f;
                log.rightPupilDiameter = 0f;
                log.leftOpenness = 0f;
                log.rightOpenness = 0f;
                log.looksAtVoid = false;
            }
        }
        else
        {
            // Mode test - générer des données factices
            log.leftPosition = Vector2.zero;
            log.rightPosition = Vector2.zero;
            log.leftGazeOrigin = Vector3.zero;
            log.rightGazeOrigin = Vector3.zero;
            log.leftGazeDirection = Vector3.forward;
            log.rightGazeDirection = Vector3.forward;
            log.leftPupilDiameter = 3.0f;
            log.rightPupilDiameter = 3.0f;
            log.leftOpenness = 1.0f;
            log.rightOpenness = 1.0f;
            log.looksAtVoid = false;
        }

        // Données HMD (toujours récupérées)
        if (mainCamera != null)
        {
            log.HMDpos = mainCamera.transform.position;
            log.HMDrot = mainCamera.transform.rotation;
        }
        else
        {
            // Fallback sur la caméra principale si mainCamera n'est pas assignée
            Camera cam = Camera.main;
            if (cam != null)
            {
                log.HMDpos = cam.transform.position;
                log.HMDrot = cam.transform.rotation;
            }
        }

        // Mettre à jour la base de données
        DatabaseManager.instance.UpdateDataLog(log);
    }

    private void OnDestroy()
    {
        Release();
    }

    private void OnApplicationQuit()
    {
        Release();
    }

    private void Release()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(
                Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback)
            );
            eye_callback_registered = false;
        }
    }

    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
    }

    // Méthodes publiques pour contrôler le logging depuis l'extérieur
    public void StartLogging()
    {
        if (DatabaseManager.instance != null)
        {
            DatabaseManager.instance.StartDataLog();
            sessionStartTime = Time.time;
        }
    }

}