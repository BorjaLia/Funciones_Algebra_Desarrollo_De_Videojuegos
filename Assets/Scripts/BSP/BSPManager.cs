using CustomMath;
using System.Collections.Generic;
using UnityEngine;

namespace BSP
{
    public class BSPManager : MonoBehaviour
    {
        [Header("Camera Settings")]

        [SerializeField] public Camera sceneCamera;

        [SerializeField] public float raysPerDegreeX = 0.1f;
        [SerializeField] public float raysPerDegreeY = 0.1f;

        [Header("Visual")]
        [SerializeField] public GameObject planeObject;
        [SerializeField] public Transform environmentParent;

        private BSPNode bspRoot;
        private Frustum frustum = new Frustum();

        public HashSet<BSPRoom> visibleRooms = new HashSet<BSPRoom>();

        private int lastVisibleRoomsCount = -1;

        private List<Portal> allPortals = new List<Portal>();

        private List<GameObject> allCachedObjects = new List<GameObject>();

        private void Start()
        {
            CreateDummyBSP();
            //frustum.Start(sceneCamera,planeObject);
        }

        private void Update()
        {
            if (sceneCamera == null || bspRoot == null) return;

            frustum.Update(sceneCamera);

            visibleRooms.Clear();

            BSPRoom currentRoom = bspRoot.GetRoom(new Vec3(sceneCamera.transform.position));
            if (currentRoom != null)
            {
                visibleRooms.Add(currentRoom);
            }

            ProcessFrustumRayGrid();
            DrawPortalOutlines();

            ApplyFrustumCulling();

            if (visibleRooms.Count != lastVisibleRoomsCount)
            {
                Debug.Log($"Visible Rooms: {visibleRooms.Count}");
                lastVisibleRoomsCount = visibleRooms.Count;
            }
        }

