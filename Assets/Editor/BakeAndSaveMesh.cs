#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BakeAndSaveMesh
{
    [MenuItem("Tools/Bake & Save Skinned Mesh")]
    static void BakeAndSave()
    {
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Sélectionne un GameObject avec un SkinnedMeshRenderer.");
            return;
        }

        var smr = selected.GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogWarning("Pas de SkinnedMeshRenderer trouvé sur l'objet sélectionné.");
            return;
        }

        // Bake du mesh
        Mesh bakedMesh = new Mesh();
        smr.BakeMesh(bakedMesh);

        // Création d'un nouvel objet dans la scène
        GameObject newObj = new GameObject(selected.name + "_Baked");
        newObj.transform.position = selected.transform.position;
        newObj.transform.rotation = selected.transform.rotation;
        newObj.AddComponent<MeshFilter>().sharedMesh = bakedMesh;
        newObj.AddComponent<MeshRenderer>().sharedMaterial = smr.sharedMaterial;

        // Sauvegarde du mesh
        string path = EditorUtility.SaveFilePanelInProject("Sauvegarder le Mesh", selected.name + "_Mesh", "asset", "Choisis un nom pour sauvegarder le mesh");
        if (!string.IsNullOrEmpty(path))
        {
            Mesh copy = Object.Instantiate(bakedMesh);
            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();
            Debug.Log("✅ Mesh sauvegardé à : " + path);
        }
    }
}
#endif
