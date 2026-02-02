using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Bridge between webcam capture and VFX Graph for Rutt/Etra effect.
/// Attach this to a GameObject with a VisualEffect component.
/// 
/// VFX Graph Setup Instructions:
/// 1. Create a new VFX Graph (Assets > Create > Visual Effects > Visual Effect Graph)
/// 2. In the VFX Graph, add these Exposed Properties:
///    - WebcamTexture (Texture2D)
///    - Resolution (Vector2) 
///    - DisplacementStrength (float, default 2)
///    - MeshWidth (float, default 16)
///    - MeshHeight (float, default 9)
///    - PrimaryColor (Color)
///    - SecondaryColor (Color)
///    - LineWidth (float, default 0.02)
///    - Brightness (float, default 0)
///    - Contrast (float, default 1)
/// 
/// 3. Set up the VFX Graph nodes:
///    INITIALIZE PARTICLE:
///    - Set Capacity to match Resolution.x * Resolution.y (e.g., 128*64 = 8192)
///    - Spawn: Set spawn mode to "Single Burst" with count = capacity
///    - Set Lifetime to infinity or very high value
///    
///    - Calculate grid position:
///      * particleId = Get Attribute: particleId
///      * x = particleId % Resolution.x
///      * y = floor(particleId / Resolution.x)
///      * normalizedX = x / (Resolution.x - 1)
///      * normalizedY = y / (Resolution.y - 1)
///      * posX = (normalizedX - 0.5) * MeshWidth
///      * posY = (normalizedY - 0.5) * MeshHeight
///      
///    - Sample texture for Z displacement:
///      * Sample Texture2D: WebcamTexture at UV(normalizedX, normalizedY)
///      * luminance = dot(rgb, float3(0.299, 0.587, 0.114))
///      * luminance = (luminance + Brightness) * Contrast
///      * posZ = luminance * DisplacementStrength
///      
///    - Set Position to (posX, posY, posZ)
///    - Set Color based on luminance lerp(PrimaryColor, SecondaryColor, luminance)
///    
///    UPDATE PARTICLE:
///    - Re-sample texture each frame for live updates
///    - Recalculate position.z based on new luminance
///    
///    OUTPUT:
///    - Use "Output Particle Line" or "Output Particle Point"
///    - For lines: connect adjacent particles (complex, may need strips)
///    - For points: use Output Particle Point with size = LineWidth
/// </summary>
[RequireComponent(typeof(VisualEffect))]
public class RuttEtraVFX : MonoBehaviour
{
    [Header("References")]
    public RuttEtraSettings settings;
    public WebcamCapture webcamCapture;
    
    [Header("VFX Property Names")]
    public string webcamTextureProperty = "WebcamTexture";
    public string resolutionProperty = "Resolution";
    public string displacementProperty = "DisplacementStrength";
    public string meshWidthProperty = "MeshWidth";
    public string meshHeightProperty = "MeshHeight";
    public string primaryColorProperty = "PrimaryColor";
    public string secondaryColorProperty = "SecondaryColor";
    public string lineWidthProperty = "LineWidth";
    public string brightnessProperty = "Brightness";
    public string contrastProperty = "Contrast";
    public string rotationProperty = "Rotation";
    public string timeProperty = "GameTime";
    
    private VisualEffect _vfx;
    private RenderTexture _webcamRT;
    
    private void Awake()
    {
        _vfx = GetComponent<VisualEffect>();
    }
    
    private void Start()
    {
        // Auto-find references
        if (settings == null)
        {
            var controller = FindFirstObjectByType<RuttEtraController>();
            if (controller) settings = controller.settings;
        }
        
        if (webcamCapture == null)
            webcamCapture = FindFirstObjectByType<WebcamCapture>();
        
        if (webcamCapture != null)
            webcamCapture.OnFrameReady += OnWebcamFrame;
        
        // Initial setup
        UpdateVFXProperties();
    }
    
    private void Update()
    {
        UpdateVFXProperties();
        
        // Send time for animations
        if (_vfx.HasFloat(timeProperty))
            _vfx.SetFloat(timeProperty, Time.time);
    }
    
    private void OnWebcamFrame(Texture sourceTexture)
    {
        if (_vfx == null) return;
        
        // Create or resize render texture to match settings resolution
        int width = settings != null ? settings.horizontalResolution : 128;
        int height = settings != null ? settings.verticalResolution : 64;
        
        if (_webcamRT == null || _webcamRT.width != width || _webcamRT.height != height)
        {
            if (_webcamRT != null)
                _webcamRT.Release();
            
            _webcamRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            _webcamRT.filterMode = FilterMode.Bilinear;
            _webcamRT.Create();
        }
        
        // Blit webcam to our sized RT
        Graphics.Blit(sourceTexture, _webcamRT);
        
        // Send to VFX
        if (_vfx.HasTexture(webcamTextureProperty))
            _vfx.SetTexture(webcamTextureProperty, _webcamRT);
    }
    
    private void UpdateVFXProperties()
    {
        if (_vfx == null || settings == null) return;
        
        // Resolution
        if (_vfx.HasVector2(resolutionProperty))
            _vfx.SetVector2(resolutionProperty, new Vector2(settings.horizontalResolution, settings.verticalResolution));
        
        // Displacement
        if (_vfx.HasFloat(displacementProperty))
            _vfx.SetFloat(displacementProperty, settings.displacementStrength);
        
        // Mesh size
        if (_vfx.HasFloat(meshWidthProperty))
            _vfx.SetFloat(meshWidthProperty, 16f * settings.horizontalScale * settings.meshScale);
        if (_vfx.HasFloat(meshHeightProperty))
            _vfx.SetFloat(meshHeightProperty, 9f * settings.verticalScale * settings.meshScale);
        
        // Colors
        if (_vfx.HasVector4(primaryColorProperty))
            _vfx.SetVector4(primaryColorProperty, settings.primaryColor);
        if (_vfx.HasVector4(secondaryColorProperty))
            _vfx.SetVector4(secondaryColorProperty, settings.secondaryColor);
        
        // Line style
        if (_vfx.HasFloat(lineWidthProperty))
            _vfx.SetFloat(lineWidthProperty, settings.lineWidth);
        
        // Input processing
        if (_vfx.HasFloat(brightnessProperty))
            _vfx.SetFloat(brightnessProperty, settings.brightness);
        if (_vfx.HasFloat(contrastProperty))
            _vfx.SetFloat(contrastProperty, settings.contrast);
        
        // Rotation
        if (_vfx.HasVector3(rotationProperty))
            _vfx.SetVector3(rotationProperty, new Vector3(settings.rotationX, settings.rotationY, settings.rotationZ));
    }
    
    private void OnDestroy()
    {
        if (webcamCapture != null)
            webcamCapture.OnFrameReady -= OnWebcamFrame;
        
        if (_webcamRT != null)
        {
            _webcamRT.Release();
            Destroy(_webcamRT);
        }
    }
    
    /// <summary>
    /// Call this to reinitialize the VFX (e.g., after resolution change)
    /// </summary>
    public void Reinitialize()
    {
        if (_vfx != null)
        {
            _vfx.Reinit();
        }
    }
}
