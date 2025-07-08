using UnityEngine;

public class CloudDrift : MonoBehaviour
{
    public Vector3 direction = new Vector3(0f, 0f, -0.05f);
    public float speed = 2.0f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}
