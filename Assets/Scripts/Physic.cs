using UnityEngine;

public class PlankPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public Transform player; // Référence au XR Rig ou Main Camera
    public float activationDistance = 0.5f;
    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.isKinematic = true; // reste en place au début
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("❌ PlankPhysics: 'player' non assigné sur l'objet → " + gameObject.name);
            return;
        }

        if (!hasFallen && Vector3.Distance(transform.position, player.position) < activationDistance)
        {
            rb.isKinematic = false;
            rb.AddForce(-transform.up * 3f, ForceMode.Impulse);
            hasFallen = true;
        }
    }
}
