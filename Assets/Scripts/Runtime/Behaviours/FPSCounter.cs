using UnityEngine;

namespace ColbyO.Untitled
{
    public class FPSCounter : MonoBehaviour
    {
        private float _deltaTime = 0.0f;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(10, 10, w, h * 0.02f);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 24;
            style.normal.textColor = Color.white;

            float fps = 1.0f / _deltaTime;
            string text = $"FPS: {Mathf.Ceil(fps)}";

            GUI.Label(rect, text, style);
        }
    }
}