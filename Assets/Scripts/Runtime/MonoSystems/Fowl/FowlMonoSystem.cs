using ColbyO.Untitled.Wildlife;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColbyO.Untitled.MonoSystems
{
    public class FowlMonoSystem : MonoBehaviour, IFowlMonoSystem
    {
        [Header("Settings")]
        [SerializeField] private FlockController _flockPrefab;
        [SerializeField] private List<FowlSettings> _fowlSettings;
        [SerializeField] private int _maxFlocks = 5;
        [SerializeField] private int _initialSwimmingCount = 2;
        [SerializeField, Range(0.0f, 1.0f)] private float _newFlockSpawnChance = 0.01f;
        [SerializeField, Range(0.0f, 1.0f)] private float _awakeSpawnChance = 0.75f;

        private BoxCollider _globalFlightArea;

        private Transform _waterPlane;

        private Queue<FlockController> _flockPool = new Queue<FlockController>();

        private List<FlockController> _activeFlocks = new List<FlockController>();

        private List<Transform> _swimmingTargets = new List<Transform>();

        private void Start()
        {
            _globalFlightArea = GameObject.FindWithTag("FowlFlightArea").GetComponent<BoxCollider>();

            _waterPlane = GameObject.FindWithTag("Water").transform;

            FetchWaterTargets();
            InitalizeFlockPool();

            for (int i = 0; i < _maxFlocks; i++)
            {
                FowlState startState = (i < _initialSwimmingCount) ? FowlState.Swimming : FowlState.Flying;
                if (i > _initialSwimmingCount && Random.value < _awakeSpawnChance) continue;

                SpawnFlock(startState);
            }
        }

        private void Update()
        {
            if (_activeFlocks.Count < _maxFlocks)
            {
                if (Random.value < _newFlockSpawnChance * Time.deltaTime)
                {
                    SpawnFlock(FowlState.Flying);
                }
            }
        }

        private void InitalizeFlockPool()
        {
            for (int i = 0; i < _maxFlocks; i++)
            {
                FowlSettings settings = _fowlSettings[Random.Range(0, _fowlSettings.Count)];

                FlockController newFlock = Instantiate<FlockController>(_flockPrefab);
                newFlock.gameObject.name = $"FowlFlock";
                newFlock.transform.parent = transform;
                newFlock.GetComponent<FlockLifecycleHook>().OnFlockDestroyed += HandleFlockDestroyed;
                newFlock.Initialize(_globalFlightArea, _swimmingTargets, _waterPlane, settings);
                newFlock.gameObject.SetActive(false);
                _flockPool.Enqueue(newFlock);
            }
        }

        private void FetchWaterTargets()
        {
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("FowlWaterTarget");

            _swimmingTargets = targetObjects.Select(obj => obj.transform).ToList();
        }

        public void SpawnFlock(FowlState state)
        {
            if (
                _fowlSettings == null || 
                _fowlSettings.Count == 0 || 
                _flockPool == null || 
                _flockPool.Count == 0
            ) return;

            

            FlockController flock = _flockPool.Dequeue();
            if (flock.gameObject != null && !flock.gameObject.activeSelf) flock.gameObject.SetActive(true);
            flock.Respawn(state);

            _activeFlocks.Add(flock);
        }

        private void HandleFlockDestroyed(GameObject flockGO)
        {
            FlockController flock = _activeFlocks.Find(f => f != null && f.gameObject == flockGO);
            _activeFlocks.RemoveAll(f => f == null || f.gameObject == flockGO);
            if (flock.gameObject != null && flock.gameObject.activeSelf) flock.gameObject.SetActive(false);
            _flockPool.Enqueue(flock);
        }
    }
}