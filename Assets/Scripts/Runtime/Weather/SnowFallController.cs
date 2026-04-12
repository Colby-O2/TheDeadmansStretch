using UnityEngine;

using ColbyO.Untitled.Player;

namespace ColbyO.Untitled
{
    public class SnowFallController : MonoBehaviour
    {
        [SerializeField] private VelocityTracker _target;
        [SerializeField] private float _leadFactor = 2.0f;

        private ParticleSystem _system;
        private ParticleSystem.VelocityOverLifetimeModule _velLifetime;

        private ParticleSystem.ShapeModule _shape;

        private float _vlXMin;
        private float _vlXMax;
        private float _vlZMin;
        private float _vlZMax;

        public void SetTarget(VelocityTracker target)
        {
            _target = target;
        }

        private void Awake()
        {
            _system = GetComponent<ParticleSystem>();
            _shape = _system.shape;
            _velLifetime = _system.velocityOverLifetime;
            _velLifetime.enabled = true;
            _vlXMin = _velLifetime.x.constantMin;
            _vlXMax = _velLifetime.x.constantMax;
            _vlZMin = _velLifetime.z.constantMin;
            _vlZMax = _velLifetime.z.constantMax;
        }

        private void Update()
        {
            Vector3 d = _target.Velocity;
            _velLifetime.x = new ParticleSystem.MinMaxCurve(_vlXMin - d.x, _vlXMax - d.x);
            _velLifetime.z = new ParticleSystem.MinMaxCurve(_vlZMin - d.z, _vlZMax - d.z);


            _shape.position = new Vector3(d.x * _leadFactor, _shape.position.y, d.z * _leadFactor);
        }
    }
}
