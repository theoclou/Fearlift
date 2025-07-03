using UnityEngine;

public class CarDestroyer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            Destroy(other.gameObject);
        }
    }
}
