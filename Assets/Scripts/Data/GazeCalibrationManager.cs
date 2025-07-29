using UnityEngine;
using System.Collections;

public class GazeCalibrationManager : MonoBehaviour
{
    public static GazeCalibrationManager Instance { get; private set; }

    [Header("Calibration Settings")]
    public Vector3 gazeOffset = Vector3.zero;
    public float horizontalMultiplier = 1.0f;
    public float verticalMultiplier = 1.0f;
    public bool enableGazeSmoothing = true;
    public float smoothingFactor = 0.8f;

    [Header("Calibration Process")]
    public bool isCalibrating = false;
    public int calibrationPointsCount = 5; // Centre + 4 coins
    public float calibrationPointDisplayTime = 2f;
    public GameObject calibrationPointPrefab;

    [Header("Canvas Mural Setup")]
    public Transform canvasTransform; // Référence au canvas mural pour la calibration
    public bool useCanvasForCalibration = true; // Si true, calibre sur le canvas, sinon dans l'espace libre

    // Variables internes pour la calibration
    private int currentCalibrationPoint = 0;
    private GameObject currentCalibrationObject;
    private Vector3[] calibrationPositions;
    private Vector3[] measuredGazeDirections;
    private Camera calibrationCamera;
    private GazeRaycast currentGazeRaycast;

    // Événements
    public System.Action OnCalibrationComplete;
    public System.Action<int, int> OnCalibrationProgress; // current, total
    public System.Action<string> OnCalibrationStatusChange; // status message

    private void Awake()
    {
        // Singleton pattern avec persistance entre scènes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCalibrationData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Chercher automatiquement le GazeRaycast et le canvas dans la scène
        FindSceneComponents();
        FindAndApplyToGazeRaycast();
    }

    private void OnEnable()
    {
        // S'assurer d'appliquer les paramètres quand l'objet devient actif
        FindSceneComponents();
        FindAndApplyToGazeRaycast();
    }

