using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using UnityEngine.XR;
using Valve.VR;
using System;
public class DataManager : MonoBehaviour
{
    private static EyeData_v2 _eyeDataV2 = new EyeData_v2();
    private static VerboseData _verboseData;
    DataStruct _log;
    [SerializeField]
    GameObject _mainCamera;
    [SerializeField] bool _isTestMode = false;
    private bool flagBrower = true;
    private bool delayFlag = true;
    private float ProcessingTime = 0;
    private int BrowerCount = 0;
    private float PreROpenness = 0;
    void Start()
    {
        DatabaseManager.instance.isTaskStart = true;
        DatabaseManager.instance.StartDataLog();
    }

    void Update()
    {
        if (!DatabaseManager.instance.isTaskStart) return;
        _log.time += Time.deltaTime;
        if (!_isTestMode)
        {

        SRanipal_Eye_API.GetEyeData_v2(ref _eyeDataV2);
        SRanipal_Eye_v2.GetVerboseData(out _verboseData, _eyeDataV2);
        _log.leftPosition = _eyeDataV2.verbose_data.left.pupil_position_in_sensor_area;
        _log.rightPosition = _eyeDataV2.verbose_data.right.pupil_position_in_sensor_area;
        _log.leftGazeOrigin = _eyeDataV2.verbose_data.left.gaze_origin_mm;
        _log.rightGazeOrigin = _eyeDataV2.verbose_data.right.gaze_origin_mm;
        _log.leftGazeDirection = _eyeDataV2.verbose_data.left.gaze_direction_normalized;
        _log.rightGazeDirection = _eyeDataV2.verbose_data.right.gaze_direction_normalized;
        _log.leftPupilDiameter = _eyeDataV2.verbose_data.left.pupil_diameter_mm;
        _log.rightPupilDiameter = _eyeDataV2.verbose_data.right.pupil_diameter_mm;
        _log.leftOpenness = _eyeDataV2.verbose_data.left.eye_openness;
        _log.rightOpenness = _eyeDataV2.verbose_data.right.eye_openness;
        }
        _log.HMDpos = _mainCamera.transform.position;
        _log.HMDrot = _mainCamera.transform.rotation;
        DatabaseManager.instance.UpdateDataLog(_log);
    }


    private void OnApplicationQuit()
    {
        DatabaseManager.instance.StopDataLog();
    }
}