using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

        private bool _movementDisabled;
        private bool _viewDisabled;

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
            _moveAction.canceled += HandleMoveAction;

            _lookAction.performed   += HandleLookAction;
            _lookAction.canceled += HandleLookAction;

            _sprintAction.performed += HandleSprintAction;
            _sprintAction.canceled  += HandleSprintAction;
            _useCameraAction.performed += HandleUseCameraAction;
        }

        private void OnDisable()
        {
            _moveAction.performed   -= HandleMoveAction;
            _moveAction.canceled -= HandleMoveAction;

            _lookAction.performed   -= HandleLookAction;
            _lookAction.canceled -= HandleLookAction;

            _sprintAction.performed -= HandleSprintAction;
            _sprintAction.canceled  -= HandleSprintAction;
            _useCameraAction.performed -= HandleUseCameraAction;
        }

        private void HandleMoveAction(InputAction.CallbackContext e)
        {
            if (_movementDisabled) return;
            RawMovement = e.ReadValue<Vector2>();
        }

        private void HandleLookAction(InputAction.CallbackContext e)
        {
            if (_viewDisabled) return;
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


        public void EnableMovement(bool justMovement = false, bool justView = false)
        {
            if (justMovement)
            {
                _moveAction.Enable();
                _movementDisabled = false;
            }
            else if (justView)
            {
                _lookAction.Enable();
                _viewDisabled = false;
            }
            else
            {
                _moveAction.Enable();
                _lookAction.Enable();

                _movementDisabled = false;
                _viewDisabled = false;
            }
        }

        public void DisableMovement(bool justMovement = false, bool justView = false)
        {
            if (justMovement)
            {

                RawMovement = Vector2.zero;
                _moveAction.Disable();
                _movementDisabled = true;
            }
            else if (justView)
            {
                RawLook = Vector2.zero;
                _lookAction.Disable();
                _viewDisabled = true;
            }
            else
            {
                RawMovement = Vector2.zero;
                RawLook = Vector2.zero;

                _moveAction.Disable();
                _lookAction.Disable();

                _movementDisabled = true;
                _viewDisabled = true;
            }
        }
    }
}