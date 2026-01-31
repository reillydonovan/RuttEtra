using UnityEngine;
using UnityEngine.InputSystem;

public class RuttEtraController : MonoBehaviour
{
    [Header("Core Components")]
    public RuttEtraSettings settings;
    public WebcamCapture webcamCapture;
    public RuttEtraMeshGenerator meshGenerator;
    
    [Header("Camera Setup")]
    public Camera mainCamera;
    public float cameraDistance = 10f;
    public float orbitSpeed = 30f;
    
    [Header("Input")]
    public bool enableOrbit = true;
    public bool enableZoom = true;
    
    private Vector3 _cameraTarget;
    private float _currentOrbitAngle = 0f;
    private float _currentTilt = 15f;
    private float _currentZoom;
    
    private void Start()
    {
        _currentZoom = cameraDistance;
        _cameraTarget = meshGenerator != null ? meshGenerator.transform.position : Vector3.zero;
        
        ValidateSetup();
    }
    
    private void ValidateSetup()
    {
        if (settings == null)
            Debug.LogError("RuttEtraSettings not assigned to controller!");
        if (webcamCapture == null)
            Debug.LogError("WebcamCapture not assigned to controller!");
        if (meshGenerator == null)
            Debug.LogError("RuttEtraMeshGenerator not assigned to controller!");
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    private void Update()
    {
        HandleInput();
        UpdateCamera();
    }
    
    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        
        if (keyboard == null) return;
        
        // Orbit with arrow keys or WASD
        if (enableOrbit)
        {
            float hInput = 0f;
            float vInput = 0f;
            
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) hInput = -1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) hInput = 1f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) vInput = 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) vInput = -1f;
            
            _currentOrbitAngle += hInput * orbitSpeed * Time.deltaTime;
            _currentTilt = Mathf.Clamp(_currentTilt - vInput * orbitSpeed * Time.deltaTime, -80f, 80f);
        }
        
        // Zoom with scroll wheel
        if (enableZoom && mouse != null)
        {
            float scroll = mouse.scroll.y.ReadValue() / 120f; // Normalize scroll value
            _currentZoom = Mathf.Clamp(_currentZoom - scroll * 0.5f, 2f, 50f);
        }
        
        // Quick controls
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            settings.invertDisplacement = !settings.invertDisplacement;
        }
        
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            settings.useSourceColor = !settings.useSourceColor;
        }
        
        if (keyboard.vKey.wasPressedThisFrame)
        {
            settings.showVerticalLines = !settings.showVerticalLines;
            meshGenerator?.RefreshMesh();
        }
        
        if (keyboard.rKey.wasPressedThisFrame)
        {
            ResetCamera();
        }
    }
    
    private void UpdateCamera()
    {
        if (mainCamera == null) return;
        
        float radAngle = _currentOrbitAngle * Mathf.Deg2Rad;
        float radTilt = _currentTilt * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(radAngle) * Mathf.Cos(radTilt),
            Mathf.Sin(radTilt),
            -Mathf.Cos(radAngle) * Mathf.Cos(radTilt)
        ) * _currentZoom;
        
        mainCamera.transform.position = _cameraTarget + offset;
        mainCamera.transform.LookAt(_cameraTarget);
    }
    
    public void ResetCamera()
    {
        _currentOrbitAngle = 0f;
        _currentTilt = 15f;
        _currentZoom = cameraDistance;
    }
    
    // Public API for UI controls
    public void SetDisplacementStrength(float value)
    {
        if (settings != null) settings.displacementStrength = value;
    }
    
    public void SetLineWidth(float value)
    {
        if (settings != null) settings.lineWidth = value;
    }
    
    public void SetResolution(int horizontal, int vertical)
    {
        if (settings != null)
        {
            settings.horizontalResolution = horizontal;
            settings.verticalResolution = vertical;
            meshGenerator?.RefreshMesh();
        }
    }
    
    public void SetPrimaryColor(Color color)
    {
        if (settings != null) settings.primaryColor = color;
    }
    
    public void SetScanLineSkip(int skip)
    {
        if (settings != null)
        {
            settings.scanLineSkip = skip;
            meshGenerator?.RefreshMesh();
        }
    }
}

