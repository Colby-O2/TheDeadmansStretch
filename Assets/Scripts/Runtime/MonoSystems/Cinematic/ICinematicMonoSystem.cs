using PlazmaGames.Core;
using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace ColbyO.Untitled.MonoSystems
{
    public interface ICinematicMonoSystem : IMonoSystem
    {
        public void Enable(bool lockMovement = true);
        public void Disabe(bool setGlobalLock = true);
        public GameObject GetCamera();
        public void MoveTo(string tag, string lookAtTag = "");
        public void MoveTo(Transform loc, Transform lookAt = null, Transform parnet = null);
        public Promise HandleCameraTransition(string fromTag, string toTag, string lookAtTag, float duration);
        public Promise HandleCameraTransition(Transform from, Transform to, Transform lookAt, float duration, Transform parnet = null);
        public Promise HandleCameraLookAt(string lookAtTag, float duration);
        public Promise HandleCameraLookAt(Transform lookAtLoc, float duration);
    }
}