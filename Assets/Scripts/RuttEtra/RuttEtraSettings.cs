using UnityEngine;

[CreateAssetMenu(fileName = "RuttEtraSettings", menuName = "RuttEtra/Settings")]
public class RuttEtraSettings : ScriptableObject
{
    [Header("Resolution")]
    [Range(16, 512)] public int horizontalResolution = 128;
    [Range(8, 256)] public int verticalResolution = 64;
    
    [Header("Input Signal")]
    [Range(-1f, 1f)] public float brightness = 0f;
    [Range(0.1f, 3f)] public float contrast = 1f;
    [Range(0f, 1f)] public float threshold = 0f;
    [Range(0.1f, 3f)] public float gamma = 1f;
    public bool edgeDetect = false;
    [Range(1, 8)] public int posterize = 1;
    
    [Header("Z-Axis Displacement")]
    [Range(0f, 5f)] public float displacementStrength = 1f;
    [Range(0f, 1f)] public float displacementSmoothing = 0.5f;
    [Range(-1f, 1f)] public float displacementOffset = 0f;
    public bool invertDisplacement = false;
    [Range(0f, 1f)] public float zModulation = 0f;
    [Range(0f, 5f)] public float zModFrequency = 1f;
    
    [Header("Raster Position")]
    [Range(-10f, 10f)] public float horizontalPosition = 0f;
    [Range(-10f, 10f)] public float verticalPosition = 0f;
    
    [Header("Raster Scale")]
    [Range(0.1f, 3f)] public float horizontalScale = 1f;
    [Range(0.1f, 3f)] public float verticalScale = 1f;
    [Range(0.1f, 3f)] public float meshScale = 1f;
    
    [Header("Raster Rotation")]
    [Range(-180f, 180f)] public float rotationX = 0f;
    [Range(-180f, 180f)] public float rotationY = 0f;
    [Range(-180f, 180f)] public float rotationZ = 0f;
    
    [Header("Raster Distortion")]
    [Range(-1f, 1f)] public float keystoneH = 0f;
    [Range(-1f, 1f)] public float keystoneV = 0f;
    [Range(-0.5f, 0.5f)] public float barrelDistortion = 0f;
    
    [Header("Scan Lines")]
    [Range(1, 8)] public int scanLineSkip = 1;
    public bool showHorizontalLines = true;
    public bool showVerticalLines = false;
    public bool interlace = false;
    
    [Header("Deflection Wave")]
    [Range(0f, 2f)] public float horizontalWave = 0f;
    [Range(0f, 2f)] public float verticalWave = 0f;
    [Range(0f, 10f)] public float waveFrequency = 2f;
    [Range(0f, 5f)] public float waveSpeed = 1f;
    
    [Header("Line Style")]
    [Range(0.001f, 0.05f)] public float lineWidth = 0.01f;
    [Range(0f, 1f)] public float lineTaper = 0f;
    [Range(0f, 2f)] public float glowIntensity = 0.5f;
    
    [Header("Colors")]
    public Color primaryColor = Color.green;
    public Color secondaryColor = Color.cyan;
    [Range(0f, 1f)] public float colorBlend = 0.5f;
    public bool useSourceColor = false;
    public Color backgroundColor = Color.black;
    
    [Header("Feedback")]
    [Range(0f, 0.98f)] public float feedback = 0f;
    [Range(-0.1f, 0.1f)] public float feedbackZoom = 0f;
    [Range(-10f, 10f)] public float feedbackRotation = 0f;
    
    [Header("Post Effects")]
    [Range(0f, 1f)] public float noiseAmount = 0f;
    [Range(0f, 1f)] public float persistence = 0f;
    [Range(0f, 1f)] public float scanlineFlicker = 0f;
    [Range(0f, 1f)] public float bloom = 0f;
}
