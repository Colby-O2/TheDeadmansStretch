using ColbyO.Untitled.Wildlife;
using UnityEngine;

namespace ColbyO.Untitled
{
    public class Fowl : MonoBehaviour
    {
        [SerializeField] private GameObject _swimmingMesh;
        [SerializeField] private GameObject _flyingMesh;

        public void Show(FowlState state)
        {
            if (state == FowlState.Flying || state == FowlState.Takeoff || state == FowlState.Landing)
            {
                if (!_flyingMesh.activeSelf) _flyingMesh.SetActive(true);
                if (_swimmingMesh.activeSelf) _swimmingMesh.SetActive(false);
            }
            else if (state == FowlState.Swimming)
            {
                if (!_swimmingMesh.activeSelf) _swimmingMesh.SetActive(true);
                if (_flyingMesh.activeSelf) _flyingMesh.SetActive(false);
            }
            else
            {
                if (_flyingMesh.activeSelf) _flyingMesh.SetActive(false);
                if (_swimmingMesh.activeSelf) _swimmingMesh.SetActive(false);
            }
        }
    }
}
