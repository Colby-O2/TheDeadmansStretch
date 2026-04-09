using ColbyO.Untitled.MonoSystems;
using InteractionSystem;
using InteractionSystem.Actions;
using PlazmaGames.Core;
using UnityEngine;

namespace ColbyO.Untitled.Interactables
{
    [System.Serializable]
    public class TalkAction : InteractionAction
    {
        [SerializeField] private string DialogueName;
        [SerializeField] private bool _isSingleUse;
        [SerializeField] private bool _isPassive;

        private Promise _promise;

        public override string ActionName => "Talk";

        public override void Execute(InteractorController interactor)
        {
            IsEnabled = !_isSingleUse;
            _owner.CanInteract = false;

            InteractionState prevState = UTGameManager.PlayerInteractiorController.CurrentState;
            if (!_isPassive)
            {
                UTGameManager.PlayerInteractiorController.ChangeState(InteractionState.Disabled);
            }

            GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise(DialogueName, passive: _isPassive)
            .Then(_ => { if (!_isPassive) UTGameManager.PlayerInteractiorController.ChangeState(prevState); })
            .Then(_ => UTGameManager.GlobalScheduler.Wait(0.2f))
            .Then(_ =>
            {
                Promise.ResolveExisting(ref _promise);
                _owner.CanInteract = true;
            });
        }

        public void SetDialogue(string dialogueName)
        {
            DialogueName = dialogueName;
        }

        public Promise AwaitTalk()
        {
            Promise.CreateExisting(ref _promise);
            return _promise;
        }
    }
}
