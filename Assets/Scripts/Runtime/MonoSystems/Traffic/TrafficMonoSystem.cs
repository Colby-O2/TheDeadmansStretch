using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace ColbyO.Untitled
{
    public class TrafficMonoSystem : MonoBehaviour, ITrafficMonoSystem
    {
        [SerializeField] private GameObject _carPrefab;
        [SerializeField] private float _spawnInterval = 2.0f;
        [SerializeField] private float _carSpeed = 15f;

        private SplineContainer _roadSplines;
        private bool _isLeftLandDisabled;

        private List<GameObject> _activeCars = new List<GameObject>();

        public bool Enabled { get; set; }

        private void Start()
        {
            Enabled = true;
            _roadSplines = GameObject.FindWithTag("TrafficLanes").GetComponent<SplineContainer>();
            InvokeRepeating(nameof(SpawnCar), 0f, _spawnInterval);
        }

        public void DisableLeftLane(bool state)
        {
            _isLeftLandDisabled = state;

            if (_isLeftLandDisabled)
            {
                DestroyLeftLaneCars();
            }
        }

        private void DestroyLeftLaneCars()
        {
            for (int i = _activeCars.Count - 1; i >= 0; i--)
            {
                GameObject car = _activeCars[i];
                if (car == null) continue;

                if (car.TryGetComponent(out SplineFollower follower))
                {
                    if (follower.SplineIndex == 1)
                    {
                        _activeCars.RemoveAt(i);
                        Destroy(car);
                    }
                }
            }
        }

        private void SpawnCar()
        {
            if (UTGameManager.IsPaused || !Enabled) return;

            int laneIndex = Random.Range(0, 2);

            if (_isLeftLandDisabled && laneIndex == 1) return;

            Vector3 startPos = _roadSplines.EvaluatePosition(laneIndex, 0f);

            GameObject newCar = Instantiate(_carPrefab, startPos, Quaternion.identity);
            newCar.transform.SetParent(transform);
            _activeCars.Add(newCar);

            if (newCar.TryGetComponent(out SplineFollower follower))
            {
                follower.Initialize(_roadSplines, laneIndex, _carSpeed)
                .Then(_ =>
                {
                    if (newCar != null)
                    {
                        _activeCars.Remove(newCar);
                        Destroy(newCar);
                    }
                });
            }
        }
    }
}