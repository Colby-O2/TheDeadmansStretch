using ColbyO.Untitled.MonoSystems;
using InteractionSystem.Controls;
using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace ColbyO.Untitled
{
    public class ViewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _target;
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _mesh;

        [Header("Settings")]
        [SerializeField] private LayerMask _ignoreCameraCollisionMask;
        [SerializeField] private MovementSettings _settings;

        [Header("Orbit")]
        [SerializeField] private float _thirdPersonDistance = 10f;
        [SerializeField] private float _firstPersonDistance = 0.1f;
        [SerializeField, ReadOnly] private float _distance = 10f;
        [SerializeField] private float _thirdPersonTargetOffset = 0.25f;
        [SerializeField, ReadOnly] private float _targetOffset = 0.25f;
        [SerializeField] private Vector3 _thirdPersonOffset;
        [SerializeField] private Vector3 _firstPersonOffset = new Vector3(0, -0.6f, 0);
        [SerializeField, ReadOnly] private Vector3 _offset;
        [SerializeField] private Vector2 _zoomLimits = new Vector2(3f, 20f);
        [SerializeField] private float _heightOffset = 1.5f;

        private IInputMonoSystem _inputSystem;

        private Vector2 _cameraAngle;

        private bool _isTransitioning = false;
        
        private bool _isInFirstPerson = false;

        public float Sensitivity => _settings.Sensitivity * (InputDeviceHandler.IsCurrentGamepad ? _settings.ControllerSensitivityScaleFactor : 1f);

        private void Awake()
        {
            _inputSystem = GameManager.GetMonoSystem<IInputMonoSystem>();
            _distance = _thirdPersonDistance;
            _targetOffset = _thirdPersonTargetOffset;
            _offset = _thirdPersonOffset;
        }

        private void Start()
        {
            _inputSystem.OnUseCamera.AddListener(ToggleFirstPerson);
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
            if (UTGameManager.LockMovement || UTGameManager.IsPaused || _isTransitioning) return;

            UpdateLook();
            UpdateZoom();
            UpdateCamera();
        }

        private Vector3 ResolveCameraCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            float viewHeight = 2.0f * Mathf.Tan(Mathf.Deg2Rad * _camera.fieldOfView * 0.5f) * _camera.nearClipPlane;
            float viewWidth = viewHeight / _camera.pixelHeight * _camera.pixelWidth;

            Vector3 camForward = (desiredPos - targetPos).normalized;
            Quaternion rotation = Quaternion.LookRotation(camForward);

            Vector3 camRight = rotation * Vector3.right;
            Vector3 camUp = rotation * Vector3.up;

            Vector3[] offsets =
            {
                -camRight * viewWidth / 2f + camUp * viewHeight / 2f,
                 camRight * viewWidth / 2f + camUp * viewHeight / 2f,
                -camRight * viewWidth / 2f - camUp * viewHeight / 2f,
                 camRight * viewWidth / 2f - camUp * viewHeight / 2f,
            };

            float closestDist = float.MaxValue;
            Vector3 bestPos = desiredPos;

            foreach (var offset in offsets)
            {
                Vector3 dir = (desiredPos + offset) - targetPos;
                Ray ray = new Ray(targetPos, dir.normalized);

                if (Physics.Raycast(ray, out RaycastHit hit, dir.magnitude, ~_ignoreCameraCollisionMask))
                {
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        bestPos = hit.point - offset;
                    }
                }
            }

            return bestPos;
        }

        private void UpdateLook()
        {
            Vector2 look = _inputSystem.RawLook;
            look.y *= -1;

            if (_settings.InvertLookX) look.x *= -1f;
            if (_settings.InvertLookY && !_isInFirstPerson) look.y *= -1f;

            _cameraAngle.x += look.x * Sensitivity;
            _cameraAngle.y += look.y * Sensitivity;

            _cameraAngle.y = Mathf.Clamp(
                _cameraAngle.y,
                _settings.YLookLimit.x,
                _settings.YLookLimit.y
            );
        }

        private void UpdateZoom()
        {
            float scroll = 0;
            _distance -= scroll;
            _distance = Mathf.Clamp(_distance, _zoomLimits.x, _zoomLimits.y);
        }

        private void UpdateCamera()
        {
            Quaternion rotation = Quaternion.Euler(_cameraAngle.y, _cameraAngle.x, 0f);

            Vector3 direction = rotation * Vector3.back;

            Vector3 targetPos = _target.position + _offset + Vector3.up * _heightOffset + transform.right * _targetOffset;
            Vector3 desiredPos = targetPos + direction * _distance;

            Vector3 finalPos = ResolveCameraCollision(targetPos, desiredPos);

            transform.position = finalPos;
            transform.LookAt(targetPos);
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

        private void LookAtStep(float t, float startYaw, float startPitch, float endYaw, float endPitch)
        {
            float alpha = Mathf.SmoothStep(0, 1, t);

            _cameraAngle.x = Mathf.LerpAngle(startYaw, endYaw, alpha);
            _cameraAngle.y = Mathf.Lerp(startPitch, endPitch, alpha);

            _cameraAngle.y = Mathf.Clamp(_cameraAngle.y, _settings.YLookLimit.x, _settings.YLookLimit.y);

        }
        
        private void ToggleFirstPerson()
        {
            _isInFirstPerson = !_isInFirstPerson;
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

    }
}
