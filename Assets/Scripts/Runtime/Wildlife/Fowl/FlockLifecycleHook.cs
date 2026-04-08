using UnityEngine;

namespace ColbyO.Untitled.Wildlife
{
    public class FlockLifecycleHook : MonoBehaviour
    {
        public System.Action<GameObject> OnFlockDestroyed;

        public void OnCleanup()
        {
            if (gameObject.scene.isLoaded)
            {
                OnFlockDestroyed?.Invoke(gameObject);
            }
        }
    }
}