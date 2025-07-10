using UnityEngine;

public class ApplyMaterialToPlanks : MonoBehaviour
{
    public Material plankMaterial; // Met le mat�riau ici dans l'inspecteur

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

        Debug.Log("Mat�riau appliqu� � toutes les planches.");
    }
}
