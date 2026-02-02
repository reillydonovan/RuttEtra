using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Audio reactive system for Rutt/Etra. Captures microphone input,
/// performs FFT analysis, beat detection, and modulates parameters.
/// </summary>
public class AudioReactive : MonoBehaviour
{
    [Header("Audio Input")]
    public bool enableAudio = true;
    public int selectedDeviceIndex = 0;
    [Range(0.1f, 10f)] public float inputGain = 1f;
    [Range(0f, 1f)] public float smoothing = 0.8f;
    
    [Header("FFT Settings")]
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    [Range(64, 8192)] public int fftSize = 1024;
    
    [Header("Frequency Bands (Read Only)")]
    [Range(0f, 1f)] public float bass;      // 20-250 Hz
    [Range(0f, 1f)] public float lowMid;    // 250-500 Hz
    [Range(0f, 1f)] public float mid;       // 500-2000 Hz
    [Range(0f, 1f)] public float highMid;   // 2000-4000 Hz
    [Range(0f, 1f)] public float treble;    // 4000-20000 Hz
    [Range(0f, 1f)] public float overall;   // Overall volume
    
    [Header("Beat Detection")]
    public bool beatDetected;
    [Range(1f, 3f)] public float beatThreshold = 1.5f;
    [Range(0.1f, 0.5f)] public float beatCooldown = 0.15f;
    public event Action OnBeat;
    
    [Header("Parameter Mapping")]
    public RuttEtraSettings settings;
    
    [Header("Displacement Mapping")]
    public bool modulateDisplacement = false;
    public AudioBand displacementBand = AudioBand.Bass;
    [Range(0f, 3f)] public float displacementAmount = 1f;
    
    [Header("Wave Mapping")]
    public bool modulateWave = false;
    public AudioBand waveBand = AudioBand.Mid;
    [Range(0f, 2f)] public float waveAmount = 0.5f;
    
    [Header("Color Mapping")]
    public bool modulateHue = false;
    public AudioBand hueBand = AudioBand.Treble;
    [Range(0f, 1f)] public float hueAmount = 0.3f;
    
    [Header("Scale Mapping")]
    public bool modulateScale = false;
    public AudioBand scaleBand = AudioBand.Overall;
    [Range(0f, 0.5f)] public float scaleAmount = 0.2f;
    
    [Header("Rotation Mapping")]
    public bool modulateRotation = false;
    public AudioBand rotationBand = AudioBand.LowMid;
    [Range(0f, 30f)] public float rotationAmount = 10f;
    
    [Header("Glow Mapping")]
    public bool modulateGlow = false;
    public AudioBand glowBand = AudioBand.HighMid;
    [Range(0f, 2f)] public float glowAmount = 1f;
    
    [Header("Beat Effects")]
    public bool flashOnBeat = false;
    public bool pulseOnBeat = false;
    [Range(0f, 1f)] public float beatIntensity = 0.5f;
    
    // Public properties
    public string[] AvailableDevices => Microphone.devices;
    public string CurrentDevice => selectedDeviceIndex < Microphone.devices.Length ? Microphone.devices[selectedDeviceIndex] : "";
    public bool IsRecording => _audioSource != null && _audioSource.isPlaying;
    
    // Events
    public event Action<string[]> OnDevicesChanged;
    public event Action<string> OnDeviceSelected;
    
    // Private
    private AudioSource _audioSource;
    private AudioClip _micClip;
    private float[] _samples;
    private float[] _spectrum;
    private float[] _bandValues;
    private float[] _bandSmoothed;
    private float[] _bandHistory;
    private int _historyIndex;
    private const int HistorySize = 43; // ~1 second at 60fps
    private float _lastBeatTime;
    private float _beatPulse;
    private string[] _lastDevices;
    
    // Base values for modulation
    private float _baseDisplacement;
    private float _baseWaveH, _baseWaveV;
    private float _baseHue;
    private float _baseScale;
    private float _baseRotY;
    private float _baseGlow;
    private bool _basesCaptured;
    
    public enum AudioBand { Bass, LowMid, Mid, HighMid, Treble, Overall }
    
    private void Awake()
    {
        _samples = new float[fftSize];
        _spectrum = new float[fftSize];
        _bandValues = new float[6];
        _bandSmoothed = new float[6];
        _bandHistory = new float[HistorySize];
    }
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
        
        RefreshDevices();
        
