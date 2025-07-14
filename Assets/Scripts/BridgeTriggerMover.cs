using UnityEngine;

public class BridgeTriggerMover : MonoBehaviour
{
    public Transform colliderToMove;
    public float moveDistance = 4f;
    public float moveSpeed = 2f;

    private Vector3 initialPos;
    private Vector3 loweredPos;
    private int activePlankContacts = 0;

    void Start()
    {
        initialPos = colliderToMove.position;
        loweredPos = initialPos - new Vector3(0, moveDistance, 0);
    }

    void Update()
    {
        Vector3 targetPos = (activePlankContacts > 0) ? loweredPos : initialPos;
        colliderToMove.position = Vector3.Lerp(colliderToMove.position, targetPos, Time.deltaTime * moveSpeed);
        Debug.Log($"Number of active contacts: {activePlankContacts}, Target Position: {targetPos}");
    }

    public void OnPlayerStepOn()
    {
        activePlankContacts++;
    }

    public void OnPlayerStepOff()
    {
        activePlankContacts = Mathf.Max(0, activePlankContacts - 1);
    }
}
