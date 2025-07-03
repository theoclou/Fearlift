using UnityEngine;
using TMPro;

public class WelcomeText : MonoBehaviour
{
    public GameObject welcomeCanvas;
    public TextMeshProUGUI welcomeText;
    public float displayTime = 5;

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
        welcomeText.text = "We are going to take a few mesurements, please step in the circle in front of you";
        Invoke(nameof(HideCanvas), displayTime);
    }
}