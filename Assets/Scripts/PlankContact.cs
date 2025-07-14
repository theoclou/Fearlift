using UnityEngine;

public class BridgeByYPosition : MonoBehaviour
{
    public Transform player;             // Référence vers le joueur (XR Rig)
    public Transform targetToMove;       // Ce qu'on bouge (ex: collider invisible)
    public float yThreshold = -1;      // Altitude seuil pour déclencher le mouvement
    public float moveDistance = 10f;    // Distance à descendre
    public float moveSpeed = 2f;         // Vitesse de déplacement

    private Vector3 initialPos;
    private Vector3 loweredPos;

    void Start()
    {
        initialPos = targetToMove.position;
        loweredPos = initialPos - new Vector3(0, moveDistance, 0);
    }

    void Update()
    {
        if (player.position.y <= yThreshold)
        {
            // Descendre
            targetToMove.position = Vector3.Lerp(targetToMove.position, loweredPos, Time.deltaTime * moveSpeed);
        }
        else
        {
            // Remonter
            targetToMove.position = Vector3.Lerp(targetToMove.position, initialPos, Time.deltaTime * moveSpeed);
        }
    }
}
