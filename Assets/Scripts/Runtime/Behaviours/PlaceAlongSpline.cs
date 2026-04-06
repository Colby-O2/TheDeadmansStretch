#if UNITY_EDITOR
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace ColbyO.Untitled
{
    [ExecuteInEditMode]
    public class PlaceAlongSpline : MonoBehaviour
    {
            [SerializeField] private SplineContainer _splineContainer;
            [SerializeField] private GameObject _prefab;
            [SerializeField] private GameObject _parnet;
            [SerializeField] private float _spacing = 2.0f;

            [SerializeField, HideInInspector] private System.Collections.Generic.List<GameObject> spawnedObjects = new();

            private void OnEnable() => Spline.Changed += OnSplineChanged;
            private void OnDisable() => Spline.Changed -= OnSplineChanged;

            private void OnValidate()
            {
                if (_splineContainer != null && _prefab != null && gameObject.activeSelf)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (this == null) return;
                        ClearExisting();
                        SpawnObjects();
                    };
                }
            }

            private void OnSplineChanged(Spline _, int __, SplineModification ___)
            {
                ClearExisting();
                SpawnObjects();
            }

            private void ClearExisting()
            {
                Transform parentTarget = (_parnet != null) ? _parnet.transform : transform;
                for (int i = parentTarget.childCount - 1; i >= 0; i--)
                {
                    GameObject child = parentTarget.GetChild(i).gameObject;
                    if (child != null) DestroyImmediate(child);
                }
                spawnedObjects.Clear();
            }

            public void SpawnObjects()
            {
                if (_splineContainer == null || _prefab == null || _spacing <= 0.1f) return;

                foreach (var spline in _splineContainer.Splines)
                {
                    float totalLength = spline.GetLength();
                    int count = Mathf.FloorToInt(totalLength / _spacing);

                    for (int i = 0; i <= count; i++)
                    {
                        float t = (i * _spacing) / totalLength;

                        spline.Evaluate(t, out float3 localPos, out float3 tangent, out float3 up);

                        Vector3 worldPos = _splineContainer.transform.TransformPoint(localPos);
                        Vector3 worldForward = _splineContainer.transform.TransformDirection(tangent);
                        Vector3 worldUp = _splineContainer.transform.TransformDirection(up);

                        GameObject newObj = Instantiate(_prefab, worldPos, Quaternion.LookRotation(worldForward, worldUp));
                        newObj.transform.parent = (_parnet != null) ? _parnet.transform : transform;
                        spawnedObjects.Add(newObj);
                    }
                }
            }
    }
}
#endif