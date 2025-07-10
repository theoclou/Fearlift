using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AntiGravity : MonoBehaviour
{
    [Tooltip("Facteur de réduction de la gravité (1 = annule totalement, 0.5 = moitié)")]
    [Range(0f, 1f)]
    public float gravityFactor = 0.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.AddForce(-Physics.gravity * rb.mass * 0.8f, ForceMode.Force);
    }

}
