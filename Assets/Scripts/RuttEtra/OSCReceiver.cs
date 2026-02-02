using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// OSC (Open Sound Control) receiver for network-based parameter control.
/// Receives OSC messages and maps them to RuttEtra parameters.
/// 
/// OSC Address Format: /rutt/[parameter] [value]
/// Example: /rutt/displacement 1.5
/// </summary>
public class OSCReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    public bool enableOSC = true;
    public int listenPort = 9000;
    public bool logMessages = false;
    
    [Header("References")]
    public RuttEtraSettings settings;
    public AudioReactive audioReactive;
    public AnalogEffects analogEffects;
    
    // Events
    public event Action<string, float> OnFloatMessage;
    public event Action<string, bool> OnBoolMessage;
    public event Action<string, string> OnStringMessage;
    
    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _isRunning;
    private Queue<OSCMessage> _messageQueue = new Queue<OSCMessage>();
    private object _lock = new object();
    
    private struct OSCMessage
    {
        public string address;
        public object[] arguments;
    }
    
    private void Start()
    {
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
        
        if (audioReactive == null)
            audioReactive = FindFirstObjectByType<AudioReactive>();
        
        if (analogEffects == null)
            analogEffects = FindFirstObjectByType<AnalogEffects>();
        
        if (enableOSC)
            StartListening();
    }
    
    public void StartListening()
    {
        try
        {
            _udpClient = new UdpClient(listenPort);
            _isRunning = true;
            _receiveThread = new Thread(ReceiveData);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
            Debug.Log($"OSC: Listening on port {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"OSC: Failed to start - {e.Message}");
        }
    }
    
    public void StopListening()
    {
        _isRunning = false;
        _udpClient?.Close();
        _receiveThread?.Join(100);
    }
    
    private void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        
        while (_isRunning)
        {
            try
            {
                byte[] data = _udpClient.Receive(ref endPoint);
                ParseOSC(data);
            }
            catch (SocketException)
            {
                // Socket closed
            }
            catch (Exception e)
            {
                if (_isRunning)
                    Debug.LogError($"OSC: Receive error - {e.Message}");
            }
        }
    }
    
    private void ParseOSC(byte[] data)
    {
        try
        {
            int index = 0;
            string address = ReadString(data, ref index);
            
            // Align to 4 bytes
            while (index % 4 != 0) index++;
            
            // Read type tag
            string typeTag = ReadString(data, ref index);
            while (index % 4 != 0) index++;
            
            // Parse arguments
            List<object> args = new List<object>();
            
            if (typeTag.Length > 0 && typeTag[0] == ',')
            {
                for (int i = 1; i < typeTag.Length; i++)
                {
                    switch (typeTag[i])
                    {
                        case 'f': // Float
                            args.Add(ReadFloat(data, ref index));
                            break;
                        case 'i': // Int
                            args.Add(ReadInt(data, ref index));
                            break;
                        case 's': // String
                            args.Add(ReadString(data, ref index));
                            while (index % 4 != 0) index++;
                            break;
                        case 'T': // True
                            args.Add(true);
                            break;
                        case 'F': // False
                            args.Add(false);
                            break;
                    }
                }
            }
            
            lock (_lock)
            {
                _messageQueue.Enqueue(new OSCMessage { address = address, arguments = args.ToArray() });
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"OSC: Parse error - {e.Message}");
        }
    }
    
    private string ReadString(byte[] data, ref int index)
    {
        StringBuilder sb = new StringBuilder();
        while (index < data.Length && data[index] != 0)
        {
            sb.Append((char)data[index]);
            index++;
        }
        index++; // Skip null terminator
        return sb.ToString();
    }
    
    private float ReadFloat(byte[] data, ref int index)
    {
        byte[] bytes = new byte[4];
        Array.Copy(data, index, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        index += 4;
        return BitConverter.ToSingle(bytes, 0);
    }
    
    private int ReadInt(byte[] data, ref int index)
    {
        byte[] bytes = new byte[4];
        Array.Copy(data, index, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        index += 4;
        return BitConverter.ToInt32(bytes, 0);
    }
    
    private void Update()
    {
        ProcessMessages();
    }
    
    private void ProcessMessages()
    {
        lock (_lock)
        {
            while (_messageQueue.Count > 0)
            {
                var msg = _messageQueue.Dequeue();
                HandleMessage(msg.address, msg.arguments);
            }
        }
    }
    
    private void HandleMessage(string address, object[] args)
    {
        if (logMessages)
            Debug.Log($"OSC: {address} {string.Join(", ", args)}");
        
        // Fire events
        if (args.Length > 0)
        {
            if (args[0] is float f)
                OnFloatMessage?.Invoke(address, f);
            else if (args[0] is int i)
                OnFloatMessage?.Invoke(address, i);
            else if (args[0] is bool b)
                OnBoolMessage?.Invoke(address, b);
            else if (args[0] is string s)
                OnStringMessage?.Invoke(address, s);
        }
        
        // Built-in mappings
        if (settings != null && args.Length > 0)
        {
            float value = args[0] is float fv ? fv : (args[0] is int iv ? iv : 0f);
            bool boolVal = args[0] is bool bv ? bv : value > 0.5f;
            
            switch (address.ToLower())
            {
                // Displacement
                case "/rutt/displacement":
                case "/rutt/displacementstrength":
                    settings.displacementStrength = value;
                    break;
                case "/rutt/displacementoffset":
                case "/rutt/offset":
                    settings.displacementOffset = value;
                    break;
                case "/rutt/invert":
                    settings.invertDisplacement = boolVal;
                    break;
                
                // Input signal
                case "/rutt/brightness":
                    settings.brightness = value;
                    break;
                case "/rutt/contrast":
                    settings.contrast = value;
                    break;
                case "/rutt/threshold":
                    settings.threshold = value;
                    break;
                case "/rutt/gamma":
                    settings.gamma = value;
                    break;
                
                // Transform
                case "/rutt/rotationx":
                    settings.rotationX = value;
                    break;
                case "/rutt/rotationy":
                    settings.rotationY = value;
                    break;
                case "/rutt/rotationz":
                    settings.rotationZ = value;
                    break;
                case "/rutt/scale":
                    settings.meshScale = value;
                    break;
                case "/rutt/hscale":
                    settings.horizontalScale = value;
                    break;
                case "/rutt/vscale":
                    settings.verticalScale = value;
                    break;
                case "/rutt/hposition":
                    settings.horizontalPosition = value;
                    break;
                case "/rutt/vposition":
                    settings.verticalPosition = value;
                    break;
                
                // Wave
                case "/rutt/hwave":
                case "/rutt/horizontalwave":
                    settings.horizontalWave = value;
                    break;
                case "/rutt/vwave":
                case "/rutt/verticalwave":
                    settings.verticalWave = value;
                    break;
                case "/rutt/wavefrequency":
                    settings.waveFrequency = value;
                    break;
                case "/rutt/wavespeed":
                    settings.waveSpeed = value;
                    break;
                
                // Line style
                case "/rutt/linewidth":
                    settings.lineWidth = value;
                    break;
                case "/rutt/glow":
                    settings.glowIntensity = value;
                    break;
                case "/rutt/linetaper":
                    settings.lineTaper = value;
                    break;
                
                // Distortion
                case "/rutt/keystoneh":
                    settings.keystoneH = value;
                    break;
                case "/rutt/keystonev":
                    settings.keystoneV = value;
                    break;
                case "/rutt/barrel":
                    settings.barrelDistortion = value;
                    break;
                
                // Colors
                case "/rutt/colorblend":
                    settings.colorBlend = value;
                    break;
                case "/rutt/sourcecolor":
                    settings.useSourceColor = boolVal;
                    break;
                case "/rutt/primaryhue":
                    Color.RGBToHSV(settings.primaryColor, out _, out float ps, out float pv);
                    settings.primaryColor = Color.HSVToRGB(value, ps, pv);
                    break;
                case "/rutt/secondaryhue":
                    Color.RGBToHSV(settings.secondaryColor, out _, out float ss, out float sv);
                    settings.secondaryColor = Color.HSVToRGB(value, ss, sv);
                    break;
                
                // Post
                case "/rutt/noise":
                    settings.noiseAmount = value;
                    break;
                case "/rutt/persistence":
                    settings.persistence = value;
                    break;
                case "/rutt/flicker":
                    settings.scanlineFlicker = value;
                    break;
                case "/rutt/bloom":
                    settings.bloom = value;
                    break;
                
                // Scan lines
                case "/rutt/hlines":
                    settings.showHorizontalLines = boolVal;
                    break;
                case "/rutt/vlines":
                    settings.showVerticalLines = boolVal;
                    break;
                case "/rutt/interlace":
                    settings.interlace = boolVal;
                    break;
            }
        }
        
        // Audio reactive mappings
        if (audioReactive != null && args.Length > 0)
        {
            float value = args[0] is float fv ? fv : (args[0] is int iv ? iv : 0f);
            bool boolVal = args[0] is bool bv ? bv : value > 0.5f;
            
            switch (address.ToLower())
            {
                case "/rutt/audio/enable":
                    audioReactive.enableAudio = boolVal;
                    break;
                case "/rutt/audio/gain":
                    audioReactive.inputGain = value;
                    break;
                case "/rutt/audio/displacement":
                    audioReactive.modulateDisplacement = boolVal;
                    break;
                case "/rutt/audio/wave":
                    audioReactive.modulateWave = boolVal;
                    break;
                case "/rutt/audio/hue":
                    audioReactive.modulateHue = boolVal;
                    break;
            }
        }
        
        // Analog effects mappings
        if (analogEffects != null && args.Length > 0)
        {
            float value = args[0] is float fv ? fv : (args[0] is int iv ? iv : 0f);
            bool boolVal = args[0] is bool bv ? bv : value > 0.5f;
            
            switch (address.ToLower())
            {
                case "/rutt/crt/enable":
                    analogEffects.enableCRT = boolVal;
                    break;
                case "/rutt/crt/scanlines":
                    analogEffects.scanlineIntensity = value;
                    break;
                case "/rutt/vhs/enable":
                    analogEffects.enableVHS = boolVal;
                    break;
                case "/rutt/chromatic":
                    analogEffects.chromaticAmount = value;
                    break;
            }
        }
    }
    
    private void OnDestroy()
    {
        StopListening();
    }
    
    private void OnDisable()
    {
        StopListening();
    }
}