    private void FindSceneComponents()
    {
        // Chercher le canvas mural automatiquement si pas assigné
        if (canvasTransform == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    canvasTransform = canvas.transform;
                    Debug.Log($"Found WorldSpace canvas: {canvas.name}");
                    break;
                }
            }
        }
    }

    public void FindAndApplyToGazeRaycast()
    {
        // Chercher le GazeRaycast dans la scène actuelle
        GazeRaycast gazeRaycast = FindObjectOfType<GazeRaycast>();
        if (gazeRaycast != null)
        {
            ApplyCalibrationTo(gazeRaycast);
            Debug.Log("Calibration applied to GazeRaycast in scene");
        }
    }

    public void ApplyCalibrationTo(GazeRaycast gazeRaycast)
    {
        if (gazeRaycast != null)
        {
            gazeRaycast.SetCalibrationData(gazeOffset, horizontalMultiplier, verticalMultiplier, enableGazeSmoothing, smoothingFactor);
            currentGazeRaycast = gazeRaycast;
        }
    }

    public void StartCalibration(Camera targetCamera = null)
    {
        if (isCalibrating) return;

        calibrationCamera = targetCamera ?? Camera.main;
        if (calibrationCamera == null)
        {
            Debug.LogError("No camera found for calibration!");
            OnCalibrationStatusChange?.Invoke("Error: No camera found!");
            return;
        }

        Debug.Log("Starting gaze calibration...");
        OnCalibrationStatusChange?.Invoke("Starting calibration...");
        isCalibrating = true;
        currentCalibrationPoint = 0;

        // Définir les positions de calibration
        SetupCalibrationPositions();

        // Initialiser les tableaux
        measuredGazeDirections = new Vector3[calibrationPointsCount];

        // Commencer la calibration
        StartCoroutine(CalibrationProcess());
    }

    private void SetupCalibrationPositions()
    {
        calibrationPositions = new Vector3[calibrationPointsCount];

        if (useCanvasForCalibration && canvasTransform != null)
        {
            // Calibration sur le canvas mural
            Vector3 canvasCenter = canvasTransform.position;
            Vector3 canvasRight = canvasTransform.right;
            Vector3 canvasUp = canvasTransform.up;

            // Ajuster selon la taille du canvas
            float offsetX = 0.4f; // Ajustez selon la largeur de votre canvas
            float offsetY = 0.3f; // Ajustez selon la hauteur de votre canvas

            // Centre du canvas
            calibrationPositions[0] = canvasCenter;

            // 4 coins du canvas
            calibrationPositions[1] = canvasCenter - canvasRight * offsetX + canvasUp * offsetY;  // Haut-gauche
            calibrationPositions[2] = canvasCenter + canvasRight * offsetX + canvasUp * offsetY;  // Haut-droite
            calibrationPositions[3] = canvasCenter - canvasRight * offsetX - canvasUp * offsetY; // Bas-gauche
            calibrationPositions[4] = canvasCenter + canvasRight * offsetX - canvasUp * offsetY; // Bas-droite

            Debug.Log($"Calibration setup on canvas: {canvasTransform.name}");
        }
        else
        {
            // Calibration dans l'espace libre (méthode originale)
            float distance = 2f;
            Vector3 center = calibrationCamera.transform.position + calibrationCamera.transform.forward * distance;

            // Centre
            calibrationPositions[0] = center;

            // 4 coins
            float offsetX = 0.8f;
            float offsetY = 0.6f;

            calibrationPositions[1] = center + calibrationCamera.transform.right * -offsetX + calibrationCamera.transform.up * offsetY;
            calibrationPositions[2] = center + calibrationCamera.transform.right * offsetX + calibrationCamera.transform.up * offsetY;
            calibrationPositions[3] = center + calibrationCamera.transform.right * -offsetX + calibrationCamera.transform.up * -offsetY;
            calibrationPositions[4] = center + calibrationCamera.transform.right * offsetX + calibrationCamera.transform.up * -offsetY;

            Debug.Log("Calibration setup in free space");
        }
    }

    private IEnumerator CalibrationProcess()
    {
        for (int i = 0; i < calibrationPointsCount; i++)
        {
            currentCalibrationPoint = i;
            OnCalibrationProgress?.Invoke(i + 1, calibrationPointsCount);

            string pointName = GetCalibrationPointName(i);
            OnCalibrationStatusChange?.Invoke($"Look at: {pointName}");

            // Créer le point de calibration
            if (calibrationPointPrefab != null)
            {
                currentCalibrationObject = Instantiate(calibrationPointPrefab);
                currentCalibrationObject.transform.position = calibrationPositions[i];

                // Si c'est sur un canvas, ajuster la rotation pour face à la caméra
                if (useCanvasForCalibration && canvasTransform != null)
                {
                    Vector3 directionToCamera = calibrationCamera.transform.position - currentCalibrationObject.transform.position;
                    currentCalibrationObject.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }

            Debug.Log($"Look at calibration point {i + 1}/{calibrationPointsCount}: {pointName}");

            // Attendre que l'utilisateur regarde le point
            yield return new WaitForSeconds(calibrationPointDisplayTime);

            // Mesurer la direction du regard
            if (currentGazeRaycast != null)
            {
                measuredGazeDirections[i] = currentGazeRaycast.GetCurrentGazeDirection();
                Debug.Log($"Measured gaze direction {i}: {measuredGazeDirections[i]}");
            }

            // Détruire le point de calibration
            if (currentCalibrationObject != null)
            {
                Destroy(currentCalibrationObject);
            }

            yield return new WaitForSeconds(0.5f); // Petite pause entre les points
        }

        // Calculer la calibration
        OnCalibrationStatusChange?.Invoke("Calculating calibration...");
        CalculateCalibration();

        // Appliquer immédiatement
        FindAndApplyToGazeRaycast();

        // Sauvegarder
        SaveCalibrationData();

        isCalibrating = false;
        OnCalibrationComplete?.Invoke();
        OnCalibrationStatusChange?.Invoke("Calibration complete!");

        Debug.Log("Calibration complete!");
    }

    private string GetCalibrationPointName(int index)
    {
        switch (index)
        {
            case 0: return "Center";
            case 1: return "Top-Left";
            case 2: return "Top-Right";
            case 3: return "Bottom-Left";
            case 4: return "Bottom-Right";
            default: return $"Point {index + 1}";
        }
    }

    private void CalculateCalibration()
    {
        // Calculer l'offset moyen basé sur les erreurs mesurées
        Vector3 totalError = Vector3.zero;
        int validMeasurements = 0;

        for (int i = 0; i < calibrationPointsCount; i++)
        {
            if (measuredGazeDirections[i] != Vector3.zero)
            {
                Vector3 expectedDirection = (calibrationPositions[i] - calibrationCamera.transform.position).normalized;
                Vector3 actualDirection = measuredGazeDirections[i].normalized;

                Vector3 error = expectedDirection - actualDirection;
                totalError += error;
                validMeasurements++;

                Debug.Log($"Point {i}: Expected={expectedDirection}, Actual={actualDirection}, Error={error}");
            }
        }

        if (validMeasurements > 0)
        {
            Vector3 averageError = totalError / validMeasurements;

            // Convertir l'erreur en offset local à la caméra
            gazeOffset = calibrationCamera.transform.InverseTransformDirection(averageError);

            Debug.Log($"Calculated gaze offset: {gazeOffset} (from {validMeasurements} measurements)");
            Debug.Log($"Average error in world space: {averageError}");
        }
        else
        {
            Debug.LogWarning("No valid measurements for calibration!");
        }
    }

    public void SaveCalibrationData()
    {
        PlayerPrefs.SetFloat("GazeOffset_X", gazeOffset.x);
        PlayerPrefs.SetFloat("GazeOffset_Y", gazeOffset.y);
        PlayerPrefs.SetFloat("GazeOffset_Z", gazeOffset.z);
        PlayerPrefs.SetFloat("GazeHorizontalMultiplier", horizontalMultiplier);
        PlayerPrefs.SetFloat("GazeVerticalMultiplier", verticalMultiplier);
        PlayerPrefs.SetInt("GazeEnableSmoothing", enableGazeSmoothing ? 1 : 0);
        PlayerPrefs.SetFloat("GazeSmoothingFactor", smoothingFactor);
        PlayerPrefs.Save();

        Debug.Log("Calibration data saved!");
    }

    public void LoadCalibrationData()
    {
        if (PlayerPrefs.HasKey("GazeOffset_X"))
        {
            gazeOffset.x = PlayerPrefs.GetFloat("GazeOffset_X");
            gazeOffset.y = PlayerPrefs.GetFloat("GazeOffset_Y");
            gazeOffset.z = PlayerPrefs.GetFloat("GazeOffset_Z");
            horizontalMultiplier = PlayerPrefs.GetFloat("GazeHorizontalMultiplier", 1.0f);
            verticalMultiplier = PlayerPrefs.GetFloat("GazeVerticalMultiplier", 1.0f);
            enableGazeSmoothing = PlayerPrefs.GetInt("GazeEnableSmoothing", 1) == 1;
            smoothingFactor = PlayerPrefs.GetFloat("GazeSmoothingFactor", 0.8f);

            Debug.Log($"Calibration data loaded: Offset={gazeOffset}");
        }
        else
        {
            Debug.Log("No saved calibration data found");
        }
    }

    public bool IsCalibrated()
    {
        return PlayerPrefs.HasKey("GazeOffset_X");
    }

    [ContextMenu("Reset Calibration")]
    public void ResetCalibration()
    {
        gazeOffset = Vector3.zero;
        horizontalMultiplier = 1.0f;
        verticalMultiplier = 1.0f;
        enableGazeSmoothing = true;
        smoothingFactor = 0.8f;

        SaveCalibrationData();
        FindAndApplyToGazeRaycast();

        OnCalibrationStatusChange?.Invoke("Calibration reset!");
        Debug.Log("Calibration reset!");
    }

    [ContextMenu("Start Quick Calibration")]
    public void StartQuickCalibration()
    {
        StartCalibration();
    }

    public void StopCalibration()
    {
        if (isCalibrating)
        {
            isCalibrating = false;
            StopAllCoroutines();

            if (currentCalibrationObject != null)
            {
                Destroy(currentCalibrationObject);
            }

            OnCalibrationStatusChange?.Invoke("Calibration stopped");
            Debug.Log("Calibration stopped");
        }
    }

    // Méthodes utilitaires pour l'UI
    public string GetCalibrationStatus()
    {
        if (isCalibrating)
        {
            return $"Calibrating... ({currentCalibrationPoint + 1}/{calibrationPointsCount})";
        }
        else if (IsCalibrated())
        {
            return $"Calibrated - Offset: ({gazeOffset.x:F2}, {gazeOffset.y:F2}, {gazeOffset.z:F2})";
        }
        else
        {
            return "Not calibrated";
        }
    }
}