using CustomMath;
using UnityEngine;

public class Frustum
{
    public MyPlane[] planes = new MyPlane[6];

    public Frustum(MyPlane[] planes) { this.planes = planes; }

    public void Update(Camera camera)
    {
        Transform camTransform = camera.transform;

        // Frustum planes

        /// near
        planes[0] = new MyPlane(new Vec3(camTransform.forward), camera.nearClipPlane);

        /// far
        planes[1] = new MyPlane(new Vec3(-camTransform.forward), camera.farClipPlane);


        float halfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        /// bottom
        Vec3 botSlope = new Vec3(camTransform.forward - (camTransform.up * halfHeight));
        Vec3 botNormal = Vec3.Cross(botSlope, new Vec3(-camTransform.right)).normalized;
        float botDistance = -Vec3.Dot(botNormal, new Vec3(camTransform.position));
        planes[2] = new MyPlane(botNormal, botDistance);


        /// top
        Vec3 topSlope = new Vec3(camTransform.forward + (camTransform.up * halfHeight));
        Vec3 topNormal = Vec3.Cross(topSlope, new Vec3(camTransform.right)).normalized;
        float topDistance = -Vec3.Dot(topNormal, new Vec3(camTransform.position));
        planes[3] = new MyPlane(topNormal, topDistance);


        float halfWidth = halfHeight * camera.aspect;

        /// left
        Vec3 leftSlope = new Vec3(camTransform.forward - (camTransform.right * halfWidth));
        Vec3 leftNormal = Vec3.Cross(leftSlope, new Vec3(camTransform.up)).normalized;
        float leftDistance = -Vec3.Dot(topNormal, new Vec3(camTransform.position));
        planes[4] = new MyPlane(leftNormal, leftDistance);

        /// right
        Vec3 rightSlope = new Vec3(camTransform.forward + (camTransform.right * halfWidth));
        Vec3 rightNormal = Vec3.Cross(rightSlope, new Vec3(-camTransform.up)).normalized;
        float rightDistance = -Vec3.Dot(topNormal, new Vec3(camTransform.position));
        planes[5] = new MyPlane(rightNormal, rightDistance);
    }
}