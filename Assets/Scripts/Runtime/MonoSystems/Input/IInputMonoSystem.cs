using PlazmaGames.Core.MonoSystem;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyO.Untitled.MonoSystems
{
    public interface IInputMonoSystem : IMonoSystem
    {
        public Vector2 RawMovement { get; }
        public Vector2 RawLook { get; }
        public UnityEvent OnShift { get; }
        public UnityEvent OnUseCamera { get; }

        public void EnableMovement();
        public void DisableMovement();
    }
}