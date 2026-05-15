using CustomMath;
using UnityEngine;
using UnityEngine.UIElements;

public class Frustum
{
    public MyPlane[] planes = new MyPlane[6];


    public Frustum(MyPlane[] planes) { this.planes = planes; }
    public Frustum() { }
    
    private GameObject[] visualPlanes = new GameObject[6];

    private bool visible = false;

    public void Start(Camera cam, GameObject planeObject)
    {
        for (int i = 0; i < 6; i++)
        {
            visualPlanes[i] = CreateVisualPlane(planes[i], new Vec3(cam.transform.position), new Vec3(50.0f, 50.0f, 50.0f), new Color(0.2f, 0.5f, 0.8f, 0.5f), "FrustumPlane", cam.transform.parent, planeObject);
        }
        visible = true;
    }

    public void Update(Camera camera)
    {
        Transform camTransform = camera.transform;
        Vec3 camPos = new Vec3(camera.transform.position);

        // Frustum planes

        /// near
        Vec3 nearNormal = new Vec3(camTransform.forward);
        Vec3 nearPoint = camPos + (nearNormal * camera.nearClipPlane);
        float nearDistance = -Vec3.Dot(nearNormal, nearPoint);
        planes[0] = new MyPlane(nearNormal, nearDistance);

        /// far
        Vec3 farNormal = new Vec3(-camTransform.forward);
        Vec3 farPoint = camPos + (new Vec3(camTransform.forward) * camera.farClipPlane);
        float farDistance = -Vec3.Dot(farNormal, farPoint);
        planes[1] = new MyPlane(farNormal, farDistance);


        float halfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        /// bottom
        Vec3 botSlope = new Vec3(camTransform.forward - (camTransform.up * halfHeight));
        Vec3 botNormal = Vec3.Cross(botSlope, new Vec3(camTransform.right)).normalized;
        float botDistance = -Vec3.Dot(botNormal, new Vec3(camTransform.position));
        planes[2] = new MyPlane(botNormal, botDistance);

        /// top
        Vec3 topSlope = new Vec3(camTransform.forward + (camTransform.up * halfHeight));
        Vec3 topNormal = Vec3.Cross(topSlope, new Vec3(-camTransform.right)).normalized;
        float topDistance = -Vec3.Dot(topNormal, new Vec3(camTransform.position));
        planes[3] = new MyPlane(topNormal, topDistance);


        float halfWidth = halfHeight * camera.aspect;

        /// left
        Vec3 leftSlope = new Vec3(camTransform.forward - (camTransform.right * halfWidth));
        Vec3 leftNormal = Vec3.Cross(leftSlope, new Vec3(-camTransform.up)).normalized;
        float leftDistance = -Vec3.Dot(leftNormal, new Vec3(camTransform.position));
        planes[4] = new MyPlane(leftNormal, leftDistance);

        /// right
        Vec3 rightSlope = new Vec3(camTransform.forward + (camTransform.right * halfWidth));
        Vec3 rightNormal = Vec3.Cross(rightSlope, new Vec3(camTransform.up)).normalized;
        float rightDistance = -Vec3.Dot(rightNormal, new Vec3(camTransform.position));
        planes[5] = new MyPlane(rightNormal, rightDistance);

        if (!visible) return;

        for (int i = 0; i < 6; i++)
        {
            UpdateVisualPlanes(visualPlanes[i], planes[i],camPos);
        }
    }

    public bool IsInside(Vec3 center, float radius = 0f)
    {
        for (int i = 0; i < 6; i++)
        {
            if (planes[i].GetDistanceToPoint(center) < -radius)
            {
                return false;
            }
        }
        return true;
    }

    private GameObject CreateVisualPlane(MyPlane plane, Vec3 pos, Vec3 scale, Color color, string name, Transform parent, GameObject planeObject)
    {
        GameObject planeObj = Object.Instantiate(planeObject, parent);
        planeObj.transform.name = name;

        planeObj.transform.position = pos;
        planeObj.transform.localScale = scale;
        planeObj.transform.rotation = Quaternion.FromToRotation(Vec3.Back, plane.normal);

        MeshRenderer renderer = planeObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = color;
        }

        return planeObj;
    }

    private void UpdateVisualPlanes(GameObject planeObj, MyPlane plane, Vec3 pos)
    {
        planeObj.transform.position = pos;
        planeObj.transform.rotation = Quaternion.FromToRotation(Vec3.Back, plane.normal);
    }
}