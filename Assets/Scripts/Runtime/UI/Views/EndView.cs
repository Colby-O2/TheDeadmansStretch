using ColbyO.Untitled.MonoSystems;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace ColbyO.Untitled
{
    public class EndView : View
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private AudioSource _musicSource;


        [SerializeField] private GameObject _menuView;
        [SerializeField] private GameObject _menuCamera;
        [SerializeField] private GameObject _playerCamera;

        private Coroutine _musicFadeRoutine;
        [SerializeField] private float _startVol;

        private void Awake()
        {
            _startVol = _musicSource.volume;
        }

        public override void Init()
        {

        }

        public GameObject GetCamera()
        {
            return _menuCamera;
        }

        private IEnumerator FadeInMusic(AudioSource source, float duration)
        {
            if (!_musicSource.isPlaying) _musicSource.Play();

            float startVolume = _startVol;

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, startVolume * GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume(), time / duration);
                yield return null;
            }

            source.volume = startVolume * GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume();
        }

        public override void Show()
        {
            base.Show();
            _menuCamera.SetActive(true);
            _playerCamera.SetActive(false);
            _menuView.SetActive(true);

            if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = StartCoroutine(FadeInMusic(_musicSource, 3f));

            InputSystem.onAnyButtonPress.Call(ctrl => Quit());

            GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = true;
            GameManager.GetMonoSystem<ICinematicMonoSystem>().Disabe();
        }

        private void Quit()
        {
            Debug.Log("Quitting");
            Application.Quit();
        }
    }
}
