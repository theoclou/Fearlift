using UnityEngine;

public class WindForce : MonoBehaviour
{
    public Vector3 windDirection = new Vector3(1, 0, 0);
    public float windStrength = 10f;

    void FixedUpdate()
    {
        foreach (Rigidbody rb in FindObjectsOfType<Rigidbody>())
        {
            rb.AddForce(windDirection.normalized * windStrength);
        }
    }
}
