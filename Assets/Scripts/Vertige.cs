using UnityEngine;

public class VertigeEffect : MonoBehaviour
{
    public float fearZoneY = 5f; // hauteur seuil où l'effet s'active
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 10f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (transform.position.y < fearZoneY)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
}
