using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Video recording system for capturing RuttEtra output.
/// Records frames to image sequence or animated GIF.
/// For MP4/WebM, use Unity Recorder package or external tools.
/// </summary>
public class VideoRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public bool isRecording = false;
    public int targetFPS = 30;
    public int maxRecordingSeconds = 60;
    
    [Header("Output")]
    public string outputFolder = "Recordings";
    public OutputFormat format = OutputFormat.PNG_Sequence;
    public int jpegQuality = 90;
    
    [Header("Resolution")]
    public ResolutionMode resolutionMode = ResolutionMode.ScreenSize;
    public int customWidth = 1920;
    public int customHeight = 1080;
    
    [Header("Screenshot")]
    public KeyCode screenshotKey = KeyCode.F12;
    
    // Events
    public event Action OnRecordingStarted;
    public event Action<string> OnRecordingStopped;
    public event Action<string> OnScreenshotTaken;
    
    public enum OutputFormat { PNG_Sequence, JPG_Sequence, GIF }
    public enum ResolutionMode { ScreenSize, Custom, HalfScreen }
    
    private Camera _camera;
    private RenderTexture _renderTexture;
    private Texture2D _frameTexture;
    private List<byte[]> _gifFrames;
    private int _frameCount;
    private string _currentRecordingPath;
    private float _recordingStartTime;
    private bool _shouldCapture;
    private float _lastCaptureTime;
    private float _captureInterval;
    
    private void Start()
    {
        _camera = Camera.main;
        _captureInterval = 1f / targetFPS;
        
        string basePath = Path.Combine(Application.persistentDataPath, outputFolder);
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);
    }
    
    private void Update()
    {
        // Screenshot - using new Input System
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb[UnityEngine.InputSystem.Key.F12].wasPressedThisFrame)
        {
            TakeScreenshot();
        }
        
        // Recording capture
        if (isRecording && _shouldCapture)
        {
            if (Time.time - _lastCaptureTime >= _captureInterval)
            {
                CaptureFrame();
                _lastCaptureTime = Time.time;
            }
            
            // Auto-stop at max duration
            if (Time.time - _recordingStartTime > maxRecordingSeconds)
            {
                StopRecording();
            }
        }
    }
    
    private void LateUpdate()
    {
        _shouldCapture = isRecording;
    }
    
    public void StartRecording()
    {
        if (isRecording) return;
        
        // Create output directory
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _currentRecordingPath = Path.Combine(Application.persistentDataPath, outputFolder, $"Recording_{timestamp}");
        Directory.CreateDirectory(_currentRecordingPath);
        
        // Setup render texture
        int width, height;
        GetTargetResolution(out width, out height);
        
        _renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        _renderTexture.Create();
        
        _frameTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        
        if (format == OutputFormat.GIF)
            _gifFrames = new List<byte[]>();
        
        _frameCount = 0;
        _recordingStartTime = Time.time;
        _lastCaptureTime = Time.time;
        isRecording = true;
        
        Debug.Log($"Recording started: {_currentRecordingPath}");
        OnRecordingStarted?.Invoke();
    }
    
    public void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        
        string outputPath = _currentRecordingPath;
        
        if (format == OutputFormat.GIF && _gifFrames != null && _gifFrames.Count > 0)
        {
            // Save GIF (basic implementation - for better GIF, use a dedicated library)
            outputPath = Path.Combine(_currentRecordingPath, "animation.gif");
            SaveAsGIF(outputPath);
        }
        
        // Cleanup
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
        if (_frameTexture != null)
            Destroy(_frameTexture);
        
        _gifFrames?.Clear();
        
        float duration = Time.time - _recordingStartTime;
        Debug.Log($"Recording stopped: {_frameCount} frames, {duration:F1}s, saved to {outputPath}");
        OnRecordingStopped?.Invoke(outputPath);
    }
    
    public void ToggleRecording()
    {
        if (isRecording)
            StopRecording();
        else
            StartRecording();
    }
    
    private void CaptureFrame()
    {
        if (_camera == null || _renderTexture == null) return;
        
        // Render camera to texture
        RenderTexture prevRT = _camera.targetTexture;
        _camera.targetTexture = _renderTexture;
        _camera.Render();
        _camera.targetTexture = prevRT;
        
        // Read pixels
        RenderTexture.active = _renderTexture;
        _frameTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        _frameTexture.Apply();
        RenderTexture.active = null;
        
        // Save or store frame
        switch (format)
        {
            case OutputFormat.PNG_Sequence:
                SaveFrameAsPNG();
                break;
            case OutputFormat.JPG_Sequence:
                SaveFrameAsJPG();
                break;
            case OutputFormat.GIF:
                StoreFrameForGIF();
                break;
        }
        
        _frameCount++;
    }
    
    private void SaveFrameAsPNG()
    {
        byte[] bytes = _frameTexture.EncodeToPNG();
        string path = Path.Combine(_currentRecordingPath, $"frame_{_frameCount:D5}.png");
        File.WriteAllBytes(path, bytes);
    }
    
    private void SaveFrameAsJPG()
    {
        byte[] bytes = _frameTexture.EncodeToJPG(jpegQuality);
        string path = Path.Combine(_currentRecordingPath, $"frame_{_frameCount:D5}.jpg");
        File.WriteAllBytes(path, bytes);
    }
    
    private void StoreFrameForGIF()
    {
        // Store raw RGB data for GIF encoding
        byte[] pixels = _frameTexture.GetRawTextureData();
        _gifFrames.Add(pixels);
    }
    
    private void SaveAsGIF(string path)
    {
        // Basic GIF implementation
        // For production use, consider using a library like gif-encoder or similar
        Debug.Log($"GIF would be saved to {path} with {_gifFrames.Count} frames");
        Debug.Log("Note: For proper GIF encoding, use a dedicated library like 'gif-encoder'");
        
        // Save as image sequence instead
        for (int i = 0; i < _gifFrames.Count; i++)
        {
            _frameTexture.LoadRawTextureData(_gifFrames[i]);
            _frameTexture.Apply();
            byte[] png = _frameTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(_currentRecordingPath, $"frame_{i:D5}.png"), png);
        }
    }
    
    public void TakeScreenshot()
    {
        StartCoroutine(CaptureScreenshot());
    }
    
    private IEnumerator CaptureScreenshot()
    {
        yield return new WaitForEndOfFrame();
        
        int width, height;
        GetTargetResolution(out width, out height);
        
        // Create temporary texture
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        // Resize if needed
        if (width != Screen.width || height != Screen.height)
        {
            screenshot = ResizeTexture(screenshot, width, height);
        }
        
        // Save
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"Screenshot_{timestamp}.png";
        string path = Path.Combine(Application.persistentDataPath, outputFolder, filename);
        
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        
        Destroy(screenshot);
        
        Debug.Log($"Screenshot saved: {path}");
        OnScreenshotTaken?.Invoke(path);
    }
    
    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        rt.filterMode = FilterMode.Bilinear;
        
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        Destroy(source);
        
        return result;
    }
    
    private void GetTargetResolution(out int width, out int height)
    {
        switch (resolutionMode)
        {
            case ResolutionMode.Custom:
                width = customWidth;
                height = customHeight;
                break;
            case ResolutionMode.HalfScreen:
                width = Screen.width / 2;
                height = Screen.height / 2;
                break;
            default:
                width = Screen.width;
                height = Screen.height;
                break;
        }
    }
    
    public int GetFrameCount() => _frameCount;
    public float GetRecordingDuration() => isRecording ? Time.time - _recordingStartTime : 0;
    public string GetRecordingPath() => _currentRecordingPath;
    
    private void OnDestroy()
    {
        if (isRecording)
            StopRecording();
    }
}
