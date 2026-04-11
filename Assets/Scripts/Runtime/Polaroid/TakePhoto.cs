using PlazmaGames.Audio;
using PlazmaGames.Core;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioType = PlazmaGames.Audio.AudioType;

namespace ColbyO.Untitled.Polaroid
{
    public class TakePhoto : MonoBehaviour
    {
        [SerializeField] private RenderTexture _cameraTexture;
        [SerializeField] private AudioClip _cameraShotSound;
        private GameObject _shutter;
        private float _shutterTime = 0f;
        private static TakePhoto _instance;
        
        public static Texture2D Capture()
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_instance._cameraShotSound, AudioType.Sfx, false, true);
            _instance._shutterTime = 0;
            RenderTexture rt = _instance._cameraTexture;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            Color[] pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = pixels[i].gamma;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            RenderTexture.active = null;
            return tex;
        }

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            _shutter = GameObject.FindWithTag("Shutter");
            _shutter.SetActive(false);
        }

        private void Update()
        {
            if (!_shutter.activeSelf && _shutterTime < UTGameManager.Preferences.PolaroidCameraShutterTime)
            {
                _shutter.SetActive(true);
            } else if (_shutter.activeSelf && _shutterTime > UTGameManager.Preferences.PolaroidCameraShutterTime)
            {
                _shutter.SetActive(false);
            }
            _shutterTime += Time.deltaTime;
            
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                Texture2D t = TakePhoto.Capture();
                System.IO.File.WriteAllBytes("TEST.png", t.EncodeToPNG());
            }
        }
    }
}
