using UnityEngine;
using UnityEngine.UI;

public class SliderTrigger : MonoBehaviour
{
    public Slider slider;
    public GameObject imageToShow;
    public AudioSource audioSource;
    public Transform playerHead; // <-- La caméra XR ici
    private bool soundPlayed = false;
    private bool moving = false;

    public float moveSpeed = 1.0f;
    public float stopDistance = 0.5f;

    void Update()
    {
        if (slider.value >= slider.maxValue && !soundPlayed)
        {
            imageToShow.SetActive(true);
            audioSource.Play();
            soundPlayed = true;
            moving = true;
        }
        else if (slider.value < slider.maxValue)
        {
            imageToShow.SetActive(false);
            soundPlayed = false;
            moving = false;
        }

        // Faire avancer l’image vers la tête du joueur
        if (moving && imageToShow.activeSelf)
        {
            Vector3 direction = (playerHead.position - imageToShow.transform.position).normalized;
            float distance = Vector3.Distance(imageToShow.transform.position, playerHead.position);

            if (distance > stopDistance)
            {
                imageToShow.transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }
    }
}
