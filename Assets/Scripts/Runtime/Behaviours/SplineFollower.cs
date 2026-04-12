using PlazmaGames.Core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace ColbyO.Untitled
{
    public class SplineFollower : MonoBehaviour
    {
        public float _speed = 10f;
        private SplineContainer _targetSpline;
        private float _distanceTraveled = 0f;
        private int _splineIndex;
        private float _splineLength;

        private Promise _promise;

        private Promise _promiseHalfway;

        public Promise Initialize(SplineContainer spline, int index, float moveSpeed)
        {
            _targetSpline = spline;
            _speed = moveSpeed;
            _splineIndex = index;

            _splineLength = _targetSpline.CalculateLength();

            _distanceTraveled = 0;

            Promise.CreateExisting(ref _promise);

            return _promise;
        }

        public Promise WaitHalf()
        {
            return Promise.CreateExisting(ref _promiseHalfway);
        }

        private void Update()
        {
            if (_targetSpline == null) return;

            _distanceTraveled += _speed * Time.deltaTime;
            float t = _distanceTraveled / _splineLength;

            _targetSpline.Evaluate(_splineIndex, t, out float3 position, out float3 tangent, out float3 upVector);

            transform.position = position;

            if (!tangent.Equals(float3.zero))
            {
                transform.rotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)upVector);
            }

            if (_promiseHalfway != null && t >= 0.5f)
            {
                Promise.ResolveExisting(ref _promiseHalfway);
                _promiseHalfway = null;
            }

            if (t >= 1f)
            {
                Promise.ResolveExisting(ref _promise);
                //Destroy(gameObject);
            }
        }
    }
}