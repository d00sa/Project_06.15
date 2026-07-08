using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CameraAspect : MonoBehaviour
{
    [SerializeField] private float _baseSize = 8f;
    private Camera _camera;

    private int _lastWidth;
    private int _lastHeight;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdateCameraSize();
    }

    #if UNITY_EDITOR
    private void Update()
    {
        if (_lastWidth != Screen.width || _lastHeight != Screen.height) {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            UpdateCameraSize();
        }
    }
    #endif

    private void UpdateCameraSize()
    {
        float targetAspect = 16f / 9f;
        float currentAspect = (float)Screen.width / Screen.height;

        _camera.orthographicSize = _baseSize;

        if (currentAspect < targetAspect) {
            _camera.orthographicSize *= targetAspect / currentAspect;
        }
    }
}
