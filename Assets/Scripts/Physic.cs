using UnityEngine;

public class PlankPhysics : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Appliquer une petite force en fonction du point d'impact
            Vector3 forceDir = -transform.up + collision.relativeVelocity.normalized;
            rb.AddForce(forceDir * 10f, ForceMode.Impulse);
        }
    }
}
