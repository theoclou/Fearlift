using UnityEngine;

public class BridgeContactDetector : MonoBehaviour
{
    public Transform colliderToMove;      // Le collider invisible à déplacer
    public float moveDistance = 500f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private Vector3 loweredPos;
    private int plankContactCount = 0;

    void Start()
    {
        startPos = colliderToMove.position;
        loweredPos = startPos - new Vector3(0, moveDistance, 0);
    }

    void Update()
    {
        Vector3 target = (plankContactCount > 0) ? loweredPos : startPos;
        colliderToMove.position = Vector3.Lerp(colliderToMove.position, target, Time.deltaTime * moveSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BridgePlank"))
        {
            plankContactCount++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("BridgePlank"))
        {
            plankContactCount = Mathf.Max(0, plankContactCount - 1);
        }
    }
}
