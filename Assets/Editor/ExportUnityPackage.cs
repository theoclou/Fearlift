using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportUnityPackage
{
    [MenuItem("Tools/Exporter TerrainDemoScene_URP en .unitypackage")]
    public static void ExportTerrainDemo()
    {
        string exportPath = "Exports";
        string folderName = "TerrainDemoScene_URP";
        string packagePath = Path.Combine(exportPath, folderName + ".unitypackage");

        // Crée le dossier d'export si nécessaire
        if (!Directory.Exists(exportPath))
            Directory.CreateDirectory(exportPath);

        // Vérifie que le dossier existe
        if (!AssetDatabase.IsValidFolder("Assets/" + folderName))
        {
            EditorUtility.DisplayDialog("Erreur", $"Dossier 'Assets/{folderName}' introuvable.", "OK");
            return;
        }

        // Exporte le package
        AssetDatabase.ExportPackage("Assets/" + folderName, packagePath, ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);
        EditorUtility.DisplayDialog("Export terminé", $"Exporté vers :\n{packagePath}", "OK");

        // Ouvre le dossier export dans l’explorateur
        EditorUtility.RevealInFinder(packagePath);
    }
}
