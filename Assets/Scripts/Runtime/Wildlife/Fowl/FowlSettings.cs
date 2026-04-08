using ColbyO.Untitled.Wildlife;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyO.Untitled
{
    [CreateAssetMenu(fileName = "FowlSettings", menuName = "Wildlife/Fowl Settings")]
    public class FowlSettings : ScriptableObject
    {
        [Header("General Settings")]
        public string SpeciesName;
        public List<Fowl> FowlPrefabs;
        public Vector2Int FlockSize = new Vector2Int(6, 17);
        public float MoveSpeed = 15f;
        public float TurnSpeed = 5f;

        [Header("Flying Settngs")]
        public float FlyingSpeedMul = 3.0f;
        public float VSpacing = 2.0f;
        public float AirDriftAmount = 0.4f;
        public float AirDriftSpeed = 1.5f;

        [Header("Swimming Settings")]
        public float SwimmingSpeedMul = 1.0f;
        public float HeigtOffset;
        public Vector2 WaitAtWaypointTime = new Vector2(10.0f, 25.0f);
        public float SwimSpread = 3.0f;
        public float IdleRadius = 4.0f;
        public Vector2 IdleWait = new Vector2(2.0f, 7.0f);

        [Header("Landing Settings")]
        [Range(0, 1)] public float ChanceToLand = 0.05f;
        public float LandingDetectionRadius = 20f;
        public float MinLandingDistance  = 2f;
        [Range(0, 1)] public float LandingSlowdownDistance = 1f;
        [Range(0, 1)] public float LandingRotationFlatttenDistance = 1f;
        public float MaxDiveAngle = 15.0f;

        [Header("Takeoff Settings")]
        [Range(0, 1)] public float ChanceToTakeoff = 0.05f;
        public float TakeoffLevelingZone = 5.0f;
    }
}
