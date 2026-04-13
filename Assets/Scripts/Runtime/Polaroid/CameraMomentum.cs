using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace ColbyO.Untitled.Polaroid
{
    public class CameraMomentum : MonoBehaviour
    {
        [SerializeField] private RectTransform _viewFinderImage;
        [SerializeField] private RectTransform _viewFinderCrosshair;
        
        private Vector2 _imagePosition = Vector2.zero;
        private Vector2 _crosshairPosition = Vector2.zero;
        
        private Vector3 _prevRotation = Vector3.zero;

        private void Update()
        {
            if (_viewFinderImage == null || _viewFinderCrosshair == null) return;

            float delta = Vector3.Angle(_prevRotation, transform.forward);
            Vector3 r = Vector3.ProjectOnPlane(transform.InverseTransformDirection(_prevRotation), Vector3.forward);
            Vector2 dir = new Vector2(r.x, r.y).normalized;
            Vector2 imageOffset = dir * (delta * UTGameManager.Preferences.PolaroidCameraShakeImageMag);
            _imagePosition += imageOffset;
            _imagePosition = Vector2.Lerp(_imagePosition, Vector2.zero, Time.deltaTime * UTGameManager.Preferences.PolaroidCameraShakeRestorationRate);
            _viewFinderImage.anchoredPosition = _imagePosition;
            Vector2 crosshairOffset = dir * (delta * UTGameManager.Preferences.PolaroidCameraShakeCrosshairMag);
            _crosshairPosition += crosshairOffset;
            _crosshairPosition = Vector2.Lerp(_crosshairPosition, Vector2.zero, Time.deltaTime * UTGameManager.Preferences.PolaroidCameraShakeRestorationRate);
            _viewFinderCrosshair.anchoredPosition = _crosshairPosition;
            _prevRotation = transform.forward;
        }
    }
}
