using ColbyO.Untitled.UI;
using InteractionSystem.Controls;
using InteractionSystem.UI;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace ColbyO.Untitled
{
    public class PolaroidView : View
    {
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _polaroidCamera;

        [SerializeField] private UIIcon _takePhotoHint;
        [SerializeField] private UIIcon _closeHint;
        [SerializeField] private UIIcon _zoomHint;
        [SerializeField] private GameObject _zoomHint2;

        private void Update()
        {
            if (_zoomHint.IsActive())
            {
                _zoomHint.UpdateIconMaterial();
                _zoomHint2.SetActive(InputDeviceHandler.IsCurrentGamepad);
            }

            if (_takePhotoHint.IsActive())
            {
                _takePhotoHint.UpdateIconMaterial();
            }

            if (_closeHint.IsActive())
            {
                _closeHint.UpdateIconMaterial();
            }
        }

        public void SetHints(bool state)
        {
            _takePhotoHint.SetActive(state);
            _closeHint.SetActive(state);
            _zoomHint.SetActive(state);
            if (_zoomHint2.activeSelf) _zoomHint2.SetActive(state);
        }

        public override void Init()
        {

        }

        public override void Show()
        {
            base.Show();
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SetCameraHint(false);
            if (_polaroidCamera && !_polaroidCamera.gameObject.activeSelf) _polaroidCamera.gameObject.SetActive(true);
            if (_playerCamera && _playerCamera.gameObject.activeSelf) _playerCamera.gameObject.SetActive(false);
        }

        public override void Hide()
        {
            base.Hide();
            if (_polaroidCamera && _polaroidCamera.gameObject.activeSelf) _polaroidCamera.gameObject.SetActive(false);
            if (_playerCamera && !_playerCamera.gameObject.activeSelf) _playerCamera.gameObject.SetActive(true);
        }
    }
}
