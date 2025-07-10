using UnityEngine;
using UnityEditor;

public class ApplyMaterialToPlanksEditor : MonoBehaviour
{
    [MenuItem("Tools/Apply Material To All Planks")]
    static void ApplyMaterial()
    {
        Material mat = Selection.activeObject as Material;
        if (mat == null)
        {
            Debug.LogError("Sélectionne un matériau dans le Project.");
            return;
        }

        GameObject planksGroup = GameObject.Find("Planches"); // adapte le nom si différent
        if (planksGroup == null)
        {
            Debug.LogError("Objet 'Planches' introuvable.");
            return;
        }

        foreach (Transform plank in planksGroup.transform)
        {
            MeshRenderer rend = plank.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.sharedMaterial = mat; // sharedMaterial pour que ça reste dans l’éditeur
                EditorUtility.SetDirty(rend); // marquer modifié
            }
        }

        Debug.Log("✅ Matériau appliqué aux planches.");
    }
}
