using UnityEngine;

public class ApplyMaterialToPlanks : MonoBehaviour
{
    public Material plankMaterial; // Met le matériau ici dans l'inspecteur

    void Start()
    {
        foreach (Transform child in transform)
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = plankMaterial;
            }
        }

        Debug.Log("Matériau appliqué à toutes les planches.");
    }
}
