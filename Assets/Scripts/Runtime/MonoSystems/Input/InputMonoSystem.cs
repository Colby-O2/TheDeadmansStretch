using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ColbyO.Untitled.MonoSystems
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputMonoSystem : MonoBehaviour, IInputMonoSystem
    {
        [SerializeField] private PlayerInput _input;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _sprintAction;
        private InputAction _useCameraAction;

        public Vector2 RawMovement { get; private set; }
        public Vector2 RawLook { get; private set; }

        public UnityEvent OnShift { get; private set; }
        public UnityEvent OnUseCamera { get; private set; }

        private void Awake()
        {
            if (!_input) _input = GetComponent<PlayerInput>();

            OnShift = new UnityEvent();
            OnUseCamera = new UnityEvent();

            _moveAction = _input.actions["Move"];
            _lookAction = _input.actions["Look"];
            _sprintAction = _input.actions["Sprint"];
            _useCameraAction = _input.actions["Camera"];
        }

        private void OnEnable()
        {
            _moveAction.performed   += HandleMoveAction;
            _lookAction.performed   += HandleLookAction;
            _sprintAction.performed += HandleSprintAction;
            _sprintAction.canceled  += HandleSprintAction;
            _useCameraAction.performed += HandleUseCameraAction;
        }

        private void OnDisable()
        {
            _moveAction.performed   -= HandleMoveAction;
            _lookAction.performed   -= HandleLookAction;
            _sprintAction.performed -= HandleSprintAction;
            _sprintAction.canceled  -= HandleSprintAction;
            _useCameraAction.performed -= HandleUseCameraAction;
        }

        private void HandleMoveAction(InputAction.CallbackContext e)
        {
            RawMovement = e.ReadValue<Vector2>();
        }

        private void HandleLookAction(InputAction.CallbackContext e)
        {
            RawLook = e.ReadValue<Vector2>();
        }

        private void HandleSprintAction(InputAction.CallbackContext e)
        {
            OnShift?.Invoke();
        }
        
        private void HandleUseCameraAction(InputAction.CallbackContext obj)
        {
            OnUseCamera.Invoke();
        }


        public void EnableMovement()
        {
            _moveAction.Enable();
            _lookAction.Enable();
        }

        public void DisableMovement()
        {
            RawMovement = Vector2.zero;
            RawLook = Vector2.zero;

            _moveAction.Disable();
            _lookAction.Disable();
        }
    }
}