using UnityEngine;
using TMPro;

public class WelcomeText : MonoBehaviour
{
    public GameObject welcomeCanvas;
    public TextMeshProUGUI welcomeText;
    public float displayTime = 7;

    void Start()
    {
        welcomeCanvas.SetActive(true);
        Invoke(nameof(ChangeText), displayTime);
    }

    void HideCanvas()
    {
        welcomeCanvas.SetActive(false);
    }

    void ChangeText()
    {
        welcomeText.text = "Please step in the circle in front of you for measurements";
        Invoke(nameof(HideCanvas), displayTime);
    }
}