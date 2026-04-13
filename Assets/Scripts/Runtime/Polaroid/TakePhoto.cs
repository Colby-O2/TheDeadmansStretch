using ColbyO.Untitled.MonoSystems;
using ColbyO.Untitled.Wildlife;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using AudioType = PlazmaGames.Audio.AudioType;
using IDialogueMonoSystem = ColbyO.Untitled.MonoSystems.IDialogueMonoSystem;

namespace ColbyO.Untitled.Polaroid
{
    public class TakePhoto : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private RenderTexture _cameraTexture;
        [SerializeField] private AudioClip _cameraShotSound;

        [Header("Input")]
        [SerializeField] private InputAction _captureAction;

        private CameraZoom _cameraZoom;
        [SerializeField] private GameObject _shutter;
        private float _shutterTime = 0f;
        private bool _gotDuck = false;
        private bool _gotGoose = false;
        private static TakePhoto _instance;

        public static void Capture()
        {
            _instance.CheckForBirds();

             GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_instance._cameraShotSound, AudioType.Sfx, false, true);
            _instance._shutterTime = 0;
        }

        private void CheckForBirds()
        {
            bool gotDuck = GameManager.GetMonoSystem<IDialogueMonoSystem>().GetFlag("GotDuck");
            bool gotGoose = GameManager.GetMonoSystem<IDialogueMonoSystem>().GetFlag("GotGoose");

            if (gotDuck && gotGoose) return;

            if (!GameManager.GetMonoSystem<IGameLogicMonoSystem>().IsInRange("PhotoArea"))
            {
                GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("NotInPhotoArea", true);
                return;
            }

            List<FlockController> flocks = GameManager.GetMonoSystem<IFowlMonoSystem>().GetFlocks();
            bool gotSomething = false;
            foreach (FlockController fc in flocks)
            {
                foreach (Fowl fowl in fc.GetFowls())
                {
                    Bounds b = fowl.GetActiveMesh().GetComponentInChildren<MeshRenderer>().bounds;
                    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
                    if (GeometryUtility.TestPlanesAABB(planes, b))
                    {
                        if (_cameraZoom.GetZoom() > UTGameManager.Preferences.PolaroidCameraZoomMinToTakePhoto)
                        {
                            GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("ZoomMore", true);
                            return;
                        }
                        switch (fowl.Species)
                        {
                            case FowlSpecies.Mallard:
                                GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("GotDuck", true);
                                GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("PictureOfDuck", true);
                                _gotDuck = true;
                                Debug.Log("Got a duck :)");
                                break;
                            case FowlSpecies.CanadaGoose:
                                GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("GotGoose", true);
                                GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("PictureOfGoose", true);
                                _gotGoose = true;
                                Debug.Log("Got a goose :(");
                                break;
                        }
                        gotSomething = true;
                        break;
                    }
                }

                if (gotSomething) break;
            }

            if (!gotSomething)
            {
                GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("PictureOfNothing", true);
            }

            if (_gotDuck && _gotGoose)
            {
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().Trigger("GotPhotos");
            }
        }

        private void Awake()
        {
            _instance = this;
            _cameraZoom = _camera.GetComponent<CameraZoom>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
            _captureAction.Enable();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
            _captureAction.Disable();
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _shutter.SetActive(false);
        }

        private void OnSceneUnload(Scene scene)
        {
        }


        private void Update()
        {
            if (_shutter == null) return;

            if (!_shutter.activeSelf && _shutterTime < UTGameManager.Preferences.PolaroidCameraShutterTime)
            {
                _shutter.SetActive(true);
            } else if (_shutter.activeSelf && _shutterTime > UTGameManager.Preferences.PolaroidCameraShutterTime)
            {
                _shutter.SetActive(false);
            }
            _shutterTime += Time.deltaTime;
            
            if (_captureAction.WasPressedThisFrame())
            {
                TakePhoto.Capture();
            }
        }
    }
}
