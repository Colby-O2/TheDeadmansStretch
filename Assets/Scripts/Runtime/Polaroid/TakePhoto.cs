using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyO.Untitled.Polaroid
{
    public class TakePhoto : MonoBehaviour
    {
        [SerializeField] private RenderTexture _cameraTexture;
        private static TakePhoto _instance;
        
        public static Texture2D Capture()
        {
            RenderTexture rt = _instance._cameraTexture;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
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

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Texture2D t = TakePhoto.Capture();
                System.IO.File.WriteAllBytes("TEST.png", t.EncodeToPNG());
            }
        }
    }
}
