using UnityEngine;

namespace ColbyO.Untitled.Wildlife
{
    [System.Serializable]
    public class FowlMember
    {
        public Transform Transform;
        public Fowl Self;
        public Vector3 GroupOffset;
        public Vector3 NoiseSeed;
        public float SpeedMultiplier;
        public Vector3 CurrentAnimatedOffset;

        public Vector3 IdleLocalTarget;
        public float NextIdleChangeTime;
        public float SwimPhaseShift;
    }
}
