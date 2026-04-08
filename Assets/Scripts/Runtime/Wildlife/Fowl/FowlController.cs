using InteractionSystem.Helpers;
using PlazmaGames.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyO.Untitled.Wildlife
{
    public class FowlController : MonoBehaviour
    {
        [Header("State")]
        [SerializeField, ReadOnly] private FowlState _currentState = FowlState.Flying;

        private FowlSettings _settings;
        [SerializeField] private Transform _waterPlane;
        private List<Transform> _swimmingTargets;
        private BoxCollider _flightArea;

        public FowlState State { get => _currentState; set => _currentState = value; }
        public float WaterSwimHeight => _waterPlane.position.y + _settings.HeigtOffset;

        private List<FowlMember> _flock = new List<FowlMember>();
        private Vector3 _currentDestination;
        private bool _isChangingWaypoint = false;
        private Bounds _flightBounds;
        private float _waitTime;
        private float _elapsedTimeSinceMove;

        private void Update()
        {
            switch (_currentState)
            {
                case FowlState.Flying:
                    UpdateFlyingState();
                    break;
                case FowlState.Landing:
                    UpdateLandingState();
                    break;
                case FowlState.Swimming:
                    UpdateSwimmingState();
                    break;
                case FowlState.Takeoff:
                    UpdateTakeoffState();
                    break;
            }
        }

        public void Initialize
        (
            FowlState startState,
            BoxCollider flightArea,
            List<Transform> swimmingTargets,
            Transform waterPlane,
            FowlSettings settings
        )
        {
            _settings = settings;

            _swimmingTargets = swimmingTargets;

            _waterPlane = waterPlane;

            _currentState = startState;

            _flightArea = flightArea;
            _flightBounds = flightArea.bounds;

            Bounds rawBounds = flightArea.bounds;

            float margin = 2.0f;
            _flightBounds.Expand(new Vector3(margin * 2.1f, 100f, margin * 2.1f));

            SetState(startState);

            if (startState == FowlState.Flying) MoveToRandomPointInAirSpace(rawBounds, margin);
            else if (startState == FowlState.Swimming) SpawnAtRandomSwimmingWaypoint();

            SpawnFlock(Random.Range(_settings.FlockSize.x, _settings.FlockSize.y));
        }

        private void SpawnFlock(int flockSize)
        {
            for (int i = 0; i < flockSize; i++)
            {
                Vector3 vOffset;
                Quaternion initialRotation = transform.rotation;

                if (_currentState == FowlState.Flying)
                {
                    float horizontalStagger = _settings.VSpacing * 0.4f;
                    float verticalStagger = 0.5f;
                    int side = (i % 2 == 0) ? 1 : -1;
                    int row = (i + 1) / 2;
                    if (i == 0) row = 0;

                    vOffset = new Vector3(side * row * _settings.VSpacing, 0, -row * _settings.VSpacing);

                    vOffset.x += Random.Range(-horizontalStagger, horizontalStagger);
                    vOffset.y += Random.Range(-verticalStagger, verticalStagger);
                    vOffset.z += Random.Range(-horizontalStagger, horizontalStagger);
                }
                else
                {
                    Vector2 randomCircle = Random.insideUnitCircle * _settings.SwimSpread;
                    vOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
                    initialRotation = Quaternion.identity;
                }

                Fowl fowl = Instantiate<Fowl>(_settings.FowlPrefabs[Random.Range(0, _settings.FowlPrefabs.Count)], transform.position + vOffset, initialRotation, this.transform);

                fowl.Show(State);

                _flock.Add(new FowlMember
                {
                    Transform = fowl.transform,
                    Self = fowl,
                    GroupOffset = vOffset,
                    NoiseSeed = new Vector3(Random.Range(0f, 10000f), Random.Range(0f, 10000f), Random.Range(0f, 10000f)),
                    SpeedMultiplier = Random.Range(0.8f, 1.2f),
                    SwimPhaseShift = Random.Range(0f, Mathf.PI * 2),
                    NextIdleChangeTime = 0
                });
            }
        }

        private void TransitionToTakeoff()
        {
            _currentState = FowlState.Takeoff;

            float minDistance = 20f;
            float maxDistance = 60f;
            Vector3 targetPos = transform.position;

            Bounds spawnArea = _flightArea.bounds;

            float randomY = Random.Range(spawnArea.center.y, spawnArea.max.y);

            int attempts = 0;
            while (attempts < 15)
            {
                float x = Random.Range(spawnArea.min.x, spawnArea.max.x);
                float z = Random.Range(spawnArea.min.z, spawnArea.max.z);

                Vector2 flatCurrent = new Vector2(transform.position.x, transform.position.z);
                Vector2 flatTarget = new Vector2(x, z);
                float dist = Vector2.Distance(flatCurrent, flatTarget);

                if (dist >= minDistance && dist <= maxDistance)
                {
                    targetPos = new Vector3(x, randomY, z);
                    break;
                }
                attempts++;
            }

            if (attempts >= 15)
            {
                targetPos = transform.position + (transform.forward * maxDistance);
                targetPos.y = randomY;

                targetPos.x = Mathf.Clamp(targetPos.x, spawnArea.min.x, spawnArea.max.x);
                targetPos.z = Mathf.Clamp(targetPos.z, spawnArea.min.z, spawnArea.max.z);
            }

            _currentDestination = targetPos;

            for (int i = 0; i < _flock.Count; i++)
            {
                FowlMember bird = _flock[i];
                bird.Self.Show(FowlState.Flying);

                int side = (i % 2 == 0) ? 1 : -1;
                int row = (i + 1) / 2;
                if (i == 0) row = 0;

                Vector3 vOffset = new Vector3(side * row * _settings.VSpacing, 0, -row * _settings.VSpacing);

                float horizontalStagger = _settings.VSpacing * 1.5f;
                float verticalStagger = 1.2f;
                float forwardBackStagger = _settings.VSpacing * 1.0f;

                float noiseX = Mathf.Sin(bird.NoiseSeed.x * 7919f);
                float noiseY = Mathf.Sin(bird.NoiseSeed.y * 4409f);
                float noiseZ = Mathf.Sin(bird.NoiseSeed.z * 1327f);

                vOffset.x += noiseX * horizontalStagger;
                vOffset.y += noiseY * verticalStagger;
                vOffset.z += noiseZ * forwardBackStagger;

                bird.GroupOffset = vOffset;
            }
        }

        private void UpdateTakeoffState()
        {
            transform.position = Vector3.MoveTowards(transform.position, _currentDestination, _settings.MoveSpeed * _settings.FlyingSpeedMul * Time.deltaTime);

            Vector3 dir = (_currentDestination - transform.position).normalized;
            float distanceToHeight = Mathf.Abs(transform.position.y - _currentDestination.y);

            if (dir != Vector3.zero)
            {
                Quaternion climbRotation = Quaternion.LookRotation(dir);

                Quaternion levelRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                float t = Mathf.Clamp01(distanceToHeight / _settings.TakeoffLevelingZone);

                Quaternion targetRot = Quaternion.Slerp(levelRotation, climbRotation, t);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _settings.TurnSpeed * Time.deltaTime);
            }

            float startY = WaterSwimHeight;
            float targetY = _currentDestination.y;
            float currentY = transform.position.y;
            float formationWeight = Mathf.Clamp01((currentY - startY) / (targetY - startY));

            float time = Time.time * _settings.AirDriftSpeed;

            for (int i = 0; i < _flock.Count; i++)
            {
                FowlMember bird = _flock[i];

                float horizontalStagger = _settings.VSpacing * 1.5f;
                float verticalStagger = 1.2f;
                float forwardBackStagger = _settings.VSpacing * 1.0f;

                int side = (i % 2 == 0) ? 1 : -1;
                int row = (i + 1) / 2;
                if (i == 0) row = 0;

                Vector3 baseV = new Vector3(side * row * _settings.VSpacing, 0, -row * _settings.VSpacing);

                float noiseX = (Mathf.Sin(bird.NoiseSeed.x * 7919f));
                float noiseY = (Mathf.Sin(bird.NoiseSeed.y * 4409f));
                float noiseZ = (Mathf.Sin(bird.NoiseSeed.z * 1327f));

                Vector3 jitter = new Vector3(
                    noiseX * horizontalStagger,
                    noiseY * verticalStagger,
                    noiseZ * forwardBackStagger
                );

                Vector3 jitteredV = baseV + jitter;

                Vector3 targetLocalPos = Vector3.Lerp(bird.CurrentAnimatedOffset, jitteredV, formationWeight);

                bird.Transform.localPosition = Vector3.Lerp(bird.Transform.localPosition, targetLocalPos, Time.deltaTime * 4.0f);
                bird.Transform.localRotation = Quaternion.Slerp(bird.Transform.localRotation, Quaternion.identity, Time.deltaTime * _settings.TurnSpeed);
            }

            if (distanceToHeight < 0.1f || transform.position.y > _currentDestination.y)
            {
                _currentState = FowlState.Flying;

                transform.position = new Vector3(transform.position.x, _currentDestination.y, transform.position.z);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            }
        }

        private void UpdateLandingState()
        {
            Vector3 flatTarget = new Vector3(_currentDestination.x, transform.position.y, _currentDestination.z);
            float distanceXZ = Vector3.Distance(transform.position, flatTarget);
            float heightToWater = transform.position.y - WaterSwimHeight;

            float normalizedHeight = Mathf.Clamp01(heightToWater / _settings.LandingSlowdownDistance);
            float spreadFactor = 1.0f - (normalizedHeight * normalizedHeight);

            float time = Time.time * _settings.AirDriftSpeed;
            for (int i = 0; i < _flock.Count; i++)
            {
                FowlMember bird = _flock[i];

                int side = (i % 2 == 0) ? 1 : -1;
                int row = (i + 1) / 2;
                if (i == 0) row = 0;

                Vector3 vFormation = new Vector3(side * row * _settings.VSpacing, bird.GroupOffset.y, -row * _settings.VSpacing);

                Vector3 messyOffset = new Vector3(
                    (Mathf.Abs(bird.NoiseSeed.x % 8f) - 4f),
                    -bird.GroupOffset.y,
                    (Mathf.Abs(bird.NoiseSeed.z % 8f) - 4f)
                );
                Vector3 messyPos = vFormation + messyOffset;

                Vector3 targetOffset = Vector3.Lerp(vFormation, messyPos, spreadFactor);

                float x = (Mathf.PerlinNoise(time + bird.NoiseSeed.x, 0) - 0.5f) * _settings.AirDriftAmount;
                float z = (Mathf.PerlinNoise(time + bird.NoiseSeed.z, 0) - 0.5f) * _settings.AirDriftAmount;

                bird.Transform.localPosition = Vector3.Lerp(bird.Transform.localPosition, targetOffset + new Vector3(x, 0, z), Time.deltaTime * 4f);
            }

            Vector3 moveDir = (_currentDestination - transform.position).normalized;
            Vector3 nextPos = Vector3.MoveTowards(transform.position, _currentDestination, _settings.MoveSpeed * _settings.FlyingSpeedMul * Time.deltaTime);
            
            float arrivalSlowdown = Mathf.Max(Mathf.SmoothStep(0.2f, 1.0f, distanceXZ / 5f), 0.6f);
            transform.position = Vector3.MoveTowards(transform.position, nextPos, _settings.MoveSpeed * _settings.FlyingSpeedMul * arrivalSlowdown * Time.deltaTime);

            if (moveDir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDir);
                Quaternion levelRot = Quaternion.Euler(0, lookRot.eulerAngles.y, 0);

                float heightFactor = Mathf.Clamp01((heightToWater - 3.0f) / _settings.LandingRotationFlatDistance);

                Quaternion targetRot = Quaternion.Slerp(levelRot, lookRot, heightFactor);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _settings.TurnSpeed * Time.deltaTime);
            }

            if (Vector3.Distance(transform.position, _currentDestination.SetY(WaterSwimHeight)) < 0.5f)
            {
                SplashDown();
            }
        }

        private void SplashDown()
        {
            _currentState = FowlState.Swimming;
            _isChangingWaypoint = false;
            _elapsedTimeSinceMove = 0f;
            _waitTime = Random.Range(_settings.WaitAtWaypointTime.x, _settings.WaitAtWaypointTime.y);

            transform.position = new Vector3(transform.position.x, WaterSwimHeight, transform.position.z);

            foreach (FowlMember bird in _flock)
            {
                bird.Self.Show(FowlState.Swimming);

                Vector2 randomCircle = Random.insideUnitCircle * _settings.SwimSpread;
                bird.GroupOffset = new Vector3(randomCircle.x, 0, randomCircle.y);

                PickNewIdleSpot(bird);
            }
        }

        public void SetState(FowlState state)
        {
            foreach (FowlMember bird in _flock) bird.Self.Show(state);
        }

        private void SpawnAtRandomSwimmingWaypoint()
        {
            if (_swimmingTargets == null || _swimmingTargets.Count == 0)
            {
                Debug.LogWarning("No swimming targets assigned to FowlController!");
                return;
            }

            Transform randomWaypoint = _swimmingTargets[Random.Range(0, _swimmingTargets.Count)];

            Vector3 spawnPos = randomWaypoint.position;
            spawnPos.y = WaterSwimHeight;

            transform.position = spawnPos;

            _currentDestination = spawnPos;
            _isChangingWaypoint = false;
            _elapsedTimeSinceMove = 0f;
            _waitTime = Random.Range(_settings.WaitAtWaypointTime.x, _settings.WaitAtWaypointTime.y);
        }

        private void MoveToRandomPointInAirSpace(Bounds spawnArea, float margin)
        {
            int side = Random.Range(0, 4);

            Vector3 spawnPos = Vector3.zero;
            Vector3 targetPos = Vector3.zero;

            if (side == 0)
            {
                spawnPos = new Vector3(Random.Range(spawnArea.min.x, spawnArea.max.x), Random.Range(spawnArea.min.y, spawnArea.max.y), spawnArea.min.z - margin);
                targetPos = new Vector3(Random.Range(spawnArea.min.x, spawnArea.max.x), Random.Range(spawnArea.min.y, spawnArea.max.y), spawnArea.max.z);
            }
            else if (side == 1)
            {
                spawnPos = new Vector3(Random.Range(spawnArea.min.x, spawnArea.max.x), Random.Range(spawnArea.min.y, spawnArea.max.y), spawnArea.max.z + margin);
                targetPos = new Vector3(Random.Range(spawnArea.min.x, spawnArea.max.x), Random.Range(spawnArea.min.y, spawnArea.max.y), spawnArea.min.z);
            }
            else if (side == 2)
            {
                spawnPos = new Vector3(spawnArea.min.x - margin, Random.Range(spawnArea.min.y, spawnArea.max.y), Random.Range(spawnArea.min.z, spawnArea.max.z));
                targetPos = new Vector3(spawnArea.max.x, Random.Range(spawnArea.min.y, spawnArea.max.y), Random.Range(spawnArea.min.z, spawnArea.max.z));
            }
            else
            {
                spawnPos = new Vector3(spawnArea.max.x + margin, Random.Range(spawnArea.min.y, spawnArea.max.y), Random.Range(spawnArea.min.z, spawnArea.max.z));
                targetPos = new Vector3(spawnArea.min.x, Random.Range(spawnArea.min.y, spawnArea.max.y), Random.Range(spawnArea.min.z, spawnArea.max.z));
            }

            Vector3 travelDir = (targetPos - spawnPos).normalized;
            if (travelDir != Vector3.zero)
            {
                transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(travelDir));
            }
        }

        private void CheckForNearbyLandingSpot()
        {
            if (_swimmingTargets == null || _swimmingTargets.Count == 0) return;

            Transform closestTarget = null;
            float closestSqrDist = Mathf.Infinity;

            float detectionSqrRadius = _settings.LandingDetectionRadius * _settings.LandingDetectionRadius;

            foreach (Transform target in _swimmingTargets)
            {
                Vector2 flockPosXZ = new Vector2(transform.position.x, transform.position.z);
                Vector2 targetPosXZ = new Vector2(target.position.x, target.position.z);

                float sqrDist = (flockPosXZ - targetPosXZ).sqrMagnitude;

                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closestTarget = target;
                }
            }

            if (closestSqrDist <= detectionSqrRadius)
            {
                if (Random.value < (_settings.ChanceToLand * Time.deltaTime))
                {
                    TransitionToLanding(closestTarget);
                }
            }
        }

        private void TransitionToLanding(Transform target)
        {
            _currentState = FowlState.Landing;

            _currentDestination = target.position.SetY(WaterSwimHeight);

            foreach (FowlMember bird in _flock)
                bird.Self.Show(FowlState.Flying);
        }

        private void UpdateFlyingState()
        {
            transform.Translate(Vector3.forward * _settings.MoveSpeed * _settings.FlyingSpeedMul * Time.deltaTime);

            float time = Time.time * _settings.AirDriftSpeed;

            foreach (FowlMember bird in _flock)
            {
                float x = (Mathf.PerlinNoise(time + bird.NoiseSeed.x, 0) - 0.5f) * _settings.AirDriftAmount;
                float y = (Mathf.PerlinNoise(time + bird.NoiseSeed.y, 0) - 0.5f) * _settings.AirDriftAmount;
                float z = (Mathf.PerlinNoise(time + bird.NoiseSeed.z, 0) - 0.5f) * _settings.AirDriftAmount;


                bird.Transform.SetLocalPositionAndRotation(bird.GroupOffset + new Vector3(x, y, z), Quaternion.identity);
            }

            CheckForNearbyLandingSpot();


            if (!_flightBounds.Contains(transform.position))
            {
                Debug.Log("RIP Bird :<");
                Destroy(gameObject);
            }
        }

        private void UpdateSwimmingState()
        {
            if (_swimmingTargets.Count == 0) return;

            if (_isChangingWaypoint && Vector3.Distance(transform.position, _currentDestination) > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _currentDestination, _settings.MoveSpeed * _settings.SwimmingSpeedMul * Time.deltaTime);
                UpdateIndividualSwimmingBirds();
            }
            else if (_isChangingWaypoint)
            {
                _isChangingWaypoint = false;

                foreach (FowlMember bird in _flock) PickNewIdleSpot(bird);

                _waitTime = Random.Range(_settings.WaitAtWaypointTime.x, _settings.WaitAtWaypointTime.y);
                _elapsedTimeSinceMove = 0.0f;
            }
            else if (_elapsedTimeSinceMove < _waitTime)
            {
                UpdateIndividualSwimmingBirds();
                _elapsedTimeSinceMove += Time.deltaTime;
            }
            else
            {
                foreach (FowlMember bird in _flock)
                {
                    bird.GroupOffset = bird.Transform.localPosition.SetY(0.0f);
                }

                _currentDestination = _swimmingTargets[Random.Range(0, _swimmingTargets.Count)].position.SetY(WaterSwimHeight);

                _isChangingWaypoint = true;
            }

            if (!_isChangingWaypoint && Random.value < (_settings.ChanceToTakeoff * Time.deltaTime))
            {
                TransitionToTakeoff();
            }
        }

        private void PickNewIdleSpot(FowlMember bird)
        {
            Vector2 randomCircle = Random.insideUnitCircle * _settings.IdleRadius;
            bird.IdleLocalTarget = new Vector3(randomCircle.x, 0, randomCircle.y);
            bird.NextIdleChangeTime = Time.time + Random.Range(_settings.IdleWait.x, _settings.IdleWait.y);
        }

        private void UpdateIndividualSwimmingBirds()
        {

            for (int i = 0; i < _flock.Count; i++)
            {
                FowlMember bird = _flock[i];

                bird.CurrentAnimatedOffset = Vector3.Lerp(bird.CurrentAnimatedOffset, bird.GroupOffset, Time.deltaTime * 2f);

                Vector3 targetWorldPos;
                if (_isChangingWaypoint)
                {
                    float bobble = Mathf.Sin(Time.time + bird.SwimPhaseShift) * 0.1f;
                    targetWorldPos = transform.position + bird.CurrentAnimatedOffset + (transform.forward * bobble);
                }
                else
                {
                    if (Time.time >= bird.NextIdleChangeTime) PickNewIdleSpot(bird);
                    targetWorldPos = transform.position + bird.IdleLocalTarget;
                }

                float finalSpeed = _isChangingWaypoint ? _settings.MoveSpeed * _settings.SwimmingSpeedMul * bird.SpeedMultiplier : _settings.MoveSpeed * _settings.SwimmingSpeedMul * 0.5f;
                bird.Transform.position = Vector3.MoveTowards(bird.Transform.position, targetWorldPos, finalSpeed * Time.deltaTime);

                Vector3 lookDir = _isChangingWaypoint ? (_currentDestination - bird.Transform.position) : (targetWorldPos - bird.Transform.position);

                if (lookDir.sqrMagnitude > 0.01f)
                {
                    lookDir.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
                    bird.Transform.rotation = Quaternion.Slerp(bird.Transform.rotation, targetRotation, _settings.TurnSpeed * Time.deltaTime);
                }
            }
        }
    }
}
