using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class HandClosed : MonoBehaviour
{
    [Header("Binding")]
    public InputActionReference gripButtonAction; // Drag votre action ici

    [Header("Meshes")]
    public MeshFilter targetMeshFilter;
    public Mesh newMesh;

    private Mesh originalMesh;
    private bool isSwapped;

    void Start()
    {
        // Sauvegarde du mesh original
        if (targetMeshFilter != null)
        {
            originalMesh = targetMeshFilter.sharedMesh;
            Debug.Log($"✅ Mesh original sauvegardé: {originalMesh?.name}");
        }
        else
        {
            Debug.LogError("❌ TargetMeshFilter est null !");
        }

        // Configuration de l'Input Action Reference
        if (gripButtonAction != null)
        {
            gripButtonAction.action.Enable();
            gripButtonAction.action.performed += OnButtonPressed;
            gripButtonAction.action.canceled += OnButtonReleased;

            Debug.Log($"✅ InputActionReference '{gripButtonAction.action.name}' activée.");
        }
        else
        {
            Debug.LogError("❌ GripButtonAction est null ! Assurez-vous de l'assigner dans l'inspecteur.");
        }
    }

    void OnDestroy()
    {
        if (gripButtonAction != null && gripButtonAction.action != null)
        {
            gripButtonAction.action.performed -= OnButtonPressed;
            gripButtonAction.action.canceled -= OnButtonReleased;
            gripButtonAction.action.Disable();
        }
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        if (!isSwapped && targetMeshFilter != null && newMesh != null)
        {
            Debug.Log("🟢 Bouton pressé → Mesh remplacé par newMesh");
            targetMeshFilter.sharedMesh = newMesh;
            isSwapped = true;
        }
    }

    private void OnButtonReleased(InputAction.CallbackContext context)
    {
        if (isSwapped && targetMeshFilter != null && originalMesh != null)
        {
            Debug.Log("⚪️ Bouton relâché → Mesh restauré");
            targetMeshFilter.sharedMesh = originalMesh;
            isSwapped = false;
        }
    }

    // Méthode de débogage (optionnelle - vous pouvez la supprimer si tout fonctionne)
    void Update()
    {
        if (gripButtonAction == null || gripButtonAction.action == null || targetMeshFilter == null)
            return;

        // Affichage de debug pour voir si l'input est détecté
        bool pressed = gripButtonAction.action.IsPressed();
        if (pressed && Time.frameCount % 30 == 0) // Log toutes les 30 frames pour éviter le spam
        {
            Debug.Log($"🔍 Bouton actuellement pressé: {pressed}");
        }
    }
}