using PlazmaGames.Core;
using UnityEngine;

namespace ColbyO.Untitled.Polaroid
{
    public class CameraMomentum : MonoBehaviour
    {
        private RectTransform _viewFinderImage;
        private RectTransform _viewFinderCrosshair;
        
        private Vector2 _imagePosition = Vector2.zero;
        private Vector2 _crosshairPosition = Vector2.zero;
        
        private Vector3 _prevRotation = Vector3.zero;
        
        void Awake()
        {
            _viewFinderImage = GameObject.FindWithTag("ViewFinderImage").GetComponent<RectTransform>();
            _viewFinderCrosshair = GameObject.FindWithTag("ViewFinderCrosshair").GetComponent<RectTransform>();
        }

        void Update()
        {
            float delta = Vector3.Angle(_prevRotation, transform.forward);
            Vector3 r = Vector3.ProjectOnPlane(transform.InverseTransformDirection(_prevRotation), Vector3.forward);
            Vector2 dir = new Vector2(r.x, r.y).normalized;
            Vector2 imageOffset = dir * (delta * UTGameManager.Preferences.PolaroidCameraShakeImageMag);
            _imagePosition += imageOffset * Time.deltaTime;
            _imagePosition = Vector2.Lerp(_imagePosition, Vector2.zero, Time.deltaTime * UTGameManager.Preferences.PolaroidCameraShakeRestorationRate);
            _viewFinderImage.anchoredPosition = _imagePosition;
            Vector2 crosshairOffset = dir * (delta * UTGameManager.Preferences.PolaroidCameraShakeCrosshairMag);
            _crosshairPosition += crosshairOffset * Time.deltaTime;
            _crosshairPosition = Vector2.Lerp(_crosshairPosition, Vector2.zero, Time.deltaTime * UTGameManager.Preferences.PolaroidCameraShakeRestorationRate);
            _viewFinderCrosshair.anchoredPosition = _crosshairPosition;
            _prevRotation = transform.forward;
        }
    }
}
