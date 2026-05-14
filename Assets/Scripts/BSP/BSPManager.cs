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
            UpdateFrustum();


        }

        private void UpdateFrustum()
        {
            Transform camTransform = sceneCamera.transform;

            // Frustum planes

            /// near
            frustum.planes[0] = new MyPlane(new Vec3(camTransform.forward),sceneCamera.nearClipPlane);

            /// far
            frustum.planes[1] = new MyPlane(new Vec3(-camTransform.forward),sceneCamera.farClipPlane);
           

            float halfHeight = Mathf.Tan(sceneCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            /// bottom
            Vec3 botSlope = new Vec3(camTransform.forward - (camTransform.up * halfHeight));
            Vec3 botNormal = Vec3.Cross(botSlope, new Vec3(-camTransform.right)).normalized;
            float botDistance = -Vec3.Dot(botNormal, new Vec3(camTransform.position));
            frustum.planes[2] = new MyPlane(botNormal, botDistance);


            /// top
            Vec3 topSlope = new Vec3(camTransform.forward + (camTransform.up * halfHeight));
            Vec3 topNormal = Vec3.Cross(topSlope, new Vec3(camTransform.right)).normalized;
            float topDistance = -Vec3.Dot(topNormal,new Vec3(camTransform.position));
            frustum.planes[3] = new MyPlane(topNormal, topDistance);


            float halfWidth = halfHeight * sceneCamera.aspect;

            /// left
            Vec3 leftSlope = new Vec3(camTransform.forward - (camTransform.right * halfWidth));
            Vec3 leftNormal = Vec3.Cross(leftSlope, new Vec3(camTransform.up)).normalized;
            float leftDistance = -Vec3.Dot(topNormal, new Vec3(camTransform.position));
            frustum.planes[4] = new MyPlane(leftNormal, leftDistance);

            /// right
            Vec3 rightSlope = new Vec3(camTransform.forward + (camTransform.right * halfWidth));
            Vec3 rightNormal = Vec3.Cross(rightSlope, new Vec3(-camTransform.up)).normalized;
            float rightDistance = -Vec3.Dot(topNormal, new Vec3(camTransform.position));
            frustum.planes[5] = new MyPlane(rightNormal, rightDistance);
        }
    }
}