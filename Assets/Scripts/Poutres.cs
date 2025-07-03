using UnityEngine;

public class AutoSwing : MonoBehaviour
{
    public float swingSpeed = 1f;
    public float swingAmount = 2f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAmount;
        transform.rotation = startRotation * Quaternion.Euler(angle, 0f, 0f);

        //augmente ou diminue progressivement swingSpeed et swingAmount tout en restant fluide  
        swingSpeed = Mathf.Lerp(swingSpeed, swingSpeed + 0.01f, Time.deltaTime * 0.1f);
        swingAmount = Mathf.Lerp(swingAmount, swingAmount + 0.01f, Time.deltaTime * 0.1f);

        // Assurez-vous que swingSpeed et swingAmount ne deviennent pas trop grands
        if (swingSpeed > 5f) swingSpeed = 5f;
        if (swingAmount > 10f) swingAmount = 10f;
        if (swingSpeed < 0.1f) swingSpeed = 0.1f;
        if (swingAmount < 0.1f) swingAmount = 0.1f;
    }
}
