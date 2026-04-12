using InteractionSystem.Controls;
using InteractionSystem.UI;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ColbyO.Untitled.UI
{
    public class MainMenuView : View
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _view;

        [SerializeField] private EventButton _play;
        [SerializeField] private EventButton _settings;
        [SerializeField] private EventButton _quit;

        [SerializeField] private GameObject _mainMenuCamera;
        [SerializeField] private GameObject _playerCamera;

        private void Update()
        {
            HandleCursor();
        }

        public override void Init()
        {
            _play.onPointerDown.AddListener(Play);
            _settings.onPointerDown.AddListener(Settings);
            _quit.onPointerDown.AddListener(Quit);
        }

        public override void Show()
        {
            base.Show();

            _view.SetActive(true);

            GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = true;

            VirtualCaster.ShowCursor();
            UTGameManager.HideCursor();

            UTGameManager.HasStarted = false;

            UTGameManager.PlayerInteractiorController.Controls.InspectionClickAction.performed += OnClick;

            UTGameManager.LockMovement = true;
        }

        public override void Hide()
        {
            base.Hide();
            VirtualCaster.HideCursor();
            UTGameManager.PlayerInteractiorController.Controls.InspectionClickAction.performed -= OnClick;
        }

        public void OnClick(InputAction.CallbackContext ctx)
        {
            ClickUI();
        }

        private void HandleCursor()
        {
            float sensitivity = InputDeviceHandler.IsCurrentGamepad
                ? UTGameManager.PlayerInteractiorController.Controls.ControllerMouseSensitivity
                : UTGameManager.PlayerInteractiorController.Controls.KeybaordMouseSensitivity;

            Vector2 delta = UTGameManager.PlayerInteractiorController.Controls.InspectionCursorAction.ReadValue<Vector2>() * sensitivity;

            VirtualCaster.WrapCursorPosition(delta);
        }

        public void ClickUI()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = VirtualCaster.GetVirtualMousePosition(),
                button = PointerEventData.InputButton.Left
            };

            List<RaycastResult> results = new List<RaycastResult>();
            _canvas.GetComponent<GraphicRaycaster>().Raycast(eventData, results);

            if (results.Count == 0) return;

            GameObject go = results[0].gameObject;

            ExecuteEvents.Execute(go, eventData, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(go, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(go, eventData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(go, eventData, ExecuteEvents.pointerClickHandler);
        }

        private void Play()
        {
            UTGameManager.HideCursor();
            VirtualCaster.HideCursor();

            GameManager.GetMonoSystem<IVisualEffectMonoSystem>().FadeOut(3f)
            .Then(_ =>
            {
                UTGameManager.HasStarted = true;

                _view.SetActive(false);
                _mainMenuCamera.SetActive(false);
                _playerCamera.SetActive(true);

                GameManager.GetMonoSystem<IVisualEffectMonoSystem>().FadeIn(5f);

                GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent("Act1");
            });
        }

        private void Settings()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<SettingsView>();
        }

        private void Quit()
        {
            Application.Quit();
        }
    }
}
