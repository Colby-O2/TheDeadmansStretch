using InteractionSystem.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyO.Untitled
{
    public class CameraZoom : MonoBehaviour
    {
        private Camera _camera;

        [SerializeField] private InputAction _zoomAction;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            _zoomAction.Enable();
        }

        private void OnDisable()
        {
            _zoomAction.Disable();
        }

        private void Update()
        {
            float zoomInput = _zoomAction.ReadValue<float>();

            float zoomInputScale = InputDeviceHandler.IsCurrentGamepad ? 0.3f : 1.0f;

            zoomInput *= zoomInputScale;

            float fov = _camera.fieldOfView;

            fov = Mathf.Clamp01(fov / UTGameManager.Preferences.PolaroidCameraZoomMin);

            fov *= 1.0f + zoomInput * UTGameManager.Preferences.PolaroidCameraZoomSpeed;

            fov = Mathf.Clamp(
                fov,
                UTGameManager.Preferences.PolaroidCameraZoomMax / UTGameManager.Preferences.PolaroidCameraZoomMin,
                1.0f
            );

            _camera.fieldOfView = fov * UTGameManager.Preferences.PolaroidCameraZoomMin;
        }

        public float GetZoom()
        {
            return _camera.fieldOfView;
        }
    }
}