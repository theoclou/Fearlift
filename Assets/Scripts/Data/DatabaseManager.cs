using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
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
    public bool myflag;
    public bool looksAtVoid; 
}
public class DatabaseManager : MonoBehaviour
{

    private string _folderPath = "ExportedData/";   

    [SerializeField]
    private bool _isTaskStart = false;
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
            if (_instance == null) _instance = new GameObject("DatabaseManager").AddComponent<DatabaseManager>();
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
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //void Update()
    //{
    //    // flag = artificialBlinkFlag.flag;
    //}
    public void StartDataLog()
    {
        _dataLog = new StringBuilder();
        _dataLog.AppendFormat("Time,HMD_Position_x,HMD_Position_y,HMD_Position_z,");
        _dataLog.AppendFormat("HMD_Rotation_x,HMD_Rotation_y,HMD_Rotation_z,HMD_Rotation_w,");
        _dataLog.AppendFormat("Left_Position_x,Left_Position_y,Left_Position_z,");
        _dataLog.AppendFormat("Right_Position_x,Right_Position_y,Right_Position_z,");
        _dataLog.AppendFormat("Left_Gaze_Origin.x,Left_Gaze_Origin.y,Left_Gaze_Origin.z,");
        _dataLog.AppendFormat("Right_Gaze_Origin.x,Right_Gaze_Origin.y,Right_Gaze_Origin.z,");
        _dataLog.AppendFormat("Left_Gaze_Direction_x,Left_Gaze_Direction_y,Left_Gaze_Direction_z,");
        _dataLog.AppendFormat("Right_Gaze_Direction_x,Right_Gaze_Direction_y,Right_Gaze_Direction_z,");
        _dataLog.AppendFormat("Left_Pupil_Diameter,Right_Pupil_Diameter,Left_Openness,Right_Openness,ArtificialBlink_Flag,RBlinkCount,LBlinkCount,BrowerFlag\n");
    }
    public void UpdateDataLog(DataStruct log)
    {
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
            }
        }
        else
        {
            leftflag = true;
        }
        _dataLog.AppendFormat("{0},{1},{2},{3},", log.time, log.HMDpos.x, log.HMDpos.y, log.HMDpos.z);
        _dataLog.AppendFormat("{0},{1},{2},{3},", log.HMDrot.x, log.HMDrot.y, log.HMDrot.z, log.HMDrot.w);
        _dataLog.AppendFormat("{0},{1},{2},", log.leftPosition.x, log.leftPosition.y, log.leftPosition.z);
        _dataLog.AppendFormat("{0},{1},{2},", log.rightPosition.x, log.rightPosition.y, log.rightPosition.z);
        _dataLog.AppendFormat("{0},{1},{2},", log.leftGazeOrigin.x, log.leftGazeOrigin.y, log.leftGazeOrigin.z);
        _dataLog.AppendFormat("{0},{1},{2},", log.rightGazeOrigin.x, log.rightGazeOrigin.y, log.rightGazeOrigin.z);
        _dataLog.AppendFormat("{0},{1},{2},", log.leftGazeDirection.x, log.leftGazeDirection.y, log.leftGazeDirection.z);
        _dataLog.AppendFormat("{0},{1},{2},", log.rightGazeDirection.x, log.rightGazeDirection.y, log.rightGazeDirection.z);
        _dataLog.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}\n", log.leftPupilDiameter, log.rightPupilDiameter, log.leftOpenness, log.rightOpenness, flagnum, Rblink, Lblink, log.myflag);
    }
    public void StopDataLog()
    {
        ExportData();
    }
    void ExportData()
    {
        string fileName = $"exportData_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!Directory.Exists(_folderPath))
        {
            Directory.CreateDirectory(_folderPath);
        }
        fileName = Path.Combine(_folderPath, fileName);
        _exFile = new StreamWriter(fileName);
        {
        };
        _exFile.WriteLine(_dataLog);
        _exFile.Flush();
        _exFile.Close();
    }
}