using InteractionSystem;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColbyO.Untitled
{
    public class UTGameManager : GameManager
    {
        [SerializeField] private GameObject _monoSystemHolder;

        public static Preferences Preferences { get => (Instance as UTGameManager)._preferences; }
        [SerializeField] private Preferences _preferences;

        public static Scheduler GlobalScheduler = new Scheduler();

        public static bool IsPaused { get; set; }
        public static bool LockMovement { get; set; }
        public static bool HasStarted { get; set; }

        public static Player.ViewController PlayerViewController;
        public static Player.MovementController PlayerMoveController;
        public static Player.AnimationController PlayerAnimationController;
        public static WalkingSound PlayerWalkingAudio;
        public static InteractorController PlayerInteractiorController;

        private void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 1000;
        }

        private void Start()
        {
            HideCursor();
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

        private void Update()
        {
            GlobalScheduler.Tick(Time.deltaTime);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                UseCustomCursor();
            }
        }

        public static void UseCustomCursor()
        {
            if (Instance)
            {
                Cursor.SetCursor(Preferences.Cursor, Vector2.zero, CursorMode.Auto);
            }
        }

        public static void HideCursor()
        {
            UseCustomCursor();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void ShowCursor()
        {
            UseCustomCursor();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public override string GetApplicationName()
        {
            return nameof(UTGameManager);
        }

        public override string GetApplicationVersion()
        {
            return "v0.0.1";
        }

        protected override void OnInitalized()
        {
            _monoSystemHolder.SetActive(true);
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            PlayerInteractiorController = FindAnyObjectByType<InteractorController>();
        }

        private void OnSceneUnload(Scene scene)
        {
            RemoveAllEventListeners();
        }

        public static void QuitGame()
        {
            Application.Quit();
        }
    }
}
