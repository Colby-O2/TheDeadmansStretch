using ColbyO.Untitled.Player;
using PlazmaGames.Animation;
using PlazmaGames.Core;
using PlazmaGames.Math;
using Unity.VisualScripting;
using UnityEngine;

namespace ColbyO.Untitled
{
    public class SerialKillerController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private VelocityTracker _vel;
        [SerializeField] private WalkingSound _audio;
        [SerializeField] private Transform _headLoc;

        private Transform _lookTarget;
        private bool _isAlwaysLooking => _lookTarget != null;

        private void Update()
        {
            _animator.SetBool("IsWalking", _vel.Velocity.SetY(0.0f).magnitude > 0.01f);

            if (_isAlwaysLooking && _lookTarget != null)
            {
                Vector3 targetDirection = (_lookTarget.position - transform.position).SetY(0).normalized;
                if (targetDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(targetDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
                }
            }
        }

        public void SetAlwaysLookAt(Transform target)
        {
            _lookTarget = target;
        }

        public Transform GetHeadLoc() => _headLoc;

        public Animator GetAnimator() => _animator;

        public void Attach(Transform parnet) => transform.SetParent(parnet);
        public void Deattach() => transform.SetParent(null);

        public void ToggleAudio(bool state)
        {
            _audio.Enabled = state;
        }

        public Promise GetOutOfCar(Vector3 endPos, Quaternion endRot, float duration)
        {
            _animator.SetBool("InDriverSeat", false);
            transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
            return GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                duration,
                (float t) =>
                {
                    float alpha = Mathf.SmoothStep(0, 1, t);
                    transform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, alpha), Quaternion.Slerp(startRot, endRot, alpha));
                }
            );
        }

        public Promise GetOutOfCar(Transform target, float duration)
        {
            return GetOutOfCar(transform.position, transform.rotation, duration);
        }

        public Promise Shoot()
        {
            _animator.SetBool("Shooting", true);
            return UTGameManager.GlobalScheduler.Wait(1f);
        }

        public Promise<int> Goto(Transform target, float durationMove, float durationRot)
        {
            transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
            return GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                durationMove,
                (float t) =>
                {
                    float alpha = Mathf.SmoothStep(0, 1, t);
                    transform.SetPositionAndRotation(Vector3.Lerp(startPos, target.position, alpha), Quaternion.Slerp(startRot, target.rotation, alpha));
                }
            );
            //.Then(_ => GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
            //    this,
            //    durationRot,
            //    (float t) =>
            //    {
            //        float alpha = Mathf.SmoothStep(0, 1, t);
            //        transform.rotation = Quaternion.Slerp(startRot, target.rotation, alpha);
            //    }
            //));
        }

        public Promise FaceTarget(Vector3 targetPos, float duration)
        {
            targetPos.y = transform.position.y;

            Vector3 direction = (targetPos - transform.position).normalized;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation(direction);

            return GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                duration,
                (float t) =>
                {
                    float alpha = Mathf.SmoothStep(0, 1, t);
                    transform.rotation = Quaternion.Slerp(startRot, endRot, alpha);
                }
            );
        }

        public void TeleportTo(Vector3? loc = null, Quaternion? rot = null)
        {
            transform.SetPositionAndRotation(loc ?? transform.position, rot ?? transform.rotation);
        }
    }
}
