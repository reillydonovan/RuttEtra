using UnityEngine;

/// <summary>
/// Mirror and kaleidoscope effects for the Rutt/Etra mesh.
/// Creates symmetrical patterns through vertex manipulation.
/// </summary>
public class MirrorKaleidoscope : MonoBehaviour
{
    [Header("Mirror Settings")]
    public bool enableMirror = false;
    public MirrorMode mirrorMode = MirrorMode.None;
    
    [Header("Kaleidoscope")]
    public bool enableKaleidoscope = false;
    [Range(2, 12)] public int kaleidoscopeSegments = 6;
    [Range(0f, 360f)] public float kaleidoscopeRotation = 0f;
    public bool animateRotation = false;
    [Range(0f, 60f)] public float rotationSpeed = 10f;
    
    [Header("Radial Symmetry")]
    public bool enableRadialSymmetry = false;
    [Range(2, 8)] public int radialCopies = 4;
    [Range(0f, 1f)] public float radialOffset = 0f;
    
    [Header("Wave Mirror")]
    public bool enableWaveMirror = false;
    [Range(0f, 2f)] public float waveMirrorAmount = 0.5f;
    [Range(0.5f, 5f)] public float waveMirrorFrequency = 2f;
    
    public enum MirrorMode
    {
        None,
        Horizontal,
        Vertical,
        Both,
        Diagonal,
        DiagonalReverse
    }
    
    private void Update()
    {
        if (animateRotation && enableKaleidoscope)
        {
            kaleidoscopeRotation = (kaleidoscopeRotation + rotationSpeed * Time.deltaTime) % 360f;
        }
    }
    
    /// <summary>
    /// Apply mirror/kaleidoscope effects to UV coordinates before sampling.
    /// Call this in the mesh generator when calculating UVs.
    /// </summary>
    public Vector2 TransformUV(Vector2 uv)
    {
        if (!enableMirror && !enableKaleidoscope && !enableWaveMirror)
            return uv;
        
        Vector2 result = uv;
        
        // Apply mirror first
        if (enableMirror)
        {
            result = ApplyMirror(result);
        }
        
        // Apply kaleidoscope
        if (enableKaleidoscope)
        {
            result = ApplyKaleidoscope(result);
        }
        
        // Apply wave mirror
        if (enableWaveMirror)
        {
            result = ApplyWaveMirror(result);
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply mirror/kaleidoscope effects to vertex positions.
    /// Creates duplicate vertices for symmetry effects.
    /// </summary>
    public Vector3 TransformVertex(Vector3 vertex, Vector3 center, int index, int totalVertices)
    {
        if (!enableRadialSymmetry)
            return vertex;
        
        // Radial symmetry creates rotated copies
        Vector3 toVertex = vertex - center;
        float baseAngle = Mathf.Atan2(toVertex.x, toVertex.z) * Mathf.Rad2Deg;
        float dist = new Vector2(toVertex.x, toVertex.z).magnitude;
        
        // Determine which segment this vertex belongs to
        int segment = index % radialCopies;
        float segmentAngle = 360f / radialCopies;
        float newAngle = (baseAngle + segment * segmentAngle + radialOffset * 360f) * Mathf.Deg2Rad;
        
        return new Vector3(
            center.x + Mathf.Sin(newAngle) * dist,
            vertex.y,
            center.z + Mathf.Cos(newAngle) * dist
        );
    }
    
    private Vector2 ApplyMirror(Vector2 uv)
    {
        switch (mirrorMode)
        {
            case MirrorMode.Horizontal:
                if (uv.x > 0.5f)
                    uv.x = 1f - uv.x;
                break;
                
            case MirrorMode.Vertical:
                if (uv.y > 0.5f)
                    uv.y = 1f - uv.y;
                break;
                
            case MirrorMode.Both:
                if (uv.x > 0.5f)
                    uv.x = 1f - uv.x;
                if (uv.y > 0.5f)
                    uv.y = 1f - uv.y;
                break;
                
            case MirrorMode.Diagonal:
                if (uv.x + uv.y > 1f)
                {
                    float temp = uv.x;
                    uv.x = 1f - uv.y;
                    uv.y = 1f - temp;
                }
                break;
                
            case MirrorMode.DiagonalReverse:
                if (uv.x > uv.y)
                {
                    float temp = uv.x;
                    uv.x = uv.y;
                    uv.y = temp;
                }
                break;
        }
        
        return uv;
    }
    
    private Vector2 ApplyKaleidoscope(Vector2 uv)
    {
        // Convert to polar coordinates centered at 0.5, 0.5
        Vector2 centered = uv - new Vector2(0.5f, 0.5f);
        float angle = Mathf.Atan2(centered.y, centered.x) * Mathf.Rad2Deg + kaleidoscopeRotation;
        float dist = centered.magnitude;
        
        // Wrap angle into segment
        float segmentAngle = 360f / kaleidoscopeSegments;
        angle = Mathf.Abs(angle % segmentAngle);
        
        // Mirror within segment for symmetry
        if (angle > segmentAngle / 2f)
            angle = segmentAngle - angle;
        
        // Convert back to UV
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(
            0.5f + Mathf.Cos(rad) * dist,
            0.5f + Mathf.Sin(rad) * dist
        );
    }
    
    private Vector2 ApplyWaveMirror(Vector2 uv)
    {
        float wave = Mathf.Sin(uv.y * waveMirrorFrequency * Mathf.PI * 2f + Time.time * 2f);
        float threshold = 0.5f + wave * waveMirrorAmount * 0.5f;
        
        if (uv.x > threshold)
            uv.x = threshold - (uv.x - threshold);
        
        return uv;
    }
    
    /// <summary>
    /// Get a description of the current effect mode
    /// </summary>
    public string GetEffectDescription()
    {
        if (!enableMirror && !enableKaleidoscope && !enableRadialSymmetry && !enableWaveMirror)
            return "None";
        
        string desc = "";
        
        if (enableMirror && mirrorMode != MirrorMode.None)
            desc += mirrorMode.ToString() + " Mirror";
        
        if (enableKaleidoscope)
        {
            if (desc.Length > 0) desc += " + ";
            desc += kaleidoscopeSegments + "-way Kaleidoscope";
        }
        
        if (enableRadialSymmetry)
        {
            if (desc.Length > 0) desc += " + ";
            desc += radialCopies + "x Radial";
        }
        
        if (enableWaveMirror)
        {
            if (desc.Length > 0) desc += " + ";
            desc += "Wave Mirror";
        }
        
        return desc;
    }
}
