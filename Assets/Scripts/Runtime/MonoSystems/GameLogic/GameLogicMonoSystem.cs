using ColbyO.Untitled.Player;
using ColbyO.Untitled.Traffic;
using ColbyO.Untitled.UI;
using InteractionSystem;
using InteractionSystem.Helpers;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Debugging;
using PlazmaGames.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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

            public static SightingScene SightingScene;

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

            public static SplineFollower KillerSplineFollower;
            public static SplineFollower PlayerSplineFollower;

            public static Openable EndDriverDoor;
            public static Openable EndDoor;
            public static SplineContainer EndingPath;

            public static SplineFollower SerialKillerCar;
            public static EngineSound SerialKillerEngine;

            public static RestrictedAreaTrigger RoadOOB;
            public static RestrictedAreaTrigger ParkOOB;

            public static Transform PlayerJumpLoc;
            public static Transform KillerJumpLoc;

            public static List<Interactable> Interactables;

            public static GameObject PushPerson;
            public static GameObject PushPersonRig;
            public static Rigidbody PushPersonRb;
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

        }

        private void Update()
        {
            _scheduler.Tick(Time.deltaTime);
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _dialogueMs = GameManager.GetMonoSystem<IDialogueMonoSystem>();

            Refs.PushPerson = GameObject.FindWithTag("PushPerson");
            Refs.PushPersonRig = GameObject.FindWithTag("PushPersonRig");
            Refs.PushPersonRb = GameObject.FindWithTag("PushPersonRb").GetComponent<Rigidbody>();
            Refs.PushPersonRig.SetActive(false);

            Refs.SightingScene = GameObject.FindAnyObjectByType<SightingScene>();

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

            Refs.KillerSplineFollower = Refs.SerialKiller.GetComponent<SplineFollower>();
            Refs.PlayerSplineFollower = UTGameManager.PlayerMoveController.GetComponent<SplineFollower>();

            Refs.EndDoor = GameObject.FindWithTag("Act1_EndDoor").GetComponent<Openable>();
            Refs.EndDriverDoor = GameObject.FindWithTag("Act1_EndingDriversDoor").GetComponent<Openable>();
            Refs.EndingPath = GameObject.FindWithTag("Act1_EndingPath").GetComponent<SplineContainer>();

            Refs.SerialKillerCar = GameObject.FindWithTag("Act1_SerialKillerCar").GetComponent<SplineFollower>();
            Refs.SerialKillerEngine = GameObject.FindWithTag("Act1_SerialKillerCar").GetComponent<EngineSound>();

            Refs.RoadOOB = GameObject.FindWithTag("Act1_RoadOOB").GetComponent<RestrictedAreaTrigger>();
            Refs.ParkOOB = GameObject.FindWithTag("Act1_ParkOOB").GetComponent<RestrictedAreaTrigger>();

            Refs.KillerJumpLoc = GameObject.FindWithTag("Act1_KillerJumpLoc").transform;
            Refs.PlayerJumpLoc = GameObject.FindWithTag("Act1_PlayerJumpLoc").transform;

            Refs.Interactables = FindObjectsByType<Interactable>().ToList();
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

                    UTGameManager.PlayerInteractiorController.CanInteract = true;

                    UTGameManager.PlayerAnimationController.SetFlag("InDriverSeat", true);
                    UTGameManager.PlayerAnimationController.SetFlag("IsParked", false);

                    UTGameManager.PlayerViewController.EnableCamera = false;

                    UTGameManager.PlayerWalkingAudio.Enabled = false;
                    UTGameManager.GetMonoSystem<IInventoryMonoSystem>().TakeItem("Camera");

                    GameManager.GetMonoSystem<ICinematicMonoSystem>().Disabe();

                    Refs.PlayerCarDoor.GetAction<CarGetOutAction>().IsEnabled = true;
                    Refs.PlayerCarDoor.CanInteract = false;

                    Refs.CameraInteractable.CanInteract = true;
                    Refs.CameraInteractable.gameObject.SetActive(true);
                    Refs.CameraInteractable.GetAction<TakeAction>().IsEnabled = false;
                    
                    Refs.PushPerson.SetActive(false);

                    Refs.PlayerCarAudio.ToggleEngine(true);

                    Refs.SerialKillerEngine.ToggleEngine(false);

                    GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = true;
                    GameManager.GetMonoSystem<ITrafficMonoSystem>().DisableLeftLane(true);

                    Refs.RoadOOB.SetDialogue("RoadOOB");
                    Refs.ParkOOB.SetDialogue("ParkOOB");
                    Refs.RoadOOB.gameObject.SetActive(false);
                    Refs.ParkOOB.gameObject.SetActive(false);

                    Refs.PlayerCarController.Initialize(Refs.TrafficSpline, 2, 10f)
                    .Then(_ =>
                    {
                        Refs.RoadOOB.gameObject.SetActive(true);
                        GameManager.GetMonoSystem<ITrafficMonoSystem>().DisableLeftLane(false);
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
                        Debug.Log("HERE  :)");
                    })
                    .Then(_ => _scheduler.When(() => IsInRange("PhotoArea")))
                    .Then(_ => 
                    {
                        Refs.ParkOOB.gameObject.SetActive(true);

                        GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = false;

                        UTGameManager.PlayerViewController.EnableCamera = true;
                        GameManager.GetMonoSystem<IUIMonoSystem>().GetView<PolaroidView>().SetHints(true);
                        GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SetCameraHint(true);

                        float t = 0f;
                        Refs.TrafficSpline.Evaluate(3, t, out float3 pos, out float3 tangent, out float3 _);
                        Quaternion rot = Quaternion.LookRotation(tangent, Vector3.up);
                        Transform car = Refs.PlayerCarController.transform;
                        car.SetPositionAndRotation(pos, rot);

                        return _dialogueMs.StartDialoguePromise("Arrive", true);
                    })
                    .Then(_ => _scheduler.When(() => IsTriggered("GotPhotos")))
                    .Then(_ =>
                    {
                        UTGameManager.PlayerViewController.ToggleFirstPerson(false);
                        Refs.SightingScene.StartScene();
                    })
                    .Then(_ => _scheduler.Wait(4))
                    .Then(_ =>
                    {
                        Refs.SightingScene.PlayGunshot();
                        GameManager.GetMonoSystem<IFowlMonoSystem>().ForceAllToFlyOff();
                        UTGameManager.PlayerViewController.ToggleFirstPerson(false);
                    })
                    .Then(_ => _scheduler.Wait(0.4f))
                    .Then(_ =>
                    {
                        GameManager.GetMonoSystem<IUIMonoSystem>().GetView<PolaroidView>().SetHints(false);
                        Refs.ParkOOB.SetDialogue("Park2OOB");
                        return _dialogueMs.StartDialoguePromise("Gunshot", passive: true);
                    })
                    .Then(_ => _scheduler.When(() => Refs.SightingScene.IsCameraLookingAtScene()))
                    .Then(_ =>
                    {
                        UTGameManager.PlayerMoveController.Freeze();
                        Refs.SightingScene.LookAtScene();
                    })
                    .Then(_ => _scheduler.Wait(1.7f))
                    .Then(_ => _dialogueMs.StartDialoguePromise("SceneComments"))
                    .Then(_ => _scheduler.Wait(1.0f))
                    .Then(_ =>
                    {
                        Refs.SightingScene.PushPerson();
                    })
                    .Then(_ => _scheduler.When(() => !Refs.SightingScene.IsFalling()))
                    .Then(_ =>
                    {
                        UTGameManager.PlayerMoveController.Unfreeze();
                        UTGameManager.PlayerViewController.ToggleFirstPerson(false);
                    })
                    .Then(_ =>
                    {
                        UTGameManager.PlayerViewController.EnableCamera = false;
                        Refs.ParkOOB.gameObject.SetActive(false);
                        GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = true;
                        Refs.RoadOOB.SetDialogue("Road2OOB");
                        return _dialogueMs.StartDialoguePromise("GottaGo", passive: true);
                    })
                     //Car Off
                    .Then(_ =>
                    {
                        Refs.PlayerCarDoor.CanInteract = true;
                        Refs.PlayerCarDoor.GetAction<CarGetOutAction>().IsEnabled = true;
                        return Refs.PlayerCarDoor.GetAction<CarGetOutAction>().WaitForDoorToOpen();
                    })
                    .Then(_ =>
                    {
                        UTGameManager.PlayerMoveController.Snow.Enable = false;

                        UTGameManager.PlayerInteractiorController.CanInteract = false;
                        foreach (Interactable interactable in Refs.Interactables) interactable.CanInteract = false;

                        Refs.RoadOOB.gameObject.SetActive(false);
                        GameManager.GetMonoSystem<ITrafficMonoSystem>().DisableLeftLane(true);
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

                        Refs.PlayerCarAudio.ToggleEngine(true);

                        Refs.PlayerCarDoor.GetAction<CarGetOutAction>().Door.Close();
                    })
                    .Then(_ =>
                    {
                        Refs.SerialKiller.ToggleAudio(false);
                        UTGameManager.PlayerAnimationController.SetFlag("IsParked", false);

                        Refs.PlayerCarController.WaitFor(0.95f).Then(_ =>
                        {
                            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio("CarStop", PlazmaGames.Audio.AudioType.Sfx, false, true);
                        });

                        return Refs.PlayerCarController.Initialize(Refs.TrafficSpline, 3, 20f);
                    })
                    .Then(_ =>
                    {
                        GameManager.GetMonoSystem<ITrafficMonoSystem>().Enabled = false;
                        return Refs.SerialKillerCarDoor1.Open();
                    })
                    .Then(_ =>
                    {
                        Refs.SerialKiller.ToggleAudio(true);
                        UTGameManager.PlayerViewController.SetAutoLookTarget(Refs.SerialKiller.GetHeadLoc());
                        return Refs.SerialKiller.GetOutOfCar(Refs.SerialKillerGetOutLoc, 1f);
                    })
                    .Then(_ => Refs.SerialKiller.Goto(Refs.SerialKillerGotoLoc, 3f, 1f))
                    .Then(_ => GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("TalkWithKiller", passive: false))
                    .Then(_ => Refs.SerialKiller.Shoot())
                    .Then(_ =>
                    {
                        UTGameManager.PlayerMoveController.Snow.SetTarget(GameManager.GetMonoSystem<ICinematicMonoSystem>().GetCameraVelocity());
                        UTGameManager.PlayerMoveController.Snow.gameObject.GetComponent<FollowXZ>().SetTarget(GameManager.GetMonoSystem<ICinematicMonoSystem>().GetCameraVelocity().transform);
                        UTGameManager.PlayerMoveController.Snow.gameObject.GetComponent<FollowXZ>().SetFollowAtHeight(true, 10.0f);
                        return GameManager.GetMonoSystem<IVisualEffectMonoSystem>().FadeOut(2f);
                    })
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
                    Refs.SerialKillerCar.WaitFor(0.75f).Then(_ =>
                        GameManager.GetMonoSystem<ICinematicMonoSystem>().HandleCameraTransition("CC_End1", "CC_End2", string.Empty, 2f));

                    return Refs.SerialKillerCar.Initialize(Refs.TrafficSpline, 4, 7.5f);
                })
                .Then(_ => Refs.EndDriverDoor.Open())
                .Then(_ =>
                {
                    Refs.KillerSplineFollower.AllowRotate = true;
                    Transform splineTransform = Refs.EndingPath.transform;

                    Refs.EndingPath.Splines[0].Evaluate(0f, out float3 localPos, out float3 localTangent, out float3 localUp);

                    Vector3 worldPos = splineTransform.TransformPoint(localPos);
                    Vector3 worldTangent = splineTransform.TransformDirection(localTangent);
                    Vector3 worldUp = splineTransform.TransformDirection(localUp);
                    Quaternion lookRotation = Quaternion.LookRotation(worldTangent, worldUp);

                    return Refs.SerialKiller.GetOutOfCar(worldPos, lookRotation, 2f);
                })
                .Then(_ =>
                {
                    Refs.SerialKiller.ToggleAudio(true);
                    return Refs.KillerSplineFollower.Initialize(Refs.EndingPath, 0, 4f);
                })
                .Then(_ =>
                {
                    Refs.KillerSplineFollower.AllowRotate = false;
                    Refs.SerialKiller.SetAlwaysLookAt(UTGameManager.PlayerMoveController.transform);
                    return Refs.SerialKiller.Shoot();
                })
                .Then(_ => Refs.EndDoor.Open())
                .Then(_ =>
                {
                    UTGameManager.PlayerMoveController.UnfreezeJustMovement();
                    UTGameManager.PlayerMoveController.Freeze();

                    Transform splineTransform = Refs.EndingPath.transform;

                    Refs.EndingPath.Splines[0].Evaluate(0.85f, out float3 localPos, out float3 localTangent, out float3 _);

                    Vector3 worldPos = splineTransform.TransformPoint(localPos);
                    Quaternion worldRot = Quaternion.LookRotation(splineTransform.TransformDirection(localTangent));

                    return UTGameManager.PlayerMoveController.GetOutOfCar(worldPos.SetY(worldPos.y + 1f), worldRot, 1.5f);
                })
                .Then(_ => Refs.EndDoor.Close())
                .Then(_ =>
                {
                    Transform splineTransform = Refs.EndingPath.transform;

                    Refs.EndingPath.Splines[1].Evaluate(0.05f, out float3 localPos, out float3 localTangent, out float3 _);

                    Vector3 worldPos = splineTransform.TransformPoint(localPos);
                    Quaternion worldRot = Quaternion.LookRotation(splineTransform.TransformDirection(localTangent));

                    Promise player = UTGameManager.PlayerMoveController.GetOutOfCar(worldPos.SetY(worldPos.y + 1f), worldRot, 1.5f);

                    Refs.EndingPath.Splines[1].Evaluate(0.0f, out localPos, out localTangent, out float3 _);

                    worldPos = splineTransform.TransformPoint(localPos);
                    worldRot = Quaternion.LookRotation(splineTransform.TransformDirection(localTangent));

                    Promise killer = Refs.SerialKiller.Goto(worldPos, worldRot, 1.5f);

                    return Promise.All(player, killer);
                })
                .Then(_ =>
                {
                    Refs.PlayerSplineFollower.HeightOffset = 1f;
                    Refs.KillerSplineFollower.AllowRotate = true;
                    Refs.SerialKiller.SetAlwaysLookAt(null);

                    UTGameManager.PlayerWalkingAudio.Enabled = true;

                    UTGameManager.PlayerAnimationController.SetFlag("IsWalking", true);
                    Refs.SerialKiller.GetAnimator().SetBool("IsWalking", true);

                    Refs.PlayerSplineFollower.WaitFor(0.3f).Then(_ =>
                    {
                        GameManager.GetMonoSystem<ICinematicMonoSystem>().MoveTo("CC_End3");
                    });

                    Refs.PlayerSplineFollower.WaitFor(0.6f).Then(_ =>
                    {
                        GameManager.GetMonoSystem<ICinematicMonoSystem>().MoveTo("CC_End4");
                    });

                    return Promise.All(
                        Refs.PlayerSplineFollower.Initialize(Refs.EndingPath, 1, 15f, startT: 0.05f),
                        Refs.KillerSplineFollower.Initialize(Refs.EndingPath, 1, 15f, startT: 0f, endT: 0.975f)
                    );
                })
                .Then(_ =>
                {
                    GameManager.GetMonoSystem<ICinematicMonoSystem>().MoveTo("CC_End5");
                    UTGameManager.PlayerAnimationController.SetFlag("IsWalking", false);
                    Refs.SerialKiller.GetAnimator().SetBool("IsWalking", false);

                    return GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("Jump1", passive: true);
                })
                .Then(_ =>
                {
                    UTGameManager.PlayerAnimationController.SetFlag("IsWalking", true);
                    Promise playerMove = UTGameManager.PlayerMoveController.TransitionTo(Refs.PlayerJumpLoc, 2.0f);
                    return playerMove;
                })
                .Then(_ =>
                {
                    UTGameManager.PlayerAnimationController.SetFlag("IsWalking", false);
                    return Refs.SerialKiller.Goto(Refs.KillerJumpLoc, 2.0f, 2.0f);
                })
                .Then(_ =>
                {
                    GameManager.GetMonoSystem<ICinematicMonoSystem>().MoveTo("CC_End6");
                })
                .Then(_ => GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("Jump2", passive: true))
                .Then(_ =>
                {
                    bool hasJumped = GameManager.GetMonoSystem<IDialogueMonoSystem>().GetFlag("Jump");

                    if (hasJumped) TriggerEvent("Jump");
                    else TriggerEvent("NoJump");
                });
                    break;

                case "Jump":
                    _dialogueMs.StartDialoguePromise("JumpOff");
                    _dialogueMs.AddListener("Push", _ =>
                    {
                        UTGameManager.PlayerMoveController.gameObject.SetActive(false);
                        Refs.PushPerson.SetActive(true);
                        Refs.PushPersonRig.SetActive(true);
                        Refs.PushPersonRb.AddForce(Vector3.back * 15.0f, ForceMode.VelocityChange);
                    });
                    break;
                case "NoJump":
                    GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise("NoJump", passive: true)
                    .Then(_ =>
                    {
                        GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio("Shoot", PlazmaGames.Audio.AudioType.Sfx, false, true);
                        return GameManager.GetMonoSystem<IVisualEffectMonoSystem>().FadeOut(1f);
                    })
                    .Then(_ => _scheduler.Wait(5f));
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

        public bool IsInRange(string rangeName) => _inRange.Contains(rangeName);
    }
}
