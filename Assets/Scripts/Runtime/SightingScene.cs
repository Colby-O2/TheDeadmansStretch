using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Math;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioType = PlazmaGames.Audio.AudioType;

namespace ColbyO.Untitled
{
    public class SightingScene : MonoBehaviour
    {
        [SerializeField] private AudioClip _gunshotSound;
        [SerializeField] private BoxCollider _sceneBounds;
        [SerializeField] private Rigidbody _forcePoint;
        [SerializeField] private GameObject _person;
        [SerializeField] private GameObject _personRig;
        [SerializeField] private GameObject _killer;
        [SerializeField] private Camera _polaroidCamera;
        [SerializeField] private Transform _sceneLookAtPoint;
        private CameraZoom _zoom;

        private bool _lookAt = false;

        private bool _found = false;
        private bool _falling = false;
        private float _fallTimer = 0;

        private void Start()
        {
            _zoom = _polaroidCamera.GetComponent<CameraZoom>();
            
            _person.SetActive(false);
            _killer.SetActive(false);
            _personRig.SetActive(false);
        }

        public bool IsCameraLookingAtScene()
        {
            if (!GameManager.GetMonoSystem<IGameLogicMonoSystem>().IsInRange("PhotoArea")) return false;
            if (_zoom.GetZoom() > UTGameManager.Preferences.PolaroidCameraZoomMinToSeeScene) return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_polaroidCamera);
            return GeometryUtility.TestPlanesAABB(planes, _sceneBounds.bounds);
        }

        public void StartScene()
        {
            _person.SetActive(true);
            _killer.SetActive(true);
            _personRig.SetActive(false);
        }

        public void LookAtScene()
        {
            _found = true;
            _lookAt = true;
        }

        public void PushPerson()
        {
            _personRig.SetActive(true);
            _forcePoint.AddForce(Vector3.back * UTGameManager.Preferences.SightingScenePushForce, ForceMode.VelocityChange);
            _falling = true;
            _fallTimer = 0;
            _lookAt = false;
        }

        public void HideKiller()
        {
            _killer.SetActive(false);
        }
        
        public bool IsFalling() => _falling;

        private void Update()
        {
            if (_lookAt)
            {
                DoLookAt(_sceneLookAtPoint.transform.position);
            }
            if (_falling)
            {
                DoLookAt(_forcePoint.transform.position);

                _fallTimer += Time.deltaTime;
                if (_killer.activeSelf && _fallTimer > UTGameManager.Preferences.SightingSceneFallTime / 2.0f)
                {
                    _killer.SetActive(false);
                }
                if (_fallTimer > UTGameManager.Preferences.SightingSceneFallTime)
                {
                    _falling = false;
                }
            }
        }

        private void DoLookAt(Vector3 target)
        {
            Vector3 dir = Vector3.Normalize(target - _polaroidCamera.transform.position);
            Vector2 rot = _polaroidCamera.transform.eulerAngles.XY();
            Vector2 targetRot = Quaternion.LookRotation(dir).eulerAngles.XY();
            rot.x = Mathf.LerpAngle(rot.x, targetRot.x, UTGameManager.Preferences.SightingSceneLookSpeed);
            rot.y = Mathf.LerpAngle(rot.y, targetRot.y, UTGameManager.Preferences.SightingSceneLookSpeed);
            _polaroidCamera.transform.eulerAngles = new Vector3(rot.x, rot.y, _polaroidCamera.transform.eulerAngles.z);
        }

        public void PlayGunshot()
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_gunshotSound, AudioType.Sfx, false, true);
        }
    }
}
