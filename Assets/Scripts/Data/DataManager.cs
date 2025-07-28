//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using ViveSR.anipal.Eye;
//using UnityEngine.XR;
//using Valve.VR;
//using System;
//public class DataManager : MonoBehaviour
//{
//    private static EyeData_v2 _eyeDataV2 = new EyeData_v2();
//    private static VerboseData _verboseData;
//    DataStruct _log;
//    [SerializeField]
//    GameObject _mainCamera;
//    [SerializeField] bool _isTestMode = false;
//    private bool flagBrower = true;
//    private bool delayFlag = true;
//    private float ProcessingTime = 0;
//    private int BrowerCount = 0;
//    private float PreROpenness = 0;
//    void Start()
//    {
//        DatabaseManager.instance.isTaskStart = true;
//        DatabaseManager.instance.StartDataLog();
//    }

//    void Update()
//    {
//        if (!DatabaseManager.instance.isTaskStart) return;
//        _log.time += Time.deltaTime;
//        if (!_isTestMode)
//        {

//            SRanipal_Eye_API.GetEyeData_v2(ref _eyeDataV2);
//            SRanipal_Eye_v2.GetVerboseData(out _verboseData, _eyeDataV2);
//            _log.leftPosition = _eyeDataV2.verbose_data.left.pupil_position_in_sensor_area;
//            _log.rightPosition = _eyeDataV2.verbose_data.right.pupil_position_in_sensor_area;
//            _log.leftGazeOrigin = _eyeDataV2.verbose_data.left.gaze_origin_mm;
//            _log.rightGazeOrigin = _eyeDataV2.verbose_data.right.gaze_origin_mm;
//            _log.leftGazeDirection = _eyeDataV2.verbose_data.left.gaze_direction_normalized;
//            _log.rightGazeDirection = _eyeDataV2.verbose_data.right.gaze_direction_normalized;
//            _log.leftPupilDiameter = _eyeDataV2.verbose_data.left.pupil_diameter_mm;
//            _log.rightPupilDiameter = _eyeDataV2.verbose_data.right.pupil_diameter_mm;
//            _log.leftOpenness = _eyeDataV2.verbose_data.left.eye_openness;
//            _log.rightOpenness = _eyeDataV2.verbose_data.right.eye_openness;
//        }
//        _log.HMDpos = _mainCamera.transform.position;
//        _log.HMDrot = _mainCamera.transform.rotation;
//        DatabaseManager.instance.UpdateDataLog(_log);
//    }


//    private void OnApplicationQuit()
//    {
//        DatabaseManager.instance.StopDataLog();
//    }
//}

using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;

public class DataManager : MonoBehaviour
{
    private static EyeData_v2 eyeData = new EyeData_v2();
    private DataStruct log;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private bool isTestMode = false;

    private bool eye_callback_registered = false;

    void Start()
    {
        // Vérifier si l'eye tracking est activé
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        // Initialiser la base de données
        DatabaseManager.instance.isTaskStart = true;
        DatabaseManager.instance.StartDataLog();
    }

    void Update()
    {
        if (!DatabaseManager.instance.isTaskStart) return;

        // Vérifier le statut du framework comme dans le sample
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
            return;

        log.time += Time.deltaTime;

        if (!isTestMode)
        {
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
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log(
                    $"Eye Data : " +
                    $"LeftGazeOrigin={eyeData.verbose_data.left.gaze_origin_mm}, " +
                    $"LeftGazeDirection={eyeData.verbose_data.left.gaze_direction_normalized}, " +
                    $"LeftPupilDiameter={eyeData.verbose_data.left.pupil_diameter_mm}, " +
                    $"LeftEyeOpenness={eyeData.verbose_data.left.eye_openness}, " +
                    $"no_user={eyeData.no_user}, ");
                }
                // Vérifier d'abord la validité des données avant de les utiliser
                bool leftDataValid = (eyeData.verbose_data.left.eye_data_validata_bit_mask & (int)SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) != 0;
                bool rightDataValid = (eyeData.verbose_data.right.eye_data_validata_bit_mask & (int)SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) != 0;

                isLeftEyeActive = eyeData.no_user && leftDataValid;
                isRightEyeActive = eyeData.no_user && rightDataValid;
            }
            else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
            {
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
            }
        }

        // Données HMD (toujours récupérées)
        if (mainCamera != null)
        {
            log.HMDpos = mainCamera.transform.position;
            log.HMDrot = mainCamera.transform.rotation;
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
        DatabaseManager.instance.StopDataLog();
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
}


