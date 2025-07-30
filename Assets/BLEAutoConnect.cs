using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class BLEHeartRateMonitor : MonoBehaviour
{
    string deviceId;
    string serviceId = "{0000180d-0000-1000-8000-00805f9b34fb}";
    string characteristicId = "{00002a37-0000-1000-8000-00805f9b34fb}";

    bool isScanning = false;
    bool isDeviceFound = false;
    bool isServiceFound = false;
    bool isCharacteristicFound = false;
    bool isSubscribed = false;
    string lastError;

    private string currentScene = "Unknown";

    public GameObject deviceScanResultProto;
    Transform scanResultRoot;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();

    public int heartRate;

    public static BLEHeartRateMonitor Instance;

    public List<HeartRateSample> heartRateHistory = new List<HeartRateSample>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Évite les doublons
        }
    }

    void Start()
    {
        heartRate = 0;
        scanResultRoot = deviceScanResultProto.transform.parent;
        deviceScanResultProto.transform.SetParent(null);
    }

    void Update()
    {

        BleApi.ScanStatus status;

        if (isScanning && !isDeviceFound)
        {
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                status = BleApi.PollDevice(ref res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (!devices.ContainsKey(res.id))
                        devices[res.id] = new Dictionary<string, string>() {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                    if (res.nameUpdated)
                        devices[res.id]["name"] = res.name;
                    if (res.isConnectableUpdated)
                        devices[res.id]["isConnectable"] = res.isConnectable.ToString();

                    if (devices[res.id]["name"] != "" && devices[res.id]["isConnectable"] == "True")
                    {
                        if (scanResultRoot.Find(res.id) == null) // 同じデバイスを複数追加しない
                        {
                            GameObject g = Instantiate(deviceScanResultProto, scanResultRoot);
                            g.name = res.id;
                            g.transform.GetChild(0).GetComponent<Text>().text = devices[res.id]["name"];
                            g.transform.GetChild(1).GetComponent<Text>().text = res.id;

                            // クリックイベント追加
                            Button btn = g.AddComponent<Button>();
                            btn.onClick.AddListener(() => SelectDevice(g));
                        }
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanning = false;
                    //Debug.Log("Device scanning finished.");
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        else if (isDeviceFound && !isServiceFound)
        {
            BleApi.Service svc;
            while ((status = BleApi.PollService(out svc, false)) == BleApi.ScanStatus.AVAILABLE)
            {
                if (svc.uuid == serviceId)
                {
                    isServiceFound = true;
                    BleApi.ScanCharacteristics(deviceId, serviceId);
                    //Debug.Log("Service found. Scanning characteristics...");
                }
            }
        }
        else if (isServiceFound && !isCharacteristicFound)
        {
            BleApi.Characteristic ch;
            while ((status = BleApi.PollCharacteristic(out ch, false)) == BleApi.ScanStatus.AVAILABLE)
            {
                if (ch.uuid == characteristicId)
                {
                    isCharacteristicFound = true;
                    Subscribe();
                    //Debug.Log("Characteristic found and subscribed.");
                }
            }
        }
        else if (isSubscribed)
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                heartRate = ParseHeartRate(res.buf);
                if (heartRate > 0)
                    //Debug.Log($"❤️ Heart Rate: {heartRate} bpm");
                    currentScene = SceneManager.GetActiveScene().name;

                    heartRateHistory.Add(new HeartRateSample(Time.time, heartRate, currentScene));
            }
        }

        BleApi.ErrorMessage err = new BleApi.ErrorMessage();
        BleApi.GetError(out err);
        if (lastError != err.msg && err.msg != "")
        {
            //Debug.LogError(err.msg);
            lastError = err.msg;
        }
    }

    public void StartScan()
    {
        ClearDeviceList();
        devices.Clear();

        isScanning = true;
        isDeviceFound = false;
        isServiceFound = false;
        isCharacteristicFound = false;
        isSubscribed = false;

        BleApi.StartDeviceScan();
        //Debug.Log("BLE scan started.");
    }

    void Subscribe()
    {
        BleApi.SubscribeCharacteristic(deviceId, serviceId, characteristicId, false);
        isSubscribed = true;
        //Debug.Log("Subscribed to Heart Rate Measurement.");
    }

    int ParseHeartRate(byte[] data)
    {
        if (data.Length < 2) return -1;

        bool isUINT16 = (data[0] & 0x01) != 0;
        return isUINT16 ? (data[1] | (data[2] << 8)) : data[1];
    }

    public void MarkSceneChange(string sceneName)
    {
        // Utilise un BPM spécial comme marqueur non physiologique (ex: -1)
        string currentScene = SceneManager.GetActiveScene().name;
        heartRateHistory.Add(new HeartRateSample(Time.time, -1, currentScene ));
        //Debug.Log($"📍 Scene changed to: {sceneName} at {Time.time}s");
    }

    public void SelectDevice(GameObject selected)
    {
        deviceId = selected.name;
        isDeviceFound = true;
        isScanning = false;

        //Debug.Log($"Selected device: {devices[deviceId]["name"]} [{deviceId}]");

        // UI上で選択状態を示す
        for (int i = 0; i < scanResultRoot.childCount; i++)
        {
            var child = scanResultRoot.GetChild(i).gameObject;
            child.transform.GetChild(0).GetComponent<Text>().color =
                child == selected ? Color.red : Color.black;
        }

        BleApi.StopDeviceScan();
        BleApi.ScanServices(deviceId);
        //Debug.Log("Service scanning started...");

        //Launch the menu
        MarkSceneChange("Menu");
        SceneManager.LoadScene("Menu");
    }

    void ClearDeviceList()
    {
        foreach (Transform child in scanResultRoot)
            Destroy(child.gameObject);
    }


    void OnApplicationQuit()
    {
        BleApi.Quit();

        string path = Path.Combine(Application.persistentDataPath, "HeartRateData.csv");
        CsvExporter.ExportToCSV(heartRateHistory, path);
        //Debug.Log("Heart rate data exported to " + path);
    }
}


public class CsvExporter
{
    public static void ExportToCSV(List<HeartRateSample> data, string filePath)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Timestamp;BPM;Scene");

        foreach (var sample in data)
        {
            string time = sample.timestamp.ToString(CultureInfo.InvariantCulture);
            sb.AppendLine($"{time};{sample.bpm};{sample.sceneName}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}