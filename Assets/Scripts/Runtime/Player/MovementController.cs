using ColbyO.Untitled.MonoSystems;
using InteractionSystem.Interfaces;
using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.Math;
using UnityEngine;

namespace ColbyO.Untitled.Player
{
    public enum MovementState
    {
        Grounded,
        Airborne
    }

    public class MovementController : MonoBehaviour, IPlayerController
    {
        private readonly float GRAVITY = -9.8f;
        private readonly float GROUNDED_VEL = -2f;

        [SerializeField] private MovementSettings _settings;

        [Header("Debug")]
        [SerializeField, ReadOnly] private MovementState _state;
        [SerializeField, ReadOnly] private float _timeOffGround;
        [SerializeField, ReadOnly] private Vector3 _velocity;
        [SerializeField, ReadOnly] private Vector2 _horizontalVelocity;
        [SerializeField, ReadOnly] private Vector2 _movement;
        [SerializeField, ReadOnly] private bool _isSprinting;
        [SerializeField, ReadOnly] private bool _isFrozen = false;
        [SerializeField, ReadOnly] private bool _isJustMovementFrozen = false;

        private CharacterController _controller;
        [SerializeField] private ViewController _viewController;
        [SerializeField] private AnimationController _animationController;
        private IInputMonoSystem _input;

        private float Speed => _settings.Speed;
        public float GravityScale = 1.0f;
        private float Gravity => GRAVITY * _settings.GravityMul * GravityScale;

        public Vector3 Velocity => _controller.velocity;

        public MovementSettings Settings => _settings;
        public bool IsSprinting => _isSprinting;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();
            _input.OnShift.AddListener(ToggleSprint);
        }

        private void OnEnable()
        {
            UTGameManager.PlayerMoveController = this;
            UTGameManager.PlayerWalkingAudio = GetComponent<WalkingSound>();
        }

        private void Update()
        {
            if (UTGameManager.LockMovement || UTGameManager.IsPaused || _isFrozen || _isJustMovementFrozen)
            {
                if (!_isFrozen) _animationController.SetWalking(false);
                return;
            }
            _movement = Vector2.ClampMagnitude(_input.RawMovement, 1f);
            _animationController.SetWalking(_movement.magnitude > 0.01f);
            ApplyGravity();
            UpdateMovement();
            RotateTowardsMovement();
            if (_controller.enabled) MoveController();
        }

        private void ToggleSprint()
        {
            _isSprinting = !_isSprinting;
            _animationController.SetSprinting(_isSprinting);
        }

        public void ApplyGravity()
        {
            _velocity.y = _state == MovementState.Airborne
                ? Mathf.MoveTowards(_velocity.y, -_settings.TerminalVelocity, -Gravity * Time.deltaTime)
                : GROUNDED_VEL * GravityScale;
        }

        private void RotateTowardsMovement()
        {
            Vector3 local = new Vector3(_movement.x, 0f, _movement.y);

            Vector3 worldDir = _viewController.transform.TransformDirection(local);
            worldDir.y = 0f;

            if (worldDir.sqrMagnitude < 0.001f) return;

            Quaternion targetRot = Quaternion.LookRotation(worldDir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                10f * Time.deltaTime
            );
        }

        private void UpdateHorizontalVelocity(float controlMultiplier)
        {
            Vector3 local = new Vector3(_movement.x, 0f, _movement.y);
            float multiplier = _isSprinting ? _settings.SprintSpeedMul : 1.0f;
            if (_movement.y < 0f) multiplier *= _settings.BackwardSpeedMul;
            if (_movement.x != 0f) multiplier *= _settings.StrafingSpeedMul;
            Vector3 moveDir = _viewController.transform.TransformDirection(local).SetY(0).normalized;
            Vector3 target3D = moveDir * Speed * multiplier;
            Vector2 target2D = new Vector2(target3D.x, target3D.z) * controlMultiplier;

            _horizontalVelocity = Vector2.Lerp(
                _horizontalVelocity,
                target2D,
                _settings.InputSmoothing * Time.deltaTime
            );

            _velocity.x = _horizontalVelocity.x;
            _velocity.z = _horizontalVelocity.y;
        }

        private void UpdateGrounded()
        {
            UpdateHorizontalVelocity(1f);
            if (!_controller.isGrounded) _state = MovementState.Airborne;
        }

        private void UpdateAirborne()
        {
            UpdateHorizontalVelocity(_settings.AirControl);
            if (_controller.isGrounded)
            {
                _state = MovementState.Grounded;
                _velocity.y = GROUNDED_VEL * GravityScale;
            }
        }

        public void MoveController() => _controller.Move(_velocity * Time.deltaTime);

        private void UpdateMovement()
        {
            switch (_state)
            {
                case MovementState.Grounded: UpdateGrounded(); break;
                case MovementState.Airborne: UpdateAirborne(); break;
            }
        }

        public void Move(Vector3 d)
        {
            bool prev = _controller.enabled;
            _controller.enabled = false;
            transform.position += d;
            _controller.enabled = prev;
        }

        public void TeleportTo(Vector3? loc = null, Quaternion? rot = null)
        {
            bool prev = _controller.enabled;
            _controller.enabled = false;
            transform.SetPositionAndRotation(loc ?? transform.position, rot ?? transform.rotation);
            _controller.enabled = prev;
        }

        public Promise TransitionTo(Transform target, float duration)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            bool wasFrozen = _isFrozen;
            bool prevChacaterControllerState = _controller.enabled;
            if (!wasFrozen) Freeze();
            if (prevChacaterControllerState) DisableChacaterController();

            return GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                duration,
                (float t) =>
                {
                    transform.SetPositionAndRotation(
                        Vector3.Lerp(startPos, target.position, t), 
                        Quaternion.Slerp(startRot, target.rotation, t)
                    );
                }
            )
            .Then(_ => 
            {
                if (!wasFrozen) Unfreeze();
                if (prevChacaterControllerState) EnableChacaterController();
            });
        }

        public void FreezeJustMovement()
        {
            _isJustMovementFrozen = true;
            _input.DisableMovement(justMovement: true);
            _velocity = Vector2.zero;
            _horizontalVelocity = Vector2.zero;
        }

        public void UnfreezeJustMovement()
        {
            _isJustMovementFrozen = false;
            _input.EnableMovement(justMovement: true);
        }


        public void Freeze()
        {
            _isFrozen = true;
            UTGameManager.PlayerViewController.IsFrozen = true;
            if (_isJustMovementFrozen) _input.DisableMovement(justView: true);
            _velocity = Vector2.zero;
            _horizontalVelocity = Vector2.zero;
        }

        public void Unfreeze()
        {
            _isFrozen = false;
            UTGameManager.PlayerViewController.IsFrozen = false;
            
            if (_isJustMovementFrozen) _input.EnableMovement(justView: true);
            else _input.EnableMovement();
        }

        public void Attach(Transform parnet)
        {
            transform.SetParent(parnet);
        }

        public void Deattach()
        {
            transform.SetParent(null);
        }

        public void DisableChacaterController()
        {
            _controller.enabled = false;
        }

        public void EnableChacaterController()
        {
            _controller.enabled = true;
        }

        public void SetHorizontalVelocity(Vector3 velocity)
        {
            _horizontalVelocity = velocity;
        }
    }
}
