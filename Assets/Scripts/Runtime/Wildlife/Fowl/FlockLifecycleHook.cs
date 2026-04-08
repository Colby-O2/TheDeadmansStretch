using UnityEngine;

namespace ColbyO.Untitled.Wildlife
{
    public class FlockLifecycleHook : MonoBehaviour
    {
        public System.Action<GameObject> OnFlockDestroyed;

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
            {
                OnFlockDestroyed?.Invoke(gameObject);
            }
        }
    }
}
