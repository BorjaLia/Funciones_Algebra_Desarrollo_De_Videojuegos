using CustomMath;
using UnityEngine;

namespace BSP
{
    public class BSPManager : MonoBehaviour
    {
        [Header("Camera Settings")]

        [SerializeField] public Camera sceneCamera;

        [SerializeField] public float raysPerDegreeX = 0.1f;
        [SerializeField] public float raysPerDegreeY = 0.1f;


        [Header("BSP Data")]
        private BSPNode bspRoot;

        Frustum frustum;

        private void Update()
        {
            frustum.Update(sceneCamera);
        }
    }
}