using ColbyO.Untitled.MonoSystems;
using ColbyO.Untitled.Traffic;
using ColbyO.VNTG.PSX;
using InteractionSystem;
using PlazmaGames.Core;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace ColbyO.Untitled
{
    public class VisualEffectMonoSystem : MonoBehaviour, IVisualEffectMonoSystem
    {
        private Volume _volume;

        private ScreenFade _screenFade;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _volume = FindAnyObjectByType<Volume>();
            _screenFade = FindAnyObjectByType<ScreenFade>();
        }

        private void OnSceneUnload(Scene scene)
        {

        }

        public Promise FadeIn(float duration)
        {
            return _screenFade.FadeIn(duration);
        }

        public Promise FadeOut(float duration)
        {
            return _screenFade.FadeOut(duration);
        }

        public PSXEffectSettings GetPSXSettings()
        {
            if (_volume == null) _volume = FindAnyObjectByType<Volume>();

            if (_volume && _volume.profile && _volume.profile.TryGet(out PSXEffectSettings psx)) return psx;

            return null;
        }
    }
}