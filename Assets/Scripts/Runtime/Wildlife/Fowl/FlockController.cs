using PlazmaGames.Attribute;
using PlazmaGames.Math;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyO.Untitled.Wildlife
{
    public class FlockController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]  private FlockLifecycleHook _lifecycleHook;
        [SerializeField] private FlockAudio _audio;

        [Header("State")]
        [SerializeField, ReadOnly] private FowlState _currentState = FowlState.Flying;

        private FowlSettings _settings;
        [SerializeField] private Transform _waterPlane;
        private List<Transform> _swimmingTargets;
        private BoxCollider _flightArea;

        public FowlState State { get => _currentState; set => _currentState = value; }
        public float WaterSwimHeight => _waterPlane.position.y + _settings.HeigtOffset;

        private float _margin = 2.0f;
        private List<FowlMember> _flock = new List<FowlMember>();
        private Vector3 _currentDestination;
        private bool _isChangingWaypoint = false;
        private Bounds _flightBounds;
        private Bounds _expandedFlightBounds;
        private float _waitTime;
        private float _elapsedTimeSinceMove;
        private float _heightAtLandStart;

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
            BoxCollider flightArea,
            List<Transform> swimmingTargets,
            Transform waterPlane,
            FowlSettings settings
        )
        {
            _settings = settings;
            _swimmingTargets = swimmingTargets;
            _waterPlane = waterPlane;
            _currentState = FowlState.None;
            _flightArea = flightArea;

            _audio.SetSpecies(_settings.Species);

            _flightBounds = _flightArea.bounds;
            _flightBounds.Expand(new Vector3(_margin * 2.1f, 100f, _margin * 2.1f));

            SpawnFlock(Random.Range(_settings.FlockSize.x, _settings.FlockSize.y), true);
        }

        public void Respawn(FowlState state)
        {
            _currentState = state;
            SetState(state);

            if (state == FowlState.Flying) MoveToRandomPointInAirSpace(_flightArea.bounds, _margin);
            else if (state == FowlState.Swimming) SpawnAtRandomSwimmingWaypoint();

            SpawnFlock(_flock.Count, false);
        }

        private void SpawnFlock(int flockSize, bool init)
        {
            for (int i = 0; i < flockSize; i++)
            {
                if (init)
                {
                    Fowl fowl = Instantiate<Fowl>(_settings.FowlPrefabs[Random.Range(0, _settings.FowlPrefabs.Count)], Vector3.zero, Quaternion.identity, this.transform);

                    fowl.Show(State);

                    _flock.Add(new FowlMember
                    {
                        Transform = fowl.transform,
                        Self = fowl,
                        GroupOffset = Vector3.zero,
                        NoiseSeed = new Vector3(Random.Range(0f, 10000f), Random.Range(0f, 10000f), Random.Range(0f, 10000f)),
                        SpeedMultiplier = Random.Range(0.8f, 1.2f),
                        SwimPhaseShift = Random.Range(0f, Mathf.PI * 2),
                        NextIdleChangeTime = 0
                    });
                }
                else
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

                    FowlMember fowl = _flock[i];

                    fowl.Transform.SetPositionAndRotation(transform.position + vOffset, initialRotation);
                    fowl.Self.Show(State);

                    fowl.GroupOffset = vOffset;
                    fowl.NoiseSeed = new Vector3(Random.Range(0f, 10000f), Random.Range(0f, 10000f), Random.Range(0f, 10000f));
                    fowl.SpeedMultiplier = Random.Range(0.8f, 1.2f);
                    fowl.SwimPhaseShift = Random.Range(0f, Mathf.PI * 2);
                    fowl.NextIdleChangeTime = 0;
                }
            }
        }

        private void TransitionToTakeoff()
        {
            _currentState = FowlState.Takeoff;

            _audio.PlayTakeOffSound();

            float minDistance = 20f;
            float maxDistance = 60f;
            Vector3 targetPos = transform.position;

            Bounds spawnArea = _flightArea.bounds;

            float randomY = Random.Range(spawnArea.min.y, spawnArea.max.y);

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

            float normalizedHeight = Mathf.Clamp01(heightToWater / (_heightAtLandStart * _settings.LandingSlowdownDistance));
            float transitionProgress = 1.0f - normalizedHeight;

            for (int i = 0; i < _flock.Count; i++)
            {
                FowlMember bird = _flock[i];

                float angle = (bird.NoiseSeed.x % 100f) / 100f * Mathf.PI * 2f;
                float radiusMultiplier = (bird.NoiseSeed.z % 100f) / 100f;
                float radius = 3.0f * _settings.SwimSpread * radiusMultiplier;

                Vector3 circlePos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                Vector3 targetLocalPos = Vector3.Lerp(bird.GroupOffset, circlePos, transitionProgress);

                bird.Transform.localPosition = Vector3.SmoothDamp(
                    bird.Transform.localPosition,
                    targetLocalPos,
                    ref bird.MovementVelocity,
                    0.8f
                );
            }

            Vector3 moveDir = (_currentDestination - transform.position).normalized;
            Vector3 nextPos = Vector3.MoveTowards(transform.position, _currentDestination, _settings.MoveSpeed * _settings.FlyingSpeedMul * Time.deltaTime);
            
            float arrivalSlowdown = Mathf.Max(Mathf.SmoothStep(0.2f, 1.0f, distanceXZ / 5f), 0.6f);
            transform.position = Vector3.MoveTowards(transform.position, nextPos, _settings.MoveSpeed * _settings.FlyingSpeedMul * arrivalSlowdown * Time.deltaTime);

            if (moveDir != Vector3.zero)
            {
                Vector3 flatLookDir = new Vector3(moveDir.x, 0, moveDir.z);

                if (flatLookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion levelRot = Quaternion.LookRotation(flatLookDir);

                    Quaternion diveRot = Quaternion.LookRotation(moveDir);
                    Vector3 diveAngles = diveRot.eulerAngles;

                    float pitch = diveAngles.x;
                    if (pitch > 180) pitch -= 360; 
                    pitch = Mathf.Clamp(pitch, -5f, _settings.MaxDiveAngle);

                    diveRot = Quaternion.Euler(pitch, diveAngles.y, 0);

                    float heightFactor = Mathf.Clamp01((heightToWater - 2.0f) / (_heightAtLandStart * _settings.LandingRotationFlatttenDistance));

                    Quaternion targetRot = Quaternion.Slerp(levelRot, diveRot, heightFactor);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _settings.TurnSpeed * Time.deltaTime);
                }
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
                bird.MovementVelocity = Vector3.zero;

                bird.Self.Show(FowlState.Swimming);

                Vector2 randomCircle = Random.insideUnitCircle * _settings.SwimSpread;
                bird.GroupOffset = new Vector3(randomCircle.x, 0, randomCircle.y);

                bird.CurrentAnimatedOffset = bird.Transform.localPosition;

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
                Debug.LogWarning("No swimming targets assigned to FlockController!");
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
            float minDetectionSqrRadius = _settings.MinLandingDistance * _settings.MinLandingDistance;

            foreach (Transform target in _swimmingTargets)
            {
                Vector2 flockPosXZ = new Vector2(transform.position.x, transform.position.z);
                Vector2 targetPosXZ = new Vector2(target.position.x, target.position.z);

                float sqrDist = (flockPosXZ - targetPosXZ).sqrMagnitude;

                if (sqrDist < closestSqrDist && sqrDist >= minDetectionSqrRadius)
                {
                    closestSqrDist = sqrDist;
                    closestTarget = target;
                }
            }

            if (closestSqrDist <= detectionSqrRadius)
            {
                Vector3 directionToTarget = (closestTarget.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, directionToTarget);
                if (dot > 0)
                {
                    if (Random.value < (_settings.ChanceToLand * Time.deltaTime))
                    {
                        TransitionToLanding(closestTarget);
                    }
                }
            }
        }

        private void TransitionToLanding(Transform target)
        {
            _currentState = FowlState.Landing;

            _audio.PlayTakeOffSound();

            _heightAtLandStart = transform.position.y;

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
                _lifecycleHook.OnCleanup();
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
                    bird.CurrentAnimatedOffset = bird.Self.transform.localPosition.SetY(0.0f);
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
