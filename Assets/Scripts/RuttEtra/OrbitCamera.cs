using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = Vector3.zero;
    
    [Header("Orbit Settings")]
    public float rotationSpeed = 5f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minDistance = 1f;
    public float maxDistance = 20f;
    
    [Header("Smoothing")]
    public float smoothTime = 0.1f;
    
    private float _currentDistance = 5f;
    private float _targetDistance = 5f;
    private float _horizontalAngle = 0f;
    private float _verticalAngle = 20f;
    private float _targetHorizontalAngle = 0f;
    private float _targetVerticalAngle = 20f;
    
    private float _distanceVelocity;
    private float _horizontalVelocity;
    private float _verticalVelocity;
    
    private Vector2 _lastMousePosition;
    private bool _isDragging;

    private void Start()
    {
        // Initialize from current position if no target set
        if (target == null)
        {
            // Try to find the RuttEtra mesh as default target
            var mesh = FindFirstObjectByType<RuttEtraMeshGenerator>();
            if (mesh != null)
                target = mesh.transform;
        }
        
        // Calculate initial angles from current camera position
        if (target != null)
        {
            Vector3 offset = transform.position - GetTargetPosition();
            _currentDistance = offset.magnitude;
            _targetDistance = _currentDistance;
            
            _horizontalAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            _verticalAngle = Mathf.Asin(offset.y / _currentDistance) * Mathf.Rad2Deg;
            
            _targetHorizontalAngle = _horizontalAngle;
            _targetVerticalAngle = _verticalAngle;
        }
    }

    private void LateUpdate()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        
        Vector2 mousePos = mouse.position.ReadValue();
        
        // Check if mouse is over UI
        bool overUI = UnityEngine.EventSystems.EventSystem.current != null &&
                      UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        
        // Only process input when NOT over UI
        if (!overUI)
        {
            // Right mouse button to orbit
            if (mouse.rightButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastMousePosition = mousePos;
            }
            
            // Handle rotation
            if (_isDragging && mouse.rightButton.isPressed)
            {
                Vector2 delta = mousePos - _lastMousePosition;
                _targetHorizontalAngle += delta.x * rotationSpeed * 0.1f;
                _targetVerticalAngle -= delta.y * rotationSpeed * 0.1f;
                _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, minVerticalAngle, maxVerticalAngle);
                _lastMousePosition = mousePos;
            }
            
            // Handle zoom with scroll wheel
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetDistance -= scroll * zoomSpeed * 0.1f;
                _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
            }
        }
        
        // Stop dragging when button released (check this regardless of UI)
        if (mouse.rightButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }
        
        // Always smooth and update camera position
        _horizontalAngle = Mathf.SmoothDamp(_horizontalAngle, _targetHorizontalAngle, ref _horizontalVelocity, smoothTime);
        _verticalAngle = Mathf.SmoothDamp(_verticalAngle, _targetVerticalAngle, ref _verticalVelocity, smoothTime);
        _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _distanceVelocity, smoothTime);
        
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (target == null) return;
        
        float horizontalRad = _horizontalAngle * Mathf.Deg2Rad;
        float verticalRad = _verticalAngle * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad),
            Mathf.Sin(verticalRad),
            Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad)
        ) * _currentDistance;
        
        Vector3 targetPos = GetTargetPosition();
        transform.position = targetPos + offset;
        transform.LookAt(targetPos);
    }

    private Vector3 GetTargetPosition()
    {
        return target != null ? target.position + targetOffset : targetOffset;
    }

    /// <summary>
    /// Reset camera to default view
    /// </summary>
    public void ResetView()
    {
        _targetHorizontalAngle = 0f;
        _targetVerticalAngle = 20f;
        _targetDistance = 5f;
    }
}
