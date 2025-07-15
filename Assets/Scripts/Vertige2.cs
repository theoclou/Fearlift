using UnityEngine;

public class VertigeLookDown : MonoBehaviour
{
    public Camera mainCam;
    public float minFOV = 50f;
    public float normalFOV = 70f;
    public float lookDownAngle = 60f;

    void Update()
    {
        float angle = Vector3.Angle(transform.forward, Vector3.down);

        if (angle < lookDownAngle)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, minFOV, Time.deltaTime * 2f);
        }
        else
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, normalFOV, Time.deltaTime * 2f);
        }
    }
}
