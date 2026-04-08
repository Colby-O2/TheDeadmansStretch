using PlazmaGames.UI;
using UnityEngine;

namespace ColbyO.Untitled
{
    public class PolaroidView : View
    {
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _polaroidCamera;

        public override void Init()
        {

        }

        public override void Show()
        {
            base.Show();
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
