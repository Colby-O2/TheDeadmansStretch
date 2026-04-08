using ColbyO.Untitled.Wildlife;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColbyO.Untitled.MonoSystems
{
    public class FowlMonoSystem : MonoBehaviour, IFowlMonoSystem
    {
        [Header("Settings")]
        [SerializeField] private List<FowlSettings> _fowlSettings;
        [SerializeField] private int _maxFlocks = 5;
        [SerializeField] private int _initialSwimmingCount = 2;

        private BoxCollider _globalFlightArea;

        private Transform _waterPlane;

        private List<FowlController> _activeFlocks = new List<FowlController>();

        private List<Transform> _swimmingTargets = new List<Transform>();

        private void Start()
        {
            _globalFlightArea = GameObject.FindWithTag("FowlFlightArea").GetComponent<BoxCollider>();

            _waterPlane = GameObject.FindWithTag("Water").transform;

            FetchWaterTargets();

            for (int i = 0; i < _maxFlocks; i++)
            {
                FowlState startState = (i < _initialSwimmingCount) ? FowlState.Swimming : FowlState.Flying;
                SpawnFlock(startState);
            }
        }

        private void FetchWaterTargets()
        {
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("FowlWaterTarget");

            _swimmingTargets = targetObjects.Select(obj => obj.transform).ToList();
        }
        public void SpawnFlock(FowlState state)
        {
            if (_fowlSettings == null || _fowlSettings.Count == 0) return;

            FowlSettings settings = _fowlSettings[Random.Range(0, _fowlSettings.Count)];

            GameObject flockContainer = new GameObject($"{settings.SpeciesName}Flock");
            flockContainer.transform.parent = transform;

            flockContainer.AddComponent<FlockLifecycleHook>().OnFlockDestroyed += HandleFlockDestroyed;
            FowlController newFlock = flockContainer.AddComponent<FowlController>();

            newFlock.Initialize(state, _globalFlightArea, _swimmingTargets, _waterPlane, settings);

            _activeFlocks.Add(newFlock);

            _activeFlocks.Add(newFlock);
        }

        private void HandleFlockDestroyed(GameObject flockGO)
        {
            _activeFlocks.RemoveAll(f => f == null || f.gameObject == flockGO);

            SpawnFlock(FowlState.Flying);
        }
    }

    public class FlockLifecycleHook : MonoBehaviour
    {
        public System.Action<GameObject> OnFlockDestroyed;

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
            {
                OnFlockDestroyed?.Invoke(gameObject);
            }
        }
    }
}