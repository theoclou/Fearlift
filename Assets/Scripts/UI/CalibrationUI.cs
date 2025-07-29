using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalibrationUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startCalibrationButton;
    public Button resetCalibrationButton;
    public Button stopCalibrationButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI progressText;
    public Slider progressSlider;
    public GameObject calibrationPanel;

    [Header("Calibration Point")]
    public GameObject calibrationPointPrefab; // Sphere rouge ou autre objet visible

    private void Start()
    {
        // Setup UI
        if (startCalibrationButton != null)
            startCalibrationButton.onClick.AddListener(StartCalibration);

        if (resetCalibrationButton != null)
            resetCalibrationButton.onClick.AddListener(ResetCalibration);

        if (stopCalibrationButton != null)
            stopCalibrationButton.onClick.AddListener(StopCalibration);

        // S'abonner aux événements du manager
        if (GazeCalibrationManager.Instance != null)
        {
            GazeCalibrationManager.Instance.OnCalibrationProgress += UpdateProgress;
            GazeCalibrationManager.Instance.OnCalibrationComplete += OnCalibrationComplete;
        }

        // Assigner le prefab au manager s'il n'en a pas
        if (GazeCalibrationManager.Instance != null && calibrationPointPrefab != null)
        {
            GazeCalibrationManager.Instance.calibrationPointPrefab = calibrationPointPrefab;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        // Se désabonner des événements
        if (GazeCalibrationManager.Instance != null)
        {
            GazeCalibrationManager.Instance.OnCalibrationProgress -= UpdateProgress;
            GazeCalibrationManager.Instance.OnCalibrationComplete -= OnCalibrationComplete;
        }
    }

    public void StartCalibration()
    {
        if (GazeCalibrationManager.Instance != null)
        {
            GazeCalibrationManager.Instance.StartCalibration();
            UpdateUI();
        }
    }

    public void ResetCalibration()
    {
        if (GazeCalibrationManager.Instance != null)
        {
            GazeCalibrationManager.Instance.ResetCalibration();
            UpdateUI();
        }
    }

    public void StopCalibration()
    {
        if (GazeCalibrationManager.Instance != null)
        {
            GazeCalibrationManager.Instance.StopCalibration();
            UpdateUI();
        }
    }

    private void UpdateProgress(int current, int total)
    {
        if (progressText != null)
            progressText.text = $"Point {current}/{total}";

        if (progressSlider != null)
        {
            progressSlider.maxValue = total;
            progressSlider.value = current;
        }

        if (statusText != null)
            statusText.text = $"Look at calibration point {current}/{total}";
    }

    private void OnCalibrationComplete()
    {
        UpdateUI();

        if (statusText != null)
            statusText.text = "Calibration complete!";

        // Cacher le panel après quelques secondes
        if (calibrationPanel != null)
        {
            Invoke(nameof(HideCalibrationPanel), 2f);
        }
    }

    private void HideCalibrationPanel()
    {
        if (calibrationPanel != null)
            calibrationPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        bool isCalibrating = GazeCalibrationManager.Instance != null && GazeCalibrationManager.Instance.isCalibrating;

        if (startCalibrationButton != null)
            startCalibrationButton.interactable = !isCalibrating;

        if (resetCalibrationButton != null)
            resetCalibrationButton.interactable = !isCalibrating;

        if (stopCalibrationButton != null)
            stopCalibrationButton.interactable = isCalibrating;

        if (statusText != null && !isCalibrating)
        {
            if (GazeCalibrationManager.Instance != null)
            {
                Vector3 offset = GazeCalibrationManager.Instance.gazeOffset;
                statusText.text = $"Ready - Offset: ({offset.x:F2}, {offset.y:F2}, {offset.z:F2})";
            }
            else
            {
                statusText.text = "Ready - No calibration data";
            }
        }

        if (progressSlider != null && !isCalibrating)
        {
            progressSlider.value = 0;
        }
    }

    private void Update()
    {
        // Mise à jour continue de l'UI si nécessaire
        if (Time.frameCount % 30 == 0) // Toutes les 30 frames
        {
            UpdateUI();
        }
    }

    // Méthodes appelables depuis l'Inspector ou d'autres scripts
    [ContextMenu("Show Calibration Panel")]
    public void ShowCalibrationPanel()
    {
        if (calibrationPanel != null)
            calibrationPanel.SetActive(true);
    }

    [ContextMenu("Hide Calibration Panel")]
    public void HideCalibrationPanelManual()
    {
        if (calibrationPanel != null)
            calibrationPanel.SetActive(false);
    }
}