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

        private Promise _promise;

        public override string ActionName => "Talk";

        public override void Execute(InteractorController interactor)
        {
            IsEnabled = !_isSingleUse;
            _owner.CanInteract = false;
            GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise(DialogueName)
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
