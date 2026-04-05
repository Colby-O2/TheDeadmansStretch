using DialogueGraph;
using PlazmaGames.Core;
using UnityEngine;

namespace ColbyO.Untitled.MonoSystems
{
    public class DialogueMonoSystem : DialogueController, IDialogueMonoSystem
    {
        private GameObject _cinematicView;

        private void Start()
        {
            _cinematicView = GameObject.FindWithTag("CinematicView");
            DisabeCinematicCamera();
        }

        private void StartDialogue(string dialogueName, System.Action finsihCallback, bool passive)
        {
            //GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SetPassive(passive);
            base.StartDialogue(dialogueName, finsihCallback);
        }
        public override void StartDialogue(string dialogueName, System.Action finsihCallback = null)
        {
            StartDialogue(dialogueName, finsihCallback, false);
        }

        public Promise StartDialoguePromise(string dialogueName, bool passive = false)
        {
            Promise p = new Promise();
            StartDialogue(dialogueName, () => p.Resolve(), passive);
            return p;
        }

        protected override void EnableCinematicCamera()
        {
            GameManager.GetMonoSystem<ICinematicMonoSystem>().Enable();
        }

        protected override void DisabeCinematicCamera()
        {
            GameManager.GetMonoSystem<ICinematicMonoSystem>().Disabe();
        }

        protected override void MoveCinematicCamera(Transform loc, Transform lookAt, Transform parnet)
        {
            GameManager.GetMonoSystem<ICinematicMonoSystem>().MoveTo(loc, lookAt: lookAt, parnet: parnet);
        }

        protected override void TransitionCinematicCamera(Transform from, Transform to, Transform lookAt, float duration, Transform parent = null)
        {
            GameManager.GetMonoSystem<ICinematicMonoSystem>().HandleCameraTransition(from, to, lookAt, duration, parent)
            .Then(_ =>
            {
                NextNode(0);
            });
        }

        protected override void CinematicCameraLookAt(Transform lookAtLoc, float duration)
        {
            GameManager.GetMonoSystem<ICinematicMonoSystem>().HandleCameraLookAt(lookAtLoc, duration)
            .Then(_ =>
            {
                NextNode(0);
            });
        }

        public override void OnDialogueFinished()
        {
            //GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().HideDialogue();
        }

        protected override void PlayDialogueNode()
        {
            //GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ShowDialogue();
            //GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().DisplayDialogue(_currentDialogueNode);
            StartCoroutine(WaitThenNext(2f));
        }
    }
}