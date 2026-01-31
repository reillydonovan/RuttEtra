using UnityEngine;
using System.Collections;
using System.Linq;

public class WebcamCapture : MonoBehaviour
{
    [Header("Webcam Settings")]
    public string preferredDeviceName = "";
    public int requestedWidth = 640;
    public int requestedHeight = 480;
    public int requestedFPS = 30;
    
    [Header("Processing")]
    public bool mirrorHorizontal = true;
    public bool mirrorVertical = false;
    
    public WebCamTexture WebcamTexture { get; private set; }
    public RenderTexture ProcessedTexture { get; private set; }
    public bool IsReady => WebcamTexture != null && WebcamTexture.isPlaying;
    
    private Material _processingMaterial;
    
    public event System.Action<Texture> OnFrameReady;
    
    private void Awake()
    {
        // Create shader for mirroring/processing
        Shader procShader = Shader.Find("Hidden/RuttEtra/WebcamProcess");
        if (procShader != null)
        {
            _processingMaterial = new Material(procShader);
        }
    }
    
    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Webcam authorization denied!");
            yield break;
        }
        
        InitializeWebcam();
    }
    
    public void InitializeWebcam()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        
        if (devices.Length == 0)
        {
            Debug.LogError("No webcam devices found!");
            return;
        }
        
        // Find preferred device or use first available
        string deviceName = devices[0].name;
        if (!string.IsNullOrEmpty(preferredDeviceName))
        {
            var preferred = devices.FirstOrDefault(d => d.name.Contains(preferredDeviceName));
            if (!string.IsNullOrEmpty(preferred.name))
                deviceName = preferred.name;
        }
        
        WebcamTexture = new WebCamTexture(deviceName, requestedWidth, requestedHeight, requestedFPS);
        WebcamTexture.Play();
        
        // Create processed render texture
        ProcessedTexture = new RenderTexture(requestedWidth, requestedHeight, 0, RenderTextureFormat.ARGB32);
        ProcessedTexture.filterMode = FilterMode.Bilinear;
        ProcessedTexture.Create();
        
        Debug.Log($"Webcam initialized: {deviceName} @ {WebcamTexture.width}x{WebcamTexture.height}");
    }
    
    private void Update()
    {
        if (!IsReady) return;
        
        // Process and mirror webcam feed
        if (_processingMaterial != null)
        {
            _processingMaterial.SetFloat("_MirrorX", mirrorHorizontal ? 1 : 0);
            _processingMaterial.SetFloat("_MirrorY", mirrorVertical ? 1 : 0);
            Graphics.Blit(WebcamTexture, ProcessedTexture, _processingMaterial);
        }
        else
        {
            Graphics.Blit(WebcamTexture, ProcessedTexture);
        }
        
        OnFrameReady?.Invoke(ProcessedTexture);
    }
    
    public string[] GetAvailableDevices()
    {
        return WebCamTexture.devices.Select(d => d.name).ToArray();
    }
    
    public void SwitchDevice(string deviceName)
    {
        if (WebcamTexture != null && WebcamTexture.isPlaying)
        {
            WebcamTexture.Stop();
        }
        
        preferredDeviceName = deviceName;
        InitializeWebcam();
    }
    
    private void OnDestroy()
    {
        if (WebcamTexture != null)
        {
            WebcamTexture.Stop();
            Destroy(WebcamTexture);
        }
        
        if (ProcessedTexture != null)
        {
            ProcessedTexture.Release();
            Destroy(ProcessedTexture);
        }
        
        if (_processingMaterial != null)
            Destroy(_processingMaterial);
    }
}




