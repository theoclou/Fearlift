using UnityEngine;

public class VRCameraCollision : MonoBehaviour
{

    [SerializeField] private Transform vrCamera;
    [SerializeField] private float headRadius = 0.15f;
    [SerializeField] private LayerMask collisionMask;

    private Vector3 previousSafePosition;

    void Start()
    {
        if (vrCamera == null)
            vrCamera = Camera.main.transform;

        previousSafePosition = vrCamera.position;
    }

    void LateUpdate()
    {
        // V�rifie s'il y a un obstacle autour de la t�te
        Collider[] hits = Physics.OverlapSphere(vrCamera.position, headRadius, collisionMask);

        if (hits.Length > 0)
        {
            // Collision d�tect�e : on repositionne la t�te � sa derni�re position s�re
            Vector3 offset = vrCamera.position - previousSafePosition;
            transform.position -= offset; // recule le XR Rig
        }
        else
        {
            // Pas de collision : c'est une position s�re
            previousSafePosition = vrCamera.position;
        }
    }
}
