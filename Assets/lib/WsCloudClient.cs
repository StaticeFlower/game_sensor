using System;
using UnityEngine;
using NativeWebSocket;

[Serializable] public class SensorMsg { public string type; public string mac; public int L; public int R; public bool online; }

/// <summary>Optional ESP32 relay input. Keyboard works when it is not configured.</summary>
public class WsCloudClient : MonoBehaviour
{
    public enum SensorProfile { DigitalButtons, AnalogPressure }
    [Header("Relay server")] public string serverHost = ""; public int serverPort = 443; public bool useSsl = true; public string deviceMac = "";
    [Header("Sensor")] public SensorProfile sensorProfile; public int analogRestValue; [Min(1)] public int analogActivationDelta = 120; [Range(0f,.5f)] public float smoothing = .08f;
    [Header("Reconnect")] public bool autoReconnect = true; public float reconnectDelay = 3f;
    public float MovementAxis { get; private set; }
    public bool IsOnline { get; private set; }
    public string ControlModeLabel => IsOnline ? (sensorProfile == SensorProfile.AnalogPressure ? "ESP32 Analog" : "ESP32 Dual") : "Keyboard";
    private WebSocket ws; private bool connecting, quitting; private float lastAttempt, targetAxis, axisVelocity;

    private string Url { get { string scheme = useSsl ? "wss" : "ws"; string port = (useSsl && serverPort == 443) || (!useSsl && serverPort == 80) ? "" : ":" + serverPort; return $"{scheme}://{serverHost}{port}/game?device={Uri.EscapeDataString(deviceMac)}"; } }
    private void Start() { ApplyUrlParameters(); if (!string.IsNullOrWhiteSpace(serverHost) && !string.IsNullOrWhiteSpace(deviceMac)) Connect(); }
    private async void Connect()
    {
        if (connecting || quitting || string.IsNullOrWhiteSpace(serverHost)) return;
        connecting = true; lastAttempt = Time.time; ws = new WebSocket(Url);
        ws.OnOpen += () => IsOnline = true; ws.OnError += _ => IsOnline = false; ws.OnClose += _ => IsOnline = false;
        ws.OnMessage += bytes => HandleMessage(System.Text.Encoding.UTF8.GetString(bytes));
        try { await ws.Connect(); } catch (Exception exception) { Debug.LogWarning("ESP32 relay unavailable: " + exception.Message); }
        connecting = false;
    }
    private void HandleMessage(string json)
    {
        SensorMsg data; try { data = JsonUtility.FromJson<SensorMsg>(json); } catch { return; }
        if (data == null) return; if (data.type == "device_status") { IsOnline = data.online; return; } if (data.type != "sensor") return;
        IsOnline = true;
        if (sensorProfile == SensorProfile.DigitalButtons) targetAxis = (data.L == 0 ? -1f : 0f) + (data.R == 0 ? 1f : 0f);
        else targetAxis = Mathf.Clamp((data.R - data.L) / (float)analogActivationDelta, -1f, 1f);
    }
    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
        if (autoReconnect && !quitting && !connecting && !IsOnline && !string.IsNullOrWhiteSpace(serverHost) && Time.time - lastAttempt >= reconnectDelay) Connect();
        
        // แก้ไข: ใช้ MovementAxis เป็นจุดเริ่ม เพื่อให้ SmoothDamp ต่อเนื่อง นุ่มนวลเท่ากันทั้ง ESP32 และ Keyboard
        MovementAxis = Mathf.SmoothDamp(MovementAxis, IsOnline ? targetAxis : 0f, ref axisVelocity, smoothing);
    }
    private void ApplyUrlParameters()
    {
        string url = Application.absoluteURL; if (string.IsNullOrEmpty(url) || !url.Contains("?")) return;
        foreach (string pair in url.Substring(url.IndexOf('?') + 1).Split('#')[0].Split('&'))
        { string[] kv = pair.Split(new[] { '=' }, 2); if (kv.Length != 2) continue; string key = Uri.UnescapeDataString(kv[0]), value = Uri.UnescapeDataString(kv[1]); if (key == "device") deviceMac = value; else if (key == "server") serverHost = value; else if (key == "profile") sensorProfile = value == "analog" ? SensorProfile.AnalogPressure : SensorProfile.DigitalButtons; }
    }
    private async void OnDestroy() { quitting = true; if (ws != null && ws.State == WebSocketState.Open) await ws.Close(); }
}