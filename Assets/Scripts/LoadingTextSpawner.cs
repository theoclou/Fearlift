// Exemple de script pour créer et placer dynamiquement le texte
using TMPro;
using UnityEngine;

public class LoadingTextSpawner : MonoBehaviour
{
    public GameObject textObj;
    public TextMeshPro textMesh;

    void Start()
    {
        textMesh.text = "Loading the next scene...";
        textMesh.fontSize = 1.5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;

        // Positionné à 2m devant la caméra
        Camera cam = Camera.main;
        textObj.transform.position = cam.transform.position + cam.transform.forward * 2f;
        textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - cam.transform.position);
    }
}
