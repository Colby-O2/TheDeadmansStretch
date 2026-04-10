using UnityEngine;

namespace ColbyO.Untitled.Lighting
{
    [ExecuteAlways, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Light))]
    public class VolumetricSpotLight : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Light _light;

        [Header("Appearance")]
        [SerializeField, Range(0f, 1f)] private float _opacity = 0.3f;
        [SerializeField, Range(4, 128)] private int _segments = 24;
        [SerializeField, Range(0f, 1f)] private float _startFadeDistance = 0.5f;
        [SerializeField, Range(0.1f, 10f)] private float fadeSharpness = 1.5f;

        [Header("Optimization")]
        [SerializeField] private bool _needToUpdate = false;
        [SerializeField] private bool _setUpdateLimit = false;
        [SerializeField] private float _updateRate = 0.1f;

        private float _timer;
        private Mesh _mesh;

        private Vector3 _lastPos;
        private Quaternion _lastRot;
        private float _lastRange, _lastAngle, _lastOpacity, _lastFade, _lastSharp;
        private int _lastSegments;

        private void OnEnable()
        {
            Setup();
            GenerateMesh();
        }

        private void Update()
        {
            if (!_needToUpdate) return;

            if (_light.type != LightType.Spot) return;

            if (_setUpdateLimit)
            {
                _timer += Time.deltaTime;
                if (_timer < _updateRate) return;
                _timer = 0;
            }

            if (NeedsUpdate())
            {
                GenerateMesh();
                CacheState();
            }
        }

        private void Setup()
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            if (!_light) _light = GetComponent<Light>();

            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        private bool NeedsUpdate()
        {
            return transform.position != _lastPos ||
                   transform.rotation != _lastRot ||
                   _light.range != _lastRange ||
                   _light.spotAngle != _lastAngle ||
                   _opacity != _lastOpacity ||
                   _segments != _lastSegments ||
                   _startFadeDistance != _lastFade ||
                   fadeSharpness != _lastSharp;
        }

        private void CacheState()
        {
            _lastPos = transform.position;
            _lastRot = transform.rotation;
            _lastRange = _light.range;
            _lastAngle = _light.spotAngle;
            _lastOpacity = _opacity;
            _lastSegments = _segments;
            _lastFade = _startFadeDistance;
            _lastSharp = fadeSharpness;
        }

        private void GenerateMesh()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = "VolumetricSpotlightMesh";
            }
            _mesh.Clear();

            float angleRad = _light.spotAngle * 0.5f * Mathf.Deg2Rad;
            float radius = Mathf.Tan(angleRad) * _light.range;

            Vector3[] vertices = new Vector3[_segments + 2];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[_segments * 3];

            vertices[0] = Vector3.zero;
            colors[0] = new Color(_light.color.r, _light.color.g, _light.color.b, _opacity);

            float fadeStartDist = _startFadeDistance * _light.range;

            for (int i = 0; i <= _segments; i++)
            {
                float frac = (float)i / _segments;
                float theta = frac * Mathf.PI * 2;

                Vector3 pos = new Vector3(Mathf.Cos(theta) * radius, Mathf.Sin(theta) * radius, _light.range);
                vertices[i + 1] = pos;

                float t = Mathf.InverseLerp(fadeStartDist, _light.range, _light.range);
                float fade = 1f - Mathf.Pow(t, fadeSharpness);

                colors[i + 1] = new Color(_light.color.r, _light.color.g, _light.color.b, fade * _opacity);
            }

            for (int i = 0; i < _segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }

            _mesh.vertices = vertices;
            _mesh.colors = colors;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _meshFilter.sharedMesh = _mesh;
        }
    }
}