        private void ApplyFrustumCulling()
        {
            foreach (GameObject obj in allCachedObjects)
            {
                if (obj != null) obj.SetActive(false);
            }

            foreach (BSPRoom room in visibleRooms)
            {
                foreach (GameObject obj in room.cachedObjects)
                {
                    if (obj == null) continue;

                    if (frustum.IsInside(new Vec3(obj.transform.position), 0.5f))
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }

        private void ProcessFrustumRayGrid()
        {
            float fovY = sceneCamera.fieldOfView;
            float fovX = Camera.VerticalToHorizontalFieldOfView(fovY, sceneCamera.aspect);

            int raysX = Mathf.CeilToInt(fovX * raysPerDegreeX);
            int raysY = Mathf.CeilToInt(fovY * raysPerDegreeY);

            float halfHeight = Mathf.Tan(fovY * 0.5f * Mathf.Deg2Rad) * sceneCamera.nearClipPlane;
            float halfWidth = halfHeight * sceneCamera.aspect;

            Vec3 camPos = new Vec3(sceneCamera.transform.position);
            Vector3 forward = sceneCamera.transform.forward;
            Vector3 right = sceneCamera.transform.right;
            Vector3 up = sceneCamera.transform.up;

            for (int x = 0; x <= raysX; x++)
            {
                for (int y = 0; y <= raysY; y++)
                {
                    float u = (raysX == 0) ? 0 : ((float)x / raysX) * 2f - 1f;
                    float v = (raysY == 0) ? 0 : ((float)y / raysY) * 2f - 1f;

                    Vector3 pointOnNearPlane = sceneCamera.transform.position +
                                               (forward * sceneCamera.nearClipPlane) +
                                               (right * (u * halfWidth)) +
                                               (up * (v * halfHeight));

                    Vec3 rayDir = new Vec3(pointOnNearPlane - sceneCamera.transform.position).normalized;

                    CastRayAndFindRooms(camPos, rayDir);
                }
            }
        }

        private void CastRayAndFindRooms(Vec3 origin, Vec3 direction)
        {
            MyPlane farPlane = frustum.planes[1];

            float denom = Vec3.Dot(farPlane.normal, direction);
            if (Mathf.Abs(denom) < 0.0001f) return;

            float tFar = -(Vec3.Dot(farPlane.normal, origin) + farPlane.distance) / denom;
            Vec3 endPoint = origin + (direction * tFar);

            Vec3 blockPoint = endPoint;
            bool hitPortal = false;

            bool reachedEnd = FindRoomsInSegment(origin, endPoint, ref blockPoint, ref hitPortal);

            Color rayColor = reachedEnd ? Color.green : (hitPortal ? Color.blue : Color.red);

            Debug.DrawLine((Vector3)origin, (Vector3)blockPoint, rayColor);
        }

        private bool FindRoomsInSegment(Vec3 start, Vec3 end, ref Vec3 outBlockPoint, ref bool hitPortal)
        {
            BSPRoom startRoom = bspRoot.GetRoom(start);
            BSPRoom endRoom = bspRoot.GetRoom(end);

            if (startRoom == endRoom)
            {
                if (startRoom != null) visibleRooms.Add(startRoom);
                return true;
            }

            if (Vec3.Distance(start, end) < 0.01f)
            {
                if (startRoom != null) visibleRooms.Add(startRoom);

                bool passedPortal = false;

                if (startRoom != null)
                {
                    foreach (Wall w in startRoom.walls)
                    {
                        float distToPlane = Mathf.Abs(Vec3.Dot(w.plane.normal, start) + w.plane.distance);
                        if (distToPlane < 0.05f)
                        {
                            foreach (Portal p in w.portals)
                            {
                                float minX = Mathf.Min(p.corners[0].x, p.corners[1].x);
                                float maxX = Mathf.Max(p.corners[0].x, p.corners[1].x);
                                float minY = Mathf.Min(p.corners[0].y, p.corners[1].y);
                                float maxY = Mathf.Max(p.corners[0].y, p.corners[1].y);

                                if (start.x >= minX && start.x <= maxX &&
                                    start.y >= minY && start.y <= maxY)
                                {
                                    passedPortal = true;
                                    hitPortal = true;

                                    if (p.nextRoom != null) visibleRooms.Add(p.nextRoom);

                                    break;
                                }
                            }
                        }
                        if (passedPortal) break;
                    }
                }

                if (!passedPortal)
                {
                    outBlockPoint = start;
                }

                return passedPortal;
            }

            Vec3 midPoint = Vec3.Lerp(start, end, 0.5f);

            bool canContinue = FindRoomsInSegment(start, midPoint, ref outBlockPoint, ref hitPortal);

            if (canContinue)
            {
                return FindRoomsInSegment(midPoint, end, ref outBlockPoint, ref hitPortal);
            }

            return false;
        }

        private void CreateDummyBSP()
        {
            BSPRoom roomA = new BSPRoom(new List<Wall>(), new List<GameObject>());
            BSPRoom roomB = new BSPRoom(new List<Wall>(), new List<GameObject>());
            BSPRoom roomC = new BSPRoom(new List<Wall>(), new List<GameObject>());
            BSPRoom outside = new BSPRoom(new List<Wall>(), new List<GameObject>());

            Wall wRightA = new Wall(new MyPlane(new Vec3(-1, 0, 0), 10));
            Wall wLeftA = new Wall(new MyPlane(new Vec3(1, 0, 0), 10));
            Wall wBackA = new Wall(new MyPlane(new Vec3(0, 0, 1), 10));
            Wall wFrontA = new Wall(new MyPlane(new Vec3(0, 0, -1), 10));
            roomA.walls.AddRange(new List<Wall> { wRightA, wLeftA, wBackA, wFrontA });

            Wall wRightB = new Wall(new MyPlane(new Vec3(-1, 0, 0), 10));
            Wall wLeftB = new Wall(new MyPlane(new Vec3(1, 0, 0), 10));
            Wall wBackB = new Wall(new MyPlane(new Vec3(0, 0, 1), -10));
            Wall wFrontB = new Wall(new MyPlane(new Vec3(0, 0, -1), 30));
            roomB.walls.AddRange(new List<Wall> { wRightB, wLeftB, wBackB, wFrontB });

            Wall wRightC = new Wall(new MyPlane(new Vec3(-1, 0, 0), 10));
            Wall wLeftC = new Wall(new MyPlane(new Vec3(1, 0, 0), 10));
            Wall wBackC = new Wall(new MyPlane(new Vec3(0, 0, 1), -30));
            Wall wFrontC = new Wall(new MyPlane(new Vec3(0, 0, -1), 50));
            roomC.walls.AddRange(new List<Wall> { wRightC, wLeftC, wBackC, wFrontC });

            AddPortalData(wFrontA, new Vec3(-3, -4, 10), new Vec3(3, 4, 10), roomB);
            AddPortalData(wBackB, new Vec3(-3, -4, 10), new Vec3(3, 4, 10), roomA);

            AddPortalData(wFrontB, new Vec3(-3, -4, 30), new Vec3(3, 4, 30), roomC);
            AddPortalData(wBackC, new Vec3(-3, -4, 30), new Vec3(3, 4, 30), roomB);

            BSPNode nodeFront = new BSPNode(wFrontC.plane, roomC, outside);
            BSPNode nodeDiv2 = new BSPNode(wFrontB.plane, roomB, nodeFront);
            BSPNode nodeDiv1 = new BSPNode(wFrontA.plane, roomA, nodeDiv2);
            BSPNode nodeBack = new BSPNode(wBackA.plane, nodeDiv1, outside);
            BSPNode nodeLeft = new BSPNode(wLeftA.plane, nodeBack, outside);
            bspRoot = new BSPNode(wRightA.plane, nodeLeft, outside);


            Color colorA = new Color(0.7f, 0.3f, 0.3f, 1f);
            Color colorB = new Color(0.3f, 0.7f, 0.3f, 1f);
            Color colorC = new Color(0.3f, 0.3f, 0.7f, 1f);

            Vector2 wallSize = new Vector2(20f, 10f);

            BuildVisualWall(wRightA, new Vec3(10, 0, 0), wallSize, colorA, "wRightA");
            BuildVisualWall(wLeftA, new Vec3(-10, 0, 0), wallSize, colorA, "wLeftA");
            BuildVisualWall(wBackA, new Vec3(0, 0, -10), wallSize, colorA, "wBackA");
            BuildVisualWall(wFrontA, new Vec3(0, 0, 10), wallSize, colorA, "wFrontA");

            BuildVisualWall(wRightB, new Vec3(10, 0, 20), wallSize, colorB, "wRightB");
            BuildVisualWall(wLeftB, new Vec3(-10, 0, 20), wallSize, colorB, "wLeftB");
            BuildVisualWall(wBackB, new Vec3(0, 0, 10), wallSize, colorB, "wBackB");
            BuildVisualWall(wFrontB, new Vec3(0, 0, 30), wallSize, colorB, "wFrontB");

            BuildVisualWall(wRightC, new Vec3(10, 0, 40), wallSize, colorC, "wRightC");
            BuildVisualWall(wLeftC, new Vec3(-10, 0, 40), wallSize, colorC, "wLeftC");
            BuildVisualWall(wBackC, new Vec3(0, 0, 30), wallSize, colorC, "wBackC");
            BuildVisualWall(wFrontC, new Vec3(0, 0, 50), wallSize, colorC, "wFrontC");


            SpawnDummyObject(new Vec3(5, 0, 5), Color.red, roomA);
            SpawnDummyObject(new Vec3(-5, 0, 5), Color.red, roomA);

            SpawnDummyObject(new Vec3(0, -2, 25), Color.green, roomB);
            SpawnDummyObject(new Vec3(5, 2, 28), Color.green, roomB);

            SpawnDummyObject(new Vec3(0, 0, 45), Color.blue, roomC);
        }

        private void SpawnDummyObject(Vec3 pos, Color color, BSPRoom room)
        {
            Transform parent = environmentParent != null ? environmentParent : this.transform;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            obj.transform.position = pos;
            obj.transform.parent = parent;
            obj.GetComponent<Renderer>().material.color = color;

            room.cachedObjects.Add(obj);
            allCachedObjects.Add(obj);
        }

        private void AddPortalData(Wall wall, Vec3 min, Vec3 max, BSPRoom nextRoom)
        {
            Portal p = new Portal();
            p.corners[0] = min;
            p.corners[1] = max;
            p.nextRoom = nextRoom;
            wall.AddPortal(p);
            allPortals.Add(p);
        }

        private void BuildVisualWall(Wall wall, Vec3 center, Vector2 size, Color color, string name)
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
            Transform parent = environmentParent != null ? environmentParent : this.transform;
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

        private void DrawPortalOutlines()
        {
            foreach (var p in allPortals)
            {
                Vec3 c1 = p.corners[0]; // Min
                Vec3 c2 = p.corners[1]; // Max

                Vec3 bottomLeft = new Vec3(c1.x, c1.y, c1.z);
                Vec3 bottomRight = new Vec3(c2.x, c1.y, c1.z);
                Vec3 topLeft = new Vec3(c1.x, c2.y, c2.z);
                Vec3 topRight = new Vec3(c2.x, c2.y, c2.z);

                Debug.DrawLine(bottomLeft, bottomRight, Color.yellow);
                Debug.DrawLine(bottomRight, topRight, Color.yellow);
                Debug.DrawLine(topRight, topLeft, Color.yellow);
                Debug.DrawLine(topLeft, bottomLeft, Color.yellow);
            }
        }
    }
}