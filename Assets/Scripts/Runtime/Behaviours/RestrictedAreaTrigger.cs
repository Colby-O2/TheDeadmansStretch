using System.Collections;

using UnityEngine;

using ColbyO.Untitled.Player;
using DialogueGraph.Data;
using PlazmaGames.Core;
using ColbyO.Untitled.MonoSystems;

namespace ColbyO.Untitled
{
    public class RestrictedAreaTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _pushBackDistance = 3.0f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Dialogue")]
        [SerializeField] private bool _onlyPlaceOnce;
        [SerializeField] private string _dialogue;

        private bool _isProcessing = false;
        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_isProcessing) return;

            if (other.TryGetComponent(out MovementController player))
            {
                if (!string.IsNullOrEmpty(_dialogue) && (!_hasTriggered || !_onlyPlaceOnce))
                {
                    GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise(_dialogue, passive: true);
                }
                _hasTriggered = true;
                StartCoroutine(ForcePlayerBack(player));
            }
        }

        private IEnumerator ForcePlayerBack(MovementController player)
        {
            _isProcessing = true;

            ViewController view = UTGameManager.PlayerViewController;
            AnimationController anim = player.GetComponentInChildren<AnimationController>();

            player.Freeze();

            Vector3 pushDirection = (player.transform.position - transform.position).normalized;
            pushDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(pushDirection);

            Vector3 finalPosition = player.transform.position + pushDirection * _pushBackDistance;

            bool wasSprinting = player.IsSprinting;
            float walkSpeed = wasSprinting ? player.Settings.Speed * player.Settings.SprintSpeedMul : player.Settings.Speed;

            if (anim != null) anim.SetWalking(true);

            float pushBackDistance = _pushBackDistance * (wasSprinting ? player.Settings.SprintSpeedMul : 1.0f);

            float travelDuration = pushBackDistance / walkSpeed;
            view.LookAtPosition(finalPosition, travelDuration);

            float traveled = 0;
            while (traveled < pushBackDistance)
            {
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );

                player.ApplyGravity();
                if (anim) anim.SetSprinting(wasSprinting);

                float step = walkSpeed * Time.deltaTime;
                player.Move(pushDirection * step);

                player.MoveController();

                traveled += step;
                yield return null;
            }

            if (anim) anim.SetSprinting(player.IsSprinting);

            walkSpeed = (player.IsSprinting) ? player.Settings.Speed : player.Settings.Speed * player.Settings.SprintSpeedMul;
            player.SetHorizontalVelocity(pushDirection * walkSpeed);

            player.Unfreeze();
            _isProcessing = false;
        }
    }
}