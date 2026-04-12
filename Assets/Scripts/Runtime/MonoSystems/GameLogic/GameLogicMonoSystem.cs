using ColbyO.Untitled.Player;
using ColbyO.Untitled.Traffic;
using InteractionSystem;
using PlazmaGames.Core;
using PlazmaGames.Core.Debugging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace ColbyO.Untitled.MonoSystems
{
    public class GameLogicMonoSystem : MonoBehaviour, IGameLogicMonoSystem
    {
        private Scheduler _scheduler = new Scheduler();
        private HashSet<string> _inRange = new HashSet<string>();
        private HashSet<string> _triggers = new HashSet<string>();
        private bool _started = false;

        private IDialogueMonoSystem _dialogueMs;

        private static class Refs
        {
            /*Act 1*/
            public static Transform PlayerCarDriverSeatLoc;
            public static Transform PlayerCarCameraTarget;
            public static Transform PlayerCarHeadLoc;
            public static SplineFollower PlayerCarController;
            public static EngineSound PlayerCarAudio;
            public static List<GameObject> PlayerCarMirrorCameras = new List<GameObject>();

            public static Interactable PlayerCarDoor;
            public static Interactable CameraInteractable;

            public static SplineContainer TrafficSpline;

            public static Transform GetOutOfCarLoc;

            public static SerialKillerController SerialKiller;
            public static Transform SerialKillerGetOutLoc;
            public static Transform SerialKillerGotoLoc;
            public static Transform SerialKillerCarPlayerLoc;
            public static Transform SerialKillerCarKillerLoc;
            public static Openable SerialKillerCarDoor1;

            public static SplineFollower SerialKillerCar;
            public static EngineSound SerialKillerEngine;

        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }

        private void Start()
        {
            _dialogueMs = GameManager.GetMonoSystem<IDialogueMonoSystem>();

            Refs.PlayerCarDriverSeatLoc = GameObject.FindWithTag("Act1_PlayerCarDriverSeatLoc").transform;
            Refs.PlayerCarCameraTarget = GameObject.FindWithTag("Act1_PlayerCarCameraTarget").transform;
            Refs.PlayerCarHeadLoc = GameObject.FindWithTag("Act1_PlayerCarHeadLoc").transform;

            GameObject playerCar = GameObject.FindWithTag("Act1_PlayerCarController");
            Refs.PlayerCarController = playerCar.GetComponent<SplineFollower>();
            Refs.PlayerCarAudio = playerCar.GetComponent<EngineSound>();
            GameObject.FindGameObjectsWithTag("Act1_PlayerCarMirrorCamera", Refs.PlayerCarMirrorCameras);

            Refs.PlayerCarDoor = GameObject.FindWithTag("Act1_PlayerCarDoor").GetComponent<Interactable>();
            Refs.CameraInteractable = GameObject.FindWithTag("Act1_CameraInteractable").GetComponent<Interactable>();

            Refs.TrafficSpline = GameObject.FindWithTag("TrafficLanes").GetComponent<SplineContainer>();

            Refs.GetOutOfCarLoc = GameObject.FindWithTag("Act1_GetOutOfCarLoc").transform;

            Refs.SerialKiller = GameObject.FindWithTag("Act1_SerialKiller").GetComponent<SerialKillerController>();
            Refs.SerialKillerGetOutLoc = GameObject.FindWithTag("Act1_SerialKillerGetOutLoc").transform;
            Refs.SerialKillerGotoLoc = GameObject.FindWithTag("Act1_SerialKillerGotoLoc").transform;
            Refs.SerialKillerCarPlayerLoc = GameObject.FindWithTag("Act1_SerialKillerCarPlayerLoc").transform;
            Refs.SerialKillerCarKillerLoc = GameObject.FindWithTag("Act1_SerialKillerCarKillerLoc").transform;
            Refs.SerialKillerCarDoor1 = GameObject.FindWithTag("Act1_SerialKillerCarDoor1").GetComponent<Openable>();

            Refs.SerialKillerCar = GameObject.FindWithTag("Act1_SerialKillerCar").GetComponent<SplineFollower>();
            Refs.SerialKillerEngine = GameObject.FindWithTag("Act1_SerialKillerCar").GetComponent<EngineSound>();
        }

        private void Update()
        {
            _scheduler.Tick(Time.deltaTime);
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {

        }

        private void OnSceneUnload(Scene scene)
        {

        }

        public void TriggerEvent(string eventName)
        {
            PlazmaDebug.Log("Event Triggered", eventName, Color.green);

            switch (eventName)
            {
                case "Act1":
                    UTGameManager.LockMovement = false;

                    UTGameManager.PlayerMoveController.Attach(Refs.PlayerCarController.transform);
                    UTGameManager.PlayerMoveController.FreezeJustMovement();
                    UTGameManager.PlayerMoveController.DisableChacaterController();
                    UTGameManager.PlayerMoveController.TeleportTo(Refs.PlayerCarDriverSeatLoc.position);
                    UTGameManager.PlayerViewController.ToggleView(
                        PlayerViewType.Fixed, 
                        offsetOverride: new Vector3(0f, -1.58f, 0f),
                        fixedTarget: Refs.PlayerCarCameraTarget
                    );

                    UTGameManager.PlayerAnimationController.SetFlag("InDriverSeat", true);
                    UTGameManager.PlayerAnimationController.SetFlag("IsParked", false);

                    UTGameManager.PlayerWalkingAudio.Enabled = false;
                    UTGameManager.GetMonoSystem<IInventoryMonoSystem>().TakeItem("Camera");

                    Refs.PlayerCarDoor.GetAction<CarGetOutAction>().IsEnabled = true;
                    Refs.PlayerCarDoor.CanInteract = false;

                    Refs.CameraInteractable.CanInteract = true;
                    Refs.CameraInteractable.gameObject.SetActive(true);
                    Refs.CameraInteractable.GetAction<TakeAction>().IsEnabled = false;

                    Refs.PlayerCarAudio.SetRpmAndThrottle(250f, 0f);
                    Refs.PlayerCarAudio.ToggleEngine(true);

                    
                    Refs.SerialKillerEngine.SetRpmAndThrottle(250f, 0f);
                    Refs.SerialKillerEngine.ToggleEngine(false);

                    Refs.PlayerCarController.Initialize(Refs.TrafficSpline, 2, 30f)
                    .Then(_ =>
                    {
                        Refs.PlayerCarAudio.ToggleEngine(false);
                        UTGameManager.PlayerAnimationController.SetFlag("IsParked", true);
                        foreach (GameObject cam in Refs.PlayerCarMirrorCameras) cam.SetActive(false);

                        Promise dialoguePromise = GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("Act1_Arrival", passive: true);

                        return dialoguePromise;
                    })
                    .Then(_ =>
                    {
                        Refs.CameraInteractable.GetAction<TakeAction>().IsEnabled = true;
                        return _scheduler.When(() => GameManager.GetMonoSystem<IInventoryMonoSystem>().HasItem("Camera"));
                    })
                    .Then(_ =>
                    {
                        Refs.PlayerCarDoor.CanInteract = true;
                        return Refs.PlayerCarDoor.GetAction<CarGetOutAction>().WaitForDoorToOpen();
                    })
                    .Then(_ =>
                    {
                        UTGameManager.PlayerAnimationController.SetFlag("InDriverSeat", false);

                        Promise playerMovePromise = UTGameManager.PlayerMoveController.TransitionTo(Refs.GetOutOfCarLoc, 1f);

                        Promise cameraLerpPromise = UTGameManager.PlayerViewController.TransitionView(
                            PlayerViewType.ThirdPerson,
                            1f
                        );

                        return Promise.All(playerMovePromise, cameraLerpPromise);
                    })
                    .Then(_ =>
                    {
                        UTGameManager.PlayerWalkingAudio.Enabled = true;
                        UTGameManager.PlayerMoveController.Deattach();
                        UTGameManager.PlayerMoveController.UnfreezeJustMovement();
                        UTGameManager.PlayerMoveController.EnableChacaterController();
                        Refs.PlayerCarDoor.GetAction<CarGetOutAction>().Door.Close();
                    })
                    // Car Off
                    .Then(_ =>
                    {
                        Refs.PlayerCarDoor.CanInteract = true;
                        Refs.PlayerCarDoor.GetAction<CarGetOutAction>().IsEnabled = true;

                        return Refs.PlayerCarDoor.GetAction<CarGetOutAction>().WaitForDoorToOpen();
                    })
                    .Then(_ =>
                    {
                        UTGameManager.PlayerWalkingAudio.Enabled = false;
                        UTGameManager.PlayerMoveController.Freeze();
                        Promise playerMovePromise = UTGameManager.PlayerMoveController.TransitionTo(Refs.PlayerCarDriverSeatLoc, 1f);
                        Promise cameraLerpPromise = UTGameManager.PlayerViewController.TransitionView(
                            PlayerViewType.Fixed,
                            1f,
                            offsetOverride: new Vector3(0f, -1.58f, 0f),
                            fixedTarget: Refs.PlayerCarHeadLoc
                        );

                        UTGameManager.PlayerAnimationController.SetWalking(false);
                        UTGameManager.PlayerAnimationController.SetSprinting(false);
                        UTGameManager.PlayerAnimationController.SetFlag("InDriverSeat", true);
                        UTGameManager.PlayerAnimationController.SetFlag("IsParked", true);

                        return Promise.All(playerMovePromise, cameraLerpPromise);
                    })
                    .Then(_ =>
                    {
                        UTGameManager.PlayerMoveController.Unfreeze();
                        UTGameManager.PlayerMoveController.FreezeJustMovement();
                        UTGameManager.PlayerMoveController.DisableChacaterController();

                        UTGameManager.PlayerMoveController.Attach(Refs.PlayerCarController.transform);

                        Refs.PlayerCarAudio.SetRpmAndThrottle(250f, 0f);
                        Refs.PlayerCarAudio.ToggleEngine(true);

                        Refs.PlayerCarDoor.GetAction<CarGetOutAction>().Door.Close();
                    })
                    .Then(_ =>
                    {
                        Refs.SerialKiller.ToggleAudio(false);
                        UTGameManager.PlayerAnimationController.SetFlag("IsParked", false);
                        return Refs.PlayerCarController.Initialize(Refs.TrafficSpline, 3, 30f);
                    })
                    .Then(_ => Refs.SerialKillerCarDoor1.Open())
                    .Then(_ =>
                    {
                        Refs.SerialKiller.ToggleAudio(true);
                        UTGameManager.PlayerViewController.SetAutoLookTarget(Refs.SerialKiller.GetHeadLoc());
                        return Refs.SerialKiller.GetOutOfCar(Refs.SerialKillerGetOutLoc, 1f);
                    })
                    .Then(_ => Refs.SerialKiller.Goto(Refs.SerialKillerGotoLoc, 3f, 1f))
                    .Then(_ => Refs.SerialKiller.Shoot())
                    .Then(_ => GameManager.GetMonoSystem<IVisualEffectMonoSystem>().FadeOut(2f))
                    .Then(_ =>
                     {
                         Refs.SerialKiller.ToggleAudio(false);
                         Refs.SerialKillerEngine.ToggleEngine(true);

                         UTGameManager.PlayerMoveController.Attach(Refs.SerialKillerCar.transform);
                         UTGameManager.PlayerMoveController.TeleportTo(Refs.SerialKillerCarPlayerLoc.position, Refs.SerialKillerCarPlayerLoc.rotation);
                         UTGameManager.PlayerAnimationController.SetFlag("InDriverSeat", true);
                         UTGameManager.PlayerAnimationController.SetFlag("IsParked", true);

                         Refs.SerialKiller.Attach(Refs.SerialKillerCar.transform);
                         Refs.SerialKiller.TeleportTo(Refs.SerialKillerCarKillerLoc.position, Refs.SerialKillerCarKillerLoc.rotation);
                         Refs.SerialKiller.GetAnimator().SetBool("Shooting", false);
                         Refs.SerialKiller.GetAnimator().SetBool("InDriverSeat", true);

                         GameManager.GetMonoSystem<ICinematicMonoSystem>().Enable();
                         GameManager.GetMonoSystem<ICinematicMonoSystem>().MoveTo("CC_End1");
                         return GameManager.GetMonoSystem<IVisualEffectMonoSystem>().FadeIn(2f);
                     })
                    .Then(_ =>
                    {
                        Refs.SerialKillerCar.WaitHalf().Then(_ => GameManager.GetMonoSystem<ICinematicMonoSystem>().HandleCameraTransition("CC_End1", "CC_End2", string.Empty, 2f));
                        return Refs.SerialKillerCar.Initialize(Refs.TrafficSpline, 4, 30f);
                    });

                    break;
            }
        }

        public void Trigger(string triggerName)
        {
            _triggers.Add(triggerName);
        }

        public void SetInRange(string rangeName, bool state)
        {
            if (state) _inRange.Add(rangeName);
            else _inRange.Remove(rangeName);

            switch (rangeName)
            {
                default: break;
            }
        }

        private bool IsTriggered(string triggerName) => _triggers.Remove(triggerName);

        private bool IsInRange(string rangeName) => _inRange.Contains(rangeName);
    }
}