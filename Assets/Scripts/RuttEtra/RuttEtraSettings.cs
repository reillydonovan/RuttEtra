using UnityEngine;

[CreateAssetMenu(fileName = "RuttEtraSettings", menuName = "RuttEtra/Settings")]
public class RuttEtraSettings : ScriptableObject
{
    [Header("Mesh Resolution")]
    [Range(16, 512)] public int horizontalResolution = 128;
    [Range(8, 256)] public int verticalResolution = 64;
    
    [Header("Displacement")]
    [Range(0f, 5f)] public float displacementStrength = 1f;
    [Range(0f, 1f)] public float displacementSmoothing = 0.5f;
    public bool invertDisplacement = false;
    
    [Header("Visual Style")]
    [Range(0.001f, 0.05f)] public float lineWidth = 0.01f;
    public Color primaryColor = Color.green;
    public Color secondaryColor = Color.cyan;
    [Range(0f, 1f)] public float colorBlend = 0.5f;
    public bool useSourceColor = false;
    
    [Header("Scan Line Effect")]
    [Range(1, 8)] public int scanLineSkip = 1;
    public bool showHorizontalLines = true;
    public bool showVerticalLines = false;
    
    [Header("Post Effects")]
    [Range(0f, 2f)] public float glowIntensity = 0.5f;
    [Range(0f, 1f)] public float noiseAmount = 0f;
    [Range(0f, 1f)] public float persistence = 0f;
}




