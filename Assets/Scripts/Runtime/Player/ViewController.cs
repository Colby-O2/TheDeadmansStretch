using ColbyO.Untitled.MonoSystems;
using InteractionSystem.Controls;
using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.UIElements;
using static UnityEngine.Audio.GeneratorInstance;

namespace ColbyO.Untitled.Player
{
    public enum PlayerViewType
    {
        FirstPerson,
        ThirdPerson,
        Fixed
    }

    public class ViewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _thirdPersonTarget;
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _mesh;

        private Transform _target;

        [Header("Settings")]
        [SerializeField] private LayerMask _ignoreCameraCollisionMask;
        [SerializeField] private MovementSettings _settings;

        [Header("Orbit/Distances")]
        [SerializeField] private float _thirdPersonDistance = 5f;
        [SerializeField] private float _firstPersonDistance = 0.05f;
        [SerializeField] private Vector2 _zoomLimits = new Vector2(1f, 10f);

        [Header("Current State")]
        [SerializeField, ReadOnly] private float _distance = 5f;
        [SerializeField, ReadOnly] private float _targetOffset = 0.25f;
        [SerializeField, ReadOnly] private PlayerViewType _currentView = PlayerViewType.ThirdPerson;

        [Header("Offsets")]
        [SerializeField] private float _thirdPersonTargetOffset = 0.25f;
        [SerializeField] private Vector3 _thirdPersonOffset;
        [SerializeField] private Vector3 _firstPersonOffset = new Vector3(0, 0.1f, 0);
        [SerializeField] private float _heightOffset = 1.5f;
        [SerializeField] private Vector3 _offset;

        [Header("Limits")]
        [SerializeField] private Vector2 _xLookLimits = new Vector2(-360f, 360f);

        [Header("Auto Look Settings")]
        [SerializeField] private float _autoLookSmoothTime = 0.1f;
        private Vector2 _autoLookVelocity;
        private Transform _autoLookTarget = null;
        private bool _isAutoLooking => _autoLookTarget != null;

        private IInputMonoSystem _inputSystem;
        private Vector2 _cameraAngle;
        private Vector2 _relativeCameraAngle;
        private bool _isTransitioning = false;
        private bool _isInFirstPerson = false;

        public float Sensitivity => _settings.Sensitivity * (InputDeviceHandler.IsCurrentGamepad ? _settings.ControllerSensitivityScaleFactor : 1f);
        public bool IsFrozen { get; set; }

        private void Awake()
        {
            _inputSystem = GameManager.GetMonoSystem<IInputMonoSystem>();
            ToggleView(PlayerViewType.ThirdPerson);
        }

        private void OnEnable()
        {
            UTGameManager.PlayerViewController = this;
        }

        private void Start()
        {
            // TODO: In Awake someone is fighting with me on weather the camera is on or not...
            _camera.gameObject.SetActive(false);
            _inputSystem.OnUseCamera.AddListener(ToggleFirstPerson);
        }

        public void ToggleFirstPerson(bool state)
        {
            if (state == _isInFirstPerson) return;
            if (UTGameManager.LockMovement || UTGameManager.IsPaused || IsFrozen || _isTransitioning) return;
            if (UTGameManager.PlayerAnimationController.GetAnimator().GetBool("InDriverSeat")) return;
            _isInFirstPerson = state;
            if (_isInFirstPerson)
            {
                GameManager.GetMonoSystem<IUIMonoSystem>().Show<PolaroidView>();
                _mesh.SetActive(false);
                _distance = _firstPersonDistance;
                _targetOffset = 0.0f;
                _offset = _firstPersonOffset;
            }
            else
            {
                _mesh.SetActive(true);
                GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
                _distance = _thirdPersonDistance;
                _targetOffset = _thirdPersonTargetOffset;
                _offset = _thirdPersonOffset;
            }
        }
        public void ToggleFirstPerson()
        {
            ToggleFirstPerson(!_isInFirstPerson);
        }

        private void Update()
        {
            if (_isTransitioning)
            {
                UpdateCamera();
            }
        }

        private void LateUpdate()
        {
            if (UTGameManager.LockMovement || UTGameManager.IsPaused || IsFrozen || _isTransitioning) return;

            UpdateLook();
            UpdateZoom();
            UpdateCamera();
        }

        public void SetAutoLookTarget(Transform target)
        {
            _autoLookTarget = target;
        }

        private Vector3 ResolveCameraCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            float clipBuffer = _camera.nearClipPlane * 1.1f;

            float h = Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * clipBuffer;
            float w = h * _camera.aspect;

            Quaternion rotation = Quaternion.LookRotation(desiredPos - targetPos);
            Vector3 camRight = rotation * Vector3.right;
            Vector3 camUp = rotation * Vector3.up;

            Vector3[] corners = {
                (camUp * h) - (camRight * w),
                (camUp * h) + (camRight * w),
                -(camUp * h) - (camRight * w),
                -(camUp * h) + (camRight * w)
            };

            float minDistance = _distance;

