// Exemple de script pour créer et placer dynamiquement le texte
using TMPro;
using UnityEngine;

public class LoadingTextSpawner2 : MonoBehaviour
{
    public GameObject textObj;
    public TextMeshPro textMesh;

    public float fontSize;

    void Start()
    {
        textMesh.text = "Bravo !!";
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;

        // Positionné à 2m devant la caméra
        Camera cam = Camera.main;
        textObj.transform.position = cam.transform.position + cam.transform.forward * 2f;
        textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - cam.transform.position);
    }
}
