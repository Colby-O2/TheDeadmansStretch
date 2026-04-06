using PlazmaGames.Math;
using UnityEngine;

namespace ColbyO.Untitled
{
    public class FollowXZ : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _snap = 0;
        [SerializeField] private bool _rotateY;
        [SerializeField] private bool _followAtHeight;
        [SerializeField] private float _followHeight;

        private void Update()
        {
            Vector2 pos = new Vector2(_target.position.x, _target.position.z);
            if (_snap > 0)
            {
                pos = new Vector2(Mathf.Round(pos.x / _snap) * _snap, Mathf.Round(pos.y / _snap) * _snap);
            }
            transform.position = new Vector3(pos.x, transform.position.y, pos.y);
            if (_rotateY)
            {
                transform.eulerAngles = transform.eulerAngles.SetY(_target.eulerAngles.y);
            }
            if (_followAtHeight)
            {
                transform.position = transform.position.SetY(_target.position.y + _followHeight);
            }
        }
    }
}