            foreach (Vector3 cornerOffset in corners)
            {
                Vector3 cornerTarget = desiredPos + cornerOffset;
                Vector3 dir = cornerTarget - targetPos;

                if (Physics.Raycast(targetPos, dir.normalized, out RaycastHit hit, dir.magnitude, ~_ignoreCameraCollisionMask, QueryTriggerInteraction.Ignore))
                {
                    float hitDistance = hit.distance - clipBuffer;
                    if (hitDistance < minDistance)
                    {
                        minDistance = hitDistance;
                    }
                }
            }

            minDistance = Mathf.Max(minDistance, 0.2f);

            Vector3 pdir = (desiredPos - targetPos);
            if (pdir.sqrMagnitude < 0.01) pdir = Vector3.forward;
            return targetPos + pdir.normalized * minDistance;
        }

        private void UpdateLook()
        {
            if (_isAutoLooking)
            {
                Vector3 targetPos = _autoLookTarget.position;
                Vector3 cameraPos = _target.position + _offset + (Vector3.up * _heightOffset);
                Vector3 dir = (targetPos - cameraPos).normalized;
                
                Quaternion lookRot = Quaternion.LookRotation(dir);
                Vector3 euler = lookRot.eulerAngles;

                _cameraAngle.x = Mathf.SmoothDampAngle(_cameraAngle.x, euler.y, ref _autoLookVelocity.x, _autoLookSmoothTime);
                _cameraAngle.y = Mathf.SmoothDampAngle(_cameraAngle.y, euler.x, ref _autoLookVelocity.y, _autoLookSmoothTime);

                _cameraAngle.y = ClampAngleRelative(_cameraAngle.y, 0, _settings.YLookLimit.x, _settings.YLookLimit.y);

                return;
            }

            Vector2 look = _inputSystem.RawLook;
            look.y *= -1;

            if (_settings.InvertLookX) look.x *= -1f;
            if (_settings.InvertLookY && _currentView != PlayerViewType.FirstPerson) look.y *= -1f;

            float deltaX = look.x * Sensitivity;
            float deltaY = look.y * Sensitivity;

            if (_currentView == PlayerViewType.Fixed || _currentView == PlayerViewType.FirstPerson)
            {
                _relativeCameraAngle.x += deltaX;
                _relativeCameraAngle.y += deltaY;

                _relativeCameraAngle.x = Mathf.Clamp(_relativeCameraAngle.x, _xLookLimits.x, _xLookLimits.y);
                _relativeCameraAngle.y = Mathf.Clamp(_relativeCameraAngle.y, _settings.YLookLimit.x, _settings.YLookLimit.y);

                _cameraAngle.x = _target.eulerAngles.y + _relativeCameraAngle.x;
                _cameraAngle.y = _target.eulerAngles.x + _relativeCameraAngle.y;
            }
            else
            {
                _cameraAngle.x += deltaX;
                _cameraAngle.y += deltaY;
                _cameraAngle.y = Mathf.Clamp(_cameraAngle.y, _settings.YLookLimit.x, _settings.YLookLimit.y);

                if (_cameraAngle.x > 360f) _cameraAngle.x -= 360f;
                if (_cameraAngle.x < -360f) _cameraAngle.x += 360f;
            }
        }

        private void UpdateZoom()
        {
            if (_currentView != PlayerViewType.ThirdPerson) return;

            //float scroll = 0;
            //_distance -= scroll;
            //_distance = Mathf.Clamp(_distance, _zoomLimits.x, _zoomLimits.y);
        }

        private void UpdateCamera()
        {
            if (float.IsNaN(_cameraAngle.x))
            {
                Debug.Log(_cameraAngle); _cameraAngle.x = 0;}
            if (float.IsNaN(_cameraAngle.y)) {Debug.Log(_cameraAngle); _cameraAngle.y = 0;}
            Quaternion rotation = Quaternion.Euler(_cameraAngle.y, _cameraAngle.x, 0f);
            Vector3 direction = rotation * Vector3.back;

            Vector3 targetPos = _target.position + _offset + (Vector3.up * _heightOffset);

            if (_currentView == PlayerViewType.ThirdPerson)
            {
                targetPos += transform.right * _targetOffset;
            }

            Vector3 desiredPos = targetPos + direction * _distance;

            Vector3 finalPos = (_currentView == PlayerViewType.ThirdPerson)
                ? ResolveCameraCollision(targetPos, desiredPos)
                : desiredPos;

            transform.position = finalPos;
            transform.rotation = rotation;
        }

        private float ClampAngleRelative(float angle, float baseAngle, float min, float max)
        {
            float delta = Mathf.DeltaAngle(baseAngle, angle);
            delta = Mathf.Clamp(delta, min, max);
            return baseAngle + delta;
        }

        public void LookAt(Transform target, float duration)
        {
            if (GameManager.GetMonoSystem<IAnimationMonoSystem>().HasAnimationRunning(this))
            {
                GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            }

            _isTransitioning = true;

            Vector3 directionToTarget = (_target.position - target.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(-directionToTarget);
            Vector3 targetEuler = targetRot.eulerAngles;

            float startYaw = _cameraAngle.x;
            float startPitch = _cameraAngle.y;
            float endYaw = Mathf.LerpAngle(startYaw, targetEuler.y, 1f);
            float endPitch = Mathf.LerpAngle(startPitch, targetEuler.x, 1f);

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation
            (
                this,
                duration,
                (float t) => LookAtStep(t, startYaw, startPitch, endYaw, endPitch)
            )
            .Then(_ =>
            {
                _isTransitioning = false;
            });
        }

        public Promise TransitionView(PlayerViewType targetType, float duration, Vector3? offsetOverride = null, Transform fixedTarget = null)
        {
            _isTransitioning = true;

            Transform startTarget = _target;
            float startDist = _distance;
            Vector3 startOffset = _offset;
            float startTargetOffset = _targetOffset;

            Transform endTarget = (targetType == PlayerViewType.Fixed) ? fixedTarget : _thirdPersonTarget;
            float endDist = (targetType == PlayerViewType.ThirdPerson) ? _thirdPersonDistance : _firstPersonDistance;
            Vector3 endOffset = offsetOverride ?? ((targetType == PlayerViewType.ThirdPerson) ? _thirdPersonOffset : Vector3.zero);
            float endTargetOffset = (targetType == PlayerViewType.ThirdPerson) ? _thirdPersonTargetOffset : 0f;

            return GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                duration,
                (float t) =>
                {
                    float alpha = Mathf.SmoothStep(0, 1, t);

                    _distance = Mathf.Lerp(startDist, endDist, alpha);
                    _offset = Vector3.Lerp(startOffset, endOffset, alpha);
                    _targetOffset = Mathf.Lerp(startTargetOffset, endTargetOffset, alpha);

                    Vector3 posA = startTarget.position + startOffset + (Vector3.up * _heightOffset);

                    Vector3 posB = endTarget.position + _offset + (Vector3.up * _heightOffset);
                    if (targetType == PlayerViewType.ThirdPerson)
                        posB += transform.right * _targetOffset;

                    Vector3 blendedTargetPos = Vector3.Lerp(posA, posB, alpha);

                    Quaternion rotation = Quaternion.Euler(_cameraAngle.y, _cameraAngle.x, 0f);
                    Vector3 direction = rotation * Vector3.back;

                    transform.SetPositionAndRotation(blendedTargetPos + (direction * _distance), rotation);

                    if (t >= 1f) _target = endTarget;
                }
            ).Then(_ =>
            {
                _currentView = targetType;
                _isTransitioning = false;
                ToggleView(targetType, offsetOverride, null, fixedTarget);
            });
        }

        public void LookAtPosition(Vector3 position, float duration)
        {
            GameObject temp = new GameObject("TempLookTarget");
            temp.transform.position = position;
            LookAt(temp.transform, duration);
            Destroy(temp, duration);
        }

        private void LookAtStep(float t, float startYaw, float startPitch, float endYaw, float endPitch)
        {
            float alpha = Mathf.SmoothStep(0, 1, t);

            _cameraAngle.x = Mathf.LerpAngle(startYaw, endYaw, alpha);
            _cameraAngle.y = Mathf.Lerp(startPitch, endPitch, alpha);

            _cameraAngle.y = Mathf.Clamp(_cameraAngle.y, _settings.YLookLimit.x, _settings.YLookLimit.y);

        }

        public void ToggleView(PlayerViewType type, Vector3? offsetOverride = null, Vector2? xLookLimitsOverride = null, Transform fixedTarget = null)
        {
            _currentView = type;
            _relativeCameraAngle = Vector2.zero;

            switch (type)
            {
                case PlayerViewType.FirstPerson:
                    _mesh.SetActive(false);
                    _target = _thirdPersonTarget;
                    _distance = _firstPersonDistance;
                    _targetOffset = 0.0f;
                    _offset = offsetOverride ?? _firstPersonOffset;
                    _xLookLimits = xLookLimitsOverride ?? new Vector2(-90f, 90f);
                    _cameraAngle.x = _target.eulerAngles.y;
                    break;

                case PlayerViewType.Fixed:
                    _mesh.SetActive(true);
                    _target = fixedTarget ?? _thirdPersonTarget;
                    _distance = _firstPersonDistance;
                    _targetOffset = 0.0f;
                    _offset = offsetOverride ?? Vector3.zero;
                    _xLookLimits = xLookLimitsOverride ?? new Vector2(-60f, 60f);
                    _cameraAngle.x = _target.eulerAngles.y;
                    _cameraAngle.y = _target.eulerAngles.x;
                    break;

                case PlayerViewType.ThirdPerson:
                    _mesh.SetActive(true);
                    _target = _thirdPersonTarget;
                    _distance = _thirdPersonDistance;
                    _targetOffset = _thirdPersonTargetOffset;
                    _offset = offsetOverride ?? _thirdPersonOffset;
                    _xLookLimits = new Vector2(-360f, 360f);
                    break;
            }
        }
    }
}