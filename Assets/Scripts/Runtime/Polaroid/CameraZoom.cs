using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyO.Untitled
{
    public class CameraZoom : MonoBehaviour
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }
        
        private void Update()
        {
            float fov = _camera.fieldOfView;
            fov = Mathf.Clamp01((fov) / UTGameManager.Preferences.PolaroidCameraZoomMin);
            fov *= 1.0f + Mouse.current.scroll.y.value * UTGameManager.Preferences.PolaroidCameraZoomSpeed;
            fov = Mathf.Clamp(fov, UTGameManager.Preferences.PolaroidCameraZoomMax / UTGameManager.Preferences.PolaroidCameraZoomMin, 1.0f);
            _camera.fieldOfView = fov * UTGameManager.Preferences.PolaroidCameraZoomMin;
        }
    }
}
