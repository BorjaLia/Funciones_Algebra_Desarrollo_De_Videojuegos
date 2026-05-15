using CustomMath;
using System.Collections.Generic;
using UnityEngine;

namespace BSP
{
    public class BSPVisualizer
    {
        private GameObject planeObject;
        private Transform environmentParent;
        public List<GameObject> allCachedObjects = new List<GameObject>();

        public BSPVisualizer(GameObject planePrefab, Transform parent)
        {
            this.planeObject = planePrefab;
            this.environmentParent = parent;
        }

        public void SpawnDummyObject(Vec3 pos, Color color, BSPRoom room)
        {
            Transform parent = environmentParent != null ? environmentParent : null;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            obj.transform.position = pos;
            obj.transform.parent = parent;
            obj.GetComponent<Renderer>().material.color = color;

            room.cachedObjects.Add(obj);
            allCachedObjects.Add(obj);
        }

        public void BuildVisualWall(Wall wall, Vec3 center, Vector2 size, Color color, string name)
        {
            float visualOffset = 0.5f;
            center += wall.plane.normal * visualOffset;

            if (wall.portals == null || wall.portals.Count == 0)
            {
                CreateVisualPlane(wall.plane, center, new Vec3(size.x, size.y, 1f), color, name);
                return;
            }

            Portal p = wall.portals[0];

            Quaternion rotation = Quaternion.FromToRotation(Vec3.Back, wall.plane.normal);

            Vec3 localMin = new Vec3(Quaternion.Inverse(rotation) * (p.corners[0] - center));
            Vec3 localMax = new Vec3(Quaternion.Inverse(rotation) * (p.corners[1] - center));

            float pMinX = Mathf.Min(localMin.x, localMax.x);
            float pMaxX = Mathf.Max(localMin.x, localMax.x);
            float pMinY = Mathf.Min(localMin.y, localMax.y);
            float pMaxY = Mathf.Max(localMin.y, localMax.y);

            float wMinX = -size.x / 2f;
            float wMaxX = size.x / 2f;
            float wMinY = -size.y / 2f;
            float wMaxY = size.y / 2f;

            if (wMaxY > pMaxY)
            {
                float h = wMaxY - pMaxY;
                Vec3 localCenter = new Vec3(0, pMaxY + h / 2f, 0);
                CreateVisualPlane(wall.plane, new Vec3(center + rotation * localCenter), new Vec3(size.x, h, 1f), color, name + "_Top");
            }

            if (pMinY > wMinY)
            {
                float h = pMinY - wMinY;
                Vec3 localCenter = new Vec3(0, wMinY + h / 2f, 0);
                CreateVisualPlane(wall.plane, new Vec3(center + rotation * localCenter), new Vec3(size.x, h, 1f), color, name + "_Bottom");
            }

            if (pMinX > wMinX)
            {
                float w = pMinX - wMinX;
                float h = pMaxY - pMinY;
                Vec3 localCenter = new Vec3(wMinX + w / 2f, (pMinY + pMaxY) / 2f, 0);
                CreateVisualPlane(wall.plane, new Vec3(center + rotation * localCenter), new Vec3(w, h, 1f), color, name + "_Left");
            }

            if (wMaxX > pMaxX)
            {
                float w = wMaxX - pMaxX;
                float h = pMaxY - pMinY;
                Vec3 localCenter = new Vec3(pMaxX + w / 2f, (pMinY + pMaxY) / 2f, 0);
                CreateVisualPlane(wall.plane, new Vec3(center + rotation * localCenter), new Vec3(w, h, 1f), color, name + "_Right");
            }

            Vec3 pCenter = new Vec3(center + rotation * new Vec3((pMinX + pMaxX) / 2f, (pMinY + pMaxY) / 2f, 0));
            Vec3 pSize = new Vec3(pMaxX - pMinX, pMaxY - pMinY, 1f);

            GameObject portalObj = CreateVisualPlane(wall.plane, pCenter, pSize, new Color(0.2f, 0.6f, 1f, 0.3f), name + "_PortalGlass");
            portalObj.transform.position += (Vector3)(-wall.plane.normal * 0.05f);
        }

        private GameObject CreateVisualPlane(MyPlane plane, Vec3 pos, Vec3 scale, Color color, string name)
        {
            Transform parent = environmentParent != null ? environmentParent : null;
            GameObject planeObj = Object.Instantiate(planeObject, parent);
            planeObj.transform.name = name;

            planeObj.transform.position = pos;
            planeObj.transform.localScale = scale;
            planeObj.transform.rotation = Quaternion.FromToRotation(Vector3.back, (Vector3)plane.normal);

            MeshRenderer renderer = planeObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = color;
            }

            return planeObj;
        }

        public void DrawPortalOutlines(List<Portal> allPortals)
        {
            foreach (var p in allPortals)
            {
                Vec3 c1 = p.corners[0]; // Min
                Vec3 c2 = p.corners[1]; // Max

                bool isSideWall = Mathf.Abs(c1.x - c2.x) < 0.01f;

                Vec3 bottomLeft, bottomRight, topLeft, topRight;

                if (isSideWall)
                {
                    bottomLeft = new Vec3(c1.x, c1.y, c1.z);
                    bottomRight = new Vec3(c1.x, c1.y, c2.z);
                    topLeft = new Vec3(c1.x, c2.y, c1.z);
                    topRight = new Vec3(c1.x, c2.y, c2.z);
                }
                else
                {
                    bottomLeft = new Vec3(c1.x, c1.y, c1.z);
                    bottomRight = new Vec3(c2.x, c1.y, c1.z);
                    topLeft = new Vec3(c1.x, c2.y, c1.z);
                    topRight = new Vec3(c2.x, c2.y, c1.z);
                }

                Debug.DrawLine(bottomLeft, bottomRight, Color.yellow);
                Debug.DrawLine(bottomRight, topRight, Color.yellow);
                Debug.DrawLine(topRight, topLeft, Color.yellow);
                Debug.DrawLine(topLeft, bottomLeft, Color.yellow);
            }
        }
    }
}