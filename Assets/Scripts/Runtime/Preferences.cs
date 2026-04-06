using UnityEngine;


namespace ColbyO.Untitled
{
    [CreateAssetMenu(fileName = "DefaultPreferences", menuName = "Preferences")]
    public class Preferences : ScriptableObject
    {
        public Texture2D Cursor;
        public float DialogueSpeedMul = 1f;
        public float PolaroidCameraShakeImageMag = 1f;
        public float PolaroidCameraShakeCrosshairMag = 1f;
        public float PolaroidCameraShakeRestorationRate = 5f;
    }
}
