using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct DataStruct
{
    public float time;
    public Vector3 HMDpos;
    public Quaternion HMDrot;
    public Vector3 leftPosition;
    public Vector3 rightPosition;
    public Vector3 leftGazeOrigin;
    public Vector3 rightGazeOrigin;
    public Vector3 leftGazeDirection;
    public Vector3 rightGazeDirection;
    public float leftPupilDiameter;
    public float rightPupilDiameter;
    public float leftOpenness;
    public float rightOpenness;
    public bool looksAtVoid;
}

public class DatabaseManager : MonoBehaviour
{
    private string _folderPath = "ExportedData";

    [SerializeField]
    private bool _isTaskStart = false;
    [SerializeField] private int blinkWindow = 30; // in seconds

    public bool isTaskStart
    {
        get { return _isTaskStart; }
        set { _isTaskStart = value; }
    }

    private StringBuilder _dataLog;
    private StreamWriter _exFile;
    private static DatabaseManager _instance = null;

    public static DatabaseManager instance
    {
        get
        {
            // Si l'instance n'existe pas, chercher dans la scène d'abord
            if (_instance == null)
            {
                _instance = FindObjectOfType<DatabaseManager>();

                // Si toujours pas trouvé, créer une nouvelle instance
                if (_instance == null)
                {
                    GameObject go = new GameObject("DatabaseManager");
                    _instance = go.AddComponent<DatabaseManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private bool flag = true;
    private int flagnum = 0;
    private int Rblink = 0;
    private int Lblink = 0;
    private bool leftflag = true;
    private bool rightflag = true;
    private string FileDate;
    private string NowTime;
    private int blinkPerMinute = 0;
    private bool isLoggingStarted = false;

    private List<float> rightBlinkTimes = new List<float>();
    private List<float> leftBlinkTimes = new List<float>();

    void Awake()
    {
        // Si une instance existe déjà et que ce n'est pas celle-ci, détruire ce GameObject
        if (_instance != null && _instance != this)
        {
            Debug.Log("DatabaseManager: Instance déjà existante, destruction de ce GameObject");
            Destroy(gameObject);
            return;
        }

        // Sinon, cette instance devient l'instance principale
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("DatabaseManager: Instance créée et conservée entre les scènes");
    }

    public void StartDataLog()
    {
        if (isLoggingStarted)
        {
            Debug.Log("DatabaseManager: Le logging est déjà démarré");
            return;
        }

        Debug.Log("DatabaseManager: Démarrage du logging des données");
        _dataLog = new StringBuilder();
        _dataLog.AppendFormat("Time,Scene,");
        _dataLog.AppendFormat("HMD_Position_x,HMD_Position_y,HMD_Position_z,");
        _dataLog.AppendFormat("HMD_Rotation_x,HMD_Rotation_y,HMD_Rotation_z,HMD_Rotation_w,");
        _dataLog.AppendFormat("Left_Position_x,Left_Position_y,Left_Position_z,");
        _dataLog.AppendFormat("Right_Position_x,Right_Position_y,Right_Position_z,");
        _dataLog.AppendFormat("Left_Gaze_Origin.x,Left_Gaze_Origin.y,Left_Gaze_Origin.z,");
        _dataLog.AppendFormat("Right_Gaze_Origin.x,Right_Gaze_Origin.y,Right_Gaze_Origin.z,");
        _dataLog.AppendFormat("Left_Gaze_Direction_x,Left_Gaze_Direction_y,Left_Gaze_Direction_z,");
        _dataLog.AppendFormat("Right_Gaze_Direction_x,Right_Gaze_Direction_y,Right_Gaze_Direction_z,");
        _dataLog.AppendFormat("Looks_At_Void,");
        _dataLog.AppendFormat("Left_Pupil_Diameter,Right_Pupil_Diameter,Left_Openness,Right_Openness,RBlinkCount,LBlinkCount,BlinkPerMinute\n");

        isLoggingStarted = true;
        _isTaskStart = true;

        // Réinitialiser les compteurs pour une nouvelle session
        Rblink = 0;
        Lblink = 0;
        rightBlinkTimes.Clear();
        leftBlinkTimes.Clear();
        blinkPerMinute = 0;
    }

    public void UpdateDataLog(DataStruct log)
    {
        if (!isLoggingStarted || _dataLog == null)
        {
            Debug.LogWarning("DatabaseManager: Tentative de mise à jour des données sans avoir démarré le logging");
            return;
        }

        // Utiliser la culture "en-US" pour forcer le point comme séparateur décimal
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        if (flag)
        {
            flagnum = 1;
        }
        else if (!flag)
        {
            flagnum = 0;
        }

        if (log.rightOpenness <= 0.5)
        {
            if (rightflag == true)
            {
                Rblink = Rblink + 1;
                rightflag = false;
                rightBlinkTimes.Add(log.time);
            }
        }
        else
        {
            rightflag = true;
        }

        if (log.leftOpenness <= 0.5)
        {
            if (leftflag == true)
            {
                Lblink = Lblink + 1;
                leftflag = false;
                leftBlinkTimes.Add(log.time);
            }
        }
        else
        {
            leftflag = true;
        }

        // Nettoyage des blinks hors fenêtre glissante (30s)
        float windowStart = log.time - blinkWindow;
        rightBlinkTimes.RemoveAll(t => t < windowStart);
        leftBlinkTimes.RemoveAll(t => t < windowStart);

        // Calcul du blink rate glissant (par minute)
        int rightCount = rightBlinkTimes.Count;
        int leftCount = leftBlinkTimes.Count;
        int minBlinkCount = Mathf.Min(rightCount, leftCount);
        blinkPerMinute = (int)((minBlinkCount * 60f) / blinkWindow);

        // Récupération de la Scène
        string currentScene = SceneManager.GetActiveScene().name;

        _dataLog.AppendFormat(culture, "{0},{1},", log.time, currentScene);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.HMDpos.x, log.HMDpos.y, log.HMDpos.z);
        _dataLog.AppendFormat(culture, "{0},{1},{2},{3},", log.HMDrot.x, log.HMDrot.y, log.HMDrot.z, log.HMDrot.w);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.leftPosition.x, log.leftPosition.y, log.leftPosition.z);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.rightPosition.x, log.rightPosition.y, log.rightPosition.z);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.leftGazeOrigin.x, log.leftGazeOrigin.y, log.leftGazeOrigin.z);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.rightGazeOrigin.x, log.rightGazeOrigin.y, log.rightGazeOrigin.z);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.leftGazeDirection.x, log.leftGazeDirection.y, log.leftGazeDirection.z);
        _dataLog.AppendFormat(culture, "{0},{1},{2},", log.rightGazeDirection.x, log.rightGazeDirection.y, log.rightGazeDirection.z);
        _dataLog.AppendFormat(culture, "{0},", log.looksAtVoid);
        _dataLog.AppendFormat(culture, "{0},{1},{2},{3},{4},{5},{6}\n", log.leftPupilDiameter, log.rightPupilDiameter, log.leftOpenness, log.rightOpenness, Rblink, Lblink, blinkPerMinute);
    }

    public void StopDataLog()
    {
        if (!isLoggingStarted)
        {
            Debug.Log("DatabaseManager: Le logging n'était pas démarré");
            return;
        }

        Debug.Log("DatabaseManager: Arrêt du logging et export des données");
        ExportData();
        isLoggingStarted = false;
        _isTaskStart = false;
    }

    void ExportData()
    {
        if (_dataLog == null || _dataLog.Length == 0)
        {
            Debug.LogWarning("DatabaseManager: Aucune donnée à exporter");
            return;
        }

        _folderPath = Path.Combine(Application.persistentDataPath, "ExportedData");
        string fileName = $"exportData_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!Directory.Exists(_folderPath))
        {
            Directory.CreateDirectory(_folderPath);
        }
        fileName = Path.Combine(_folderPath, fileName);

        try
        {
            _exFile = new StreamWriter(fileName);
            _exFile.WriteLine(_dataLog);
            _exFile.Flush();
            _exFile.Close();
            Debug.Log($"DatabaseManager: Données exportées vers {fileName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DatabaseManager: Erreur lors de l'export : {e.Message}");
        }
    }

    // Méthode pour nettoyer manuellement si nécessaire
    public void ResetLogging()
    {
        isLoggingStarted = false;
        _isTaskStart = false;
        if (_dataLog != null) _dataLog.Clear();
        Rblink = 0;
        Lblink = 0;
        rightBlinkTimes.Clear();
        leftBlinkTimes.Clear();
        blinkPerMinute = 0;
        Debug.Log("DatabaseManager: Logging réinitialisé");
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            Debug.Log("DatabaseManager: Instance principale détruite");
            _instance = null;
        }
    }
}