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

        private void Start()
        {
            _roadSplines = GameObject.FindWithTag("TrafficLanes").GetComponent<SplineContainer>();
            InvokeRepeating(nameof(SpawnCar), 0f, _spawnInterval);
        }

        private void SpawnCar()
        {
            int laneIndex = Random.Range(0, 2);

            Vector3 startPos = _roadSplines.EvaluatePosition(laneIndex, 0f);

            GameObject newCar = Instantiate(_carPrefab, startPos, Quaternion.identity);

            if (newCar.TryGetComponent(out SplineFollower follower))
            {
                follower.Initialize(_roadSplines, laneIndex, _carSpeed);
            }
        }
    }
}