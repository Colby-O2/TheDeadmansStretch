using ColbyO.Untitled.MonoSystems;
using ColbyO.Untitled.UI;
using InteractionSystem.Controls;
using InteractionSystem.UI;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace ColbyO.Untitled
{
    public class EndView : View
    {
        [SerializeField] private Canvas _canvas;


        [SerializeField] private GameObject _menuView;
        [SerializeField] private GameObject _menuCamera;
        [SerializeField] private GameObject _playerCamera;


        private void Update()
        {

        }

        public override void Init()
        {

        }

        public override void Show()
        {
            base.Show();
            _menuCamera.SetActive(true);
            _playerCamera.SetActive(false);
            _menuView.SetActive(true);

            InputSystem.onAnyButtonPress.Call(ctrl => Quit());

            GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = true;
            GameManager.GetMonoSystem<ICinematicMonoSystem>().Disabe();
        }

        private void Quit()
        {
            Debug.Log("Quitting");
            Application.Quit();
        }
    }
}
