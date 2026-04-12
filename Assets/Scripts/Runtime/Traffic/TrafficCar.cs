using System.Collections.Generic;
using UnityEngine;

namespace ColbyO.Untitled.Traffic
{
    public class TrafficCar : MonoBehaviour
    {
        [SerializeField] private List<CarComponets> _components;

        [SerializeField] private MeshRenderer _driver;
        [SerializeField] private int _hairIdx;
        [SerializeField] private int _coatIdx;

        [SerializeField] private EngineSound _engine;

        private void Awake()
        {
            if (_components != null)
            {
                float hueShift = Random.Range(-1.0f, 1.0f);
                float lightnessShift = Random.Range(0.5f, 2.0f);

                for (int i = 0; i < _components.Count; i++)
                {
                    _components[i].MesnRenderer.materials[_components[i].MaterialIdx].SetFloat("_HueShift", hueShift);
                    _components[i].MesnRenderer.materials[_components[i].MaterialIdx].SetFloat("_LightnessMultiplier", lightnessShift);
                }

                if (_driver != null)
                {
                    // Driver Coat
                    _driver.materials[_coatIdx].SetFloat("_HueShift", Random.Range(-1.0f, 1.0f));
                    _driver.materials[_coatIdx].SetFloat("_LightnessMultiplier", Random.Range(0.5f, 2.0f));

                    // Driver Hair
                    _driver.materials[_hairIdx].SetFloat("_HueShift", Random.Range(0.0f, 0.12f));
                    _driver.materials[_hairIdx].SetFloat("_LightnessMultiplier", Random.Range(0.5f, 2.0f));
                }
            }
        }

        [System.Serializable]
        private struct CarComponets
        {
            public MeshRenderer MesnRenderer;
            public int MaterialIdx;
        }
    }
}