using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LookDownEffect : MonoBehaviour
{
    public Volume volume;
    public float angleTrigger = 60f;

    public float vignetteIntensity = 0.4f;
    public float saturation = -40f;
    public float shakeAmount = 0.05f;

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private Vector3 originalCamPos;

    void Start()
    {
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out colorAdjustments);
        originalCamPos = transform.localPosition;
    }

    void Update()
    {
        float angle = Vector3.Angle(transform.forward, Vector3.down);

        if (angle < angleTrigger)
        {
            // Active effets visuels
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, vignetteIntensity, Time.deltaTime * 3);
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, saturation, Time.deltaTime * 3);

            // Tremblement léger
            float offsetX = Mathf.Sin(Time.time * 20f) * shakeAmount;
            float offsetY = Mathf.Cos(Time.time * 25f) * shakeAmount;
            transform.localPosition = originalCamPos + new Vector3(offsetX, offsetY, 0);
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * 3);
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, 0f, Time.deltaTime * 3);
            transform.localPosition = originalCamPos;
        }
    }
}
