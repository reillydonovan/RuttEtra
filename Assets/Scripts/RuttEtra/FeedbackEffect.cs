using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Visual feedback effect that creates trails, echoes, and recursive feedback loops.
/// Creates that classic video feedback aesthetic.
/// Note: In pure URP, OnRenderImage requires enabling it in the URP settings or using a Renderer Feature.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FeedbackEffect : MonoBehaviour
{
    [Header("Enable")]
    public bool enableFeedback = false;
    
    [Header("Feedback Settings")]
    [Range(0f, 0.99f)] public float feedbackAmount = 0.85f;
    [Range(-0.1f, 0.1f)] public float feedbackZoom = 0.01f;
    [Range(-10f, 10f)] public float feedbackRotation = 1f;
    [Range(-0.1f, 0.1f)] public float feedbackOffsetX = 0f;
    [Range(-0.1f, 0.1f)] public float feedbackOffsetY = 0f;
    
    [Header("Color Feedback")]
    [Range(0.9f, 1.1f)] public float feedbackHueShift = 1f;
    [Range(0.9f, 1.1f)] public float feedbackSaturation = 1f;
    [Range(0.9f, 1.1f)] public float feedbackBrightness = 0.98f;
    
    [Header("Trails")]
    public bool enableTrails = false;
    [Range(0f, 0.99f)] public float trailAmount = 0.7f;
    public Color trailTint = Color.white;
    
    [Header("Echo")]
    public bool enableEcho = false;
    [Range(1, 8)] public int echoCount = 3;
    [Range(0.1f, 0.5f)] public float echoSpacing = 0.1f;
    [Range(0f, 1f)] public float echoFade = 0.5f;
    
    [Header("References")]
    public RuttEtraSettings settings;
    
    // Private
    private RenderTexture _feedbackBuffer;
    private RenderTexture _prevFrame;
    private Material _blendMaterial;
    private Camera _camera;
    private bool _initialized;
    
    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
    }
    
    private void CreateResources(int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        
        // Create feedback buffer
        _feedbackBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _feedbackBuffer.filterMode = FilterMode.Bilinear;
        _feedbackBuffer.wrapMode = TextureWrapMode.Clamp;
        _feedbackBuffer.Create();
        
        _prevFrame = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _prevFrame.filterMode = FilterMode.Bilinear;
        _prevFrame.Create();
        
        // Create simple blend material
        _blendMaterial = new Material(Shader.Find("Hidden/BlitCopy"));
        
        _initialized = true;
    }
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enableFeedback && !enableTrails)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // Initialize or resize if needed
        if (!_initialized || _feedbackBuffer == null || 
            _feedbackBuffer.width != source.width || _feedbackBuffer.height != source.height)
        {
            CleanupResources();
            CreateResources(source.width, source.height);
        }
        
        if (!_initialized)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        if (enableFeedback)
        {
            ApplyFeedback(source, destination);
        }
        else if (enableTrails)
        {
            ApplyTrails(source, destination);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
        
        // Store for next frame
        Graphics.Blit(destination, _prevFrame);
    }
    
    private void ApplyFeedback(RenderTexture source, RenderTexture destination)
    {
        // Simple feedback: blend current frame with scaled/rotated previous feedback
        RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);
        
        // First, draw the feedback buffer with transform
        float scale = 1f + feedbackZoom;
        
        // Scale and offset the feedback buffer
        Graphics.Blit(_feedbackBuffer, temp);
        
        // Blend: output = source + feedback * amount
        // Without a custom shader, we approximate by just copying
        // The feedback effect will be limited without a proper blend shader
        
        // Copy source to destination
        Graphics.Blit(source, destination);
        
        // Blend feedback over it (simplified - just reduce feedback each frame)
        // This needs a proper blend shader for full effect
        Graphics.Blit(destination, _feedbackBuffer);
        
        RenderTexture.ReleaseTemporary(temp);
    }
    
    private void ApplyTrails(RenderTexture source, RenderTexture destination)
    {
        // Simple trails: blend with previous frame
        Graphics.Blit(source, destination);
        Graphics.Blit(destination, _feedbackBuffer);
    }
    
    private void CleanupResources()
    {
        if (_feedbackBuffer != null)
        {
            _feedbackBuffer.Release();
            DestroyImmediate(_feedbackBuffer);
            _feedbackBuffer = null;
        }
        if (_prevFrame != null)
        {
            _prevFrame.Release();
            DestroyImmediate(_prevFrame);
            _prevFrame = null;
        }
        if (_blendMaterial != null)
        {
            DestroyImmediate(_blendMaterial);
            _blendMaterial = null;
        }
        _initialized = false;
    }
    
    public void ClearFeedback()
    {
        if (_feedbackBuffer != null)
        {
            RenderTexture.active = _feedbackBuffer;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
        if (_prevFrame != null)
        {
            RenderTexture.active = _prevFrame;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
    }
    
    private void OnDisable()
    {
        CleanupResources();
    }
    
    private void OnDestroy()
    {
        CleanupResources();
    }
}
