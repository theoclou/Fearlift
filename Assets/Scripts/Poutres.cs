using UnityEngine;

public class AutoSwing : MonoBehaviour
{
    public float swingSpeed = 0.1f;
    public float swingAmount = 0.5f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAmount;

        // Bascule gauche/droite autour de l'axe Z local
        transform.rotation = startRotation * Quaternion.Euler(0, 0, angle);

        // Animation fluide
        swingSpeed = Mathf.Lerp(swingSpeed, swingSpeed + 0.01f, Time.deltaTime * 0.1f);
        swingAmount = Mathf.Lerp(swingAmount, swingAmount + 0.01f, Time.deltaTime * 0.1f);

        swingSpeed = Mathf.Clamp(swingSpeed, 0.1f, 5f);
        swingAmount = Mathf.Clamp(swingAmount, 0.1f, 10f);
    }
}