        if (enableAudio && Microphone.devices.Length > 0)
        {
            StartMicrophone();
        }
    }
    
    /// <summary>
    /// Get list of available audio input devices
    /// </summary>
    public List<string> GetDeviceList()
    {
        var devices = new List<string>();
        foreach (var device in Microphone.devices)
        {
            devices.Add(device);
        }
        if (devices.Count == 0)
        {
            devices.Add("No devices found");
        }
        return devices;
    }
    
    /// <summary>
    /// Refresh the list of available devices
    /// </summary>
    public void RefreshDevices()
    {
        if (_lastDevices == null || !ArraysEqual(_lastDevices, Microphone.devices))
        {
            _lastDevices = (string[])Microphone.devices.Clone();
            OnDevicesChanged?.Invoke(Microphone.devices);
            Debug.Log($"Audio devices: {string.Join(", ", Microphone.devices)}");
        }
    }
    
    private bool ArraysEqual(string[] a, string[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
    
    /// <summary>
    /// Select an audio device by index
    /// </summary>
    public void SelectDevice(int index)
    {
        if (index < 0 || index >= Microphone.devices.Length)
        {
            Debug.LogWarning($"Invalid device index: {index}");
            return;
        }
        
        selectedDeviceIndex = index;
        string deviceName = Microphone.devices[index];
        Debug.Log($"Selected audio device: {deviceName}");
        
        // Restart with new device
        if (enableAudio)
        {
            StopMicrophone();
            StartMicrophone();
        }
        
        OnDeviceSelected?.Invoke(deviceName);
    }
    
    /// <summary>
    /// Select an audio device by name
    /// </summary>
    public void SelectDeviceByName(string deviceName)
    {
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            if (Microphone.devices[i] == deviceName)
            {
                SelectDevice(i);
                return;
            }
        }
        Debug.LogWarning($"Device not found: {deviceName}");
    }
    
    public void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone found");
            return;
        }
        
        // Ensure valid device index
        if (selectedDeviceIndex >= Microphone.devices.Length)
        {
            selectedDeviceIndex = 0;
        }
        
        string device = Microphone.devices[selectedDeviceIndex];
        
        // Stop any existing recording
        if (Microphone.IsRecording(device))
        {
            Microphone.End(device);
        }
        
        // Also stop recording on all other devices
        foreach (var d in Microphone.devices)
        {
            if (Microphone.IsRecording(d))
            {
                Microphone.End(d);
            }
        }
        
        // Start new recording
        _micClip = Microphone.Start(device, true, 1, AudioSettings.outputSampleRate);
        
        // Wait for mic to start (with timeout)
        int timeout = 0;
        while (Microphone.GetPosition(device) <= 0 && timeout < 1000)
        {
            timeout++;
            System.Threading.Thread.Sleep(1);
        }
        
        if (timeout >= 1000)
        {
            Debug.LogWarning($"Timeout waiting for microphone: {device}");
            return;
        }
        
        _audioSource.clip = _micClip;
        _audioSource.Play();
        _audioSource.volume = 0; // Mute playback, we just want the data
        
        Debug.Log($"Started microphone: {device}");
    }
    
    public void StopMicrophone()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
        
        foreach (var device in Microphone.devices)
        {
            if (Microphone.IsRecording(device))
            {
                Microphone.End(device);
            }
        }
    }
    
    public void SetEnabled(bool enabled)
    {
        enableAudio = enabled;
        if (enabled)
        {
            StartMicrophone();
        }
        else
        {
            StopMicrophone();
        }
    }
    
    private void Update()
    {
        // Periodically check for device changes
        if (Time.frameCount % 300 == 0)
        {
            RefreshDevices();
        }
        
        if (!enableAudio || _audioSource == null || !_audioSource.isPlaying) return;
        
        CaptureBaseValues();
        AnalyzeAudio();
        DetectBeat();
        UpdateBeatPulse();
        ApplyModulation();
    }
    
    private void CaptureBaseValues()
    {
        if (_basesCaptured || settings == null) return;
        
        _baseDisplacement = settings.displacementStrength;
        _baseWaveH = settings.horizontalWave;
        _baseWaveV = settings.verticalWave;
        Color.RGBToHSV(settings.primaryColor, out _baseHue, out _, out _);
        _baseScale = settings.meshScale;
        _baseRotY = settings.rotationY;
        _baseGlow = settings.glowIntensity;
        _basesCaptured = true;
    }
    
    public void RecaptureBaseValues()
    {
        _basesCaptured = false;
    }
    
    private void AnalyzeAudio()
    {
        if (_samples == null || _spectrum == null) return;
        
        // Get audio samples
        _audioSource.GetOutputData(_samples, 0);
        
        // Calculate overall volume (RMS)
        float sum = 0;
        for (int i = 0; i < _samples.Length; i++)
            sum += _samples[i] * _samples[i];
        float rms = Mathf.Sqrt(sum / _samples.Length) * inputGain;
        
        // Get spectrum data
        _audioSource.GetSpectrumData(_spectrum, 0, fftWindow);
        
        // Calculate frequency bands
        // Bass: 20-250 Hz
        _bandValues[0] = GetFrequencyBandAverage(20, 250);
        // Low Mid: 250-500 Hz
        _bandValues[1] = GetFrequencyBandAverage(250, 500);
        // Mid: 500-2000 Hz
        _bandValues[2] = GetFrequencyBandAverage(500, 2000);
        // High Mid: 2000-4000 Hz
        _bandValues[3] = GetFrequencyBandAverage(2000, 4000);
        // Treble: 4000-20000 Hz
        _bandValues[4] = GetFrequencyBandAverage(4000, 20000);
        // Overall
        _bandValues[5] = rms;
        
        // Apply gain and smoothing
        for (int i = 0; i < _bandValues.Length; i++)
        {
            _bandValues[i] *= inputGain;
            _bandSmoothed[i] = Mathf.Lerp(_bandValues[i], _bandSmoothed[i], smoothing);
        }
        
        // Update public values
        bass = _bandSmoothed[0];
        lowMid = _bandSmoothed[1];
        mid = _bandSmoothed[2];
        highMid = _bandSmoothed[3];
        treble = _bandSmoothed[4];
        overall = _bandSmoothed[5];
    }
    
    private float GetFrequencyBandAverage(float minFreq, float maxFreq)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        float freqPerBin = sampleRate / 2f / _spectrum.Length;
        
        int minBin = Mathf.Max(0, Mathf.FloorToInt(minFreq / freqPerBin));
        int maxBin = Mathf.Min(_spectrum.Length - 1, Mathf.FloorToInt(maxFreq / freqPerBin));
        
        float sum = 0;
        int count = 0;
        for (int i = minBin; i <= maxBin; i++)
        {
            sum += _spectrum[i];
            count++;
        }
        
        return count > 0 ? sum / count * 10f : 0f; // Scale up for visibility
    }
    
    private void DetectBeat()
    {
        // Use bass for beat detection
        float current = bass;
        
        // Add to history
        _bandHistory[_historyIndex] = current;
        _historyIndex = (_historyIndex + 1) % HistorySize;
        
        // Calculate average
        float avg = 0;
        for (int i = 0; i < HistorySize; i++)
            avg += _bandHistory[i];
        avg /= HistorySize;
        
        // Detect beat
        beatDetected = false;
        
        if (Time.time - _lastBeatTime > beatCooldown)
        {
            if (current > avg * beatThreshold && current > 0.1f)
            {
                beatDetected = true;
                _lastBeatTime = Time.time;
                _beatPulse = 1f;
                OnBeat?.Invoke();
            }
        }
    }
    
    private void UpdateBeatPulse()
    {
        _beatPulse = Mathf.MoveTowards(_beatPulse, 0f, Time.deltaTime * 5f);
    }
    
    private void ApplyModulation()
    {
        if (settings == null) return;
        
        // Displacement
        if (modulateDisplacement)
        {
            float mod = GetBandValue(displacementBand) * displacementAmount;
            settings.displacementStrength = _baseDisplacement + mod;
        }
        
        // Wave
        if (modulateWave)
        {
            float mod = GetBandValue(waveBand) * waveAmount;
            settings.horizontalWave = Mathf.Max(0, _baseWaveH + mod);
            settings.verticalWave = Mathf.Max(0, _baseWaveV + mod * 0.7f);
        }
        
        // Hue
        if (modulateHue)
        {
            float mod = GetBandValue(hueBand) * hueAmount;
            float h = (_baseHue + mod) % 1f;
            Color.RGBToHSV(settings.primaryColor, out _, out float s, out float v);
            settings.primaryColor = Color.HSVToRGB(h, s, v);
        }
        
        // Scale
        if (modulateScale)
        {
            float mod = GetBandValue(scaleBand) * scaleAmount;
            settings.meshScale = Mathf.Max(0.1f, _baseScale + mod);
        }
        
        // Rotation
        if (modulateRotation)
        {
            float mod = GetBandValue(rotationBand) * rotationAmount;
            settings.rotationY = _baseRotY + mod;
        }
        
        // Glow
        if (modulateGlow)
        {
            float mod = GetBandValue(glowBand) * glowAmount;
            settings.glowIntensity = Mathf.Max(0, _baseGlow + mod);
        }
        
        // Beat effects
        if (flashOnBeat && _beatPulse > 0)
        {
            settings.brightness = Mathf.Lerp(settings.brightness, beatIntensity, _beatPulse);
        }
        
        if (pulseOnBeat && _beatPulse > 0)
        {
            settings.meshScale = Mathf.Lerp(settings.meshScale, _baseScale + beatIntensity * 0.3f, _beatPulse);
        }
    }
    
    public float GetBandValue(AudioBand band)
    {
        return band switch
        {
            AudioBand.Bass => bass,
            AudioBand.LowMid => lowMid,
            AudioBand.Mid => mid,
            AudioBand.HighMid => highMid,
            AudioBand.Treble => treble,
            AudioBand.Overall => overall,
            _ => 0f
        };
    }
    
    private void OnDestroy()
    {
        StopMicrophone();
    }
    
    private void OnDisable()
    {
        StopMicrophone();
    }
    
    private void OnEnable()
    {
        if (enableAudio && _audioSource != null && !_audioSource.isPlaying)
        {
            StartMicrophone();
        }
    }
}
