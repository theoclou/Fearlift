using UnityEngine;
using ViveSR.anipal.Eye;

public class GazeRaycast : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField] LayerMask videLayer;
    [SerializeField] float gazeRayLength = 20f;

    private EyeData_v2 _eyeData = new EyeData_v2();
    public bool LooksAtVoid { get; private set; } = false;

    void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;

        SRanipal_Eye_API.GetEyeData_v2(ref _eyeData);
        VerboseData verboseData = _eyeData.verbose_data;

        Vector3 gazeOrigin = verboseData.left.gaze_origin_mm * 0.001f;
        Vector3 gazeDirection = verboseData.left.gaze_direction_normalized;

        Vector3 worldOrigin = mainCamera.transform.TransformPoint(gazeOrigin);
        Vector3 worldDirection = mainCamera.transform.TransformDirection(gazeDirection);

        if (Physics.Raycast(worldOrigin, worldDirection, out RaycastHit hit, gazeRayLength, videLayer))
        {
            LooksAtVoid = true;
            Debug.DrawRay(worldOrigin, worldDirection * gazeRayLength, Color.red);
        }
        else
        {
            LooksAtVoid = false;
            Debug.DrawRay(worldOrigin, worldDirection * gazeRayLength, Color.green);
        }
    }
}
