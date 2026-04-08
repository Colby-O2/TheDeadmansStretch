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
                _flyingMesh.SetActive(true);
                _swimmingMesh.SetActive(false);
            }
            else if (state == FowlState.Swimming)
            {
                _swimmingMesh.SetActive(true);
                _flyingMesh.SetActive(false);
            }
            else
            {
                _flyingMesh.SetActive(false);
                _swimmingMesh.SetActive(false);
            }
        }
    }
}
