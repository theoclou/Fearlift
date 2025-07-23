using System;

[System.Serializable]
public struct HeartRateSample
{
    public float timestamp;
    public int bpm;
    public string sceneName;

    public HeartRateSample(float time, int bpm, string sceneName)
    {
        this.timestamp = time;
        this.bpm = bpm;
        this.sceneName = sceneName;
    }
}
