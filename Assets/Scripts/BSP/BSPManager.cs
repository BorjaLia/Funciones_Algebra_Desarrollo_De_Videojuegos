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

        private BSPBuilder builder;
        private BSPVisualizer visualizer;
        private BSPNode bspRoot;
        private Frustum frustum = new Frustum();

        public HashSet<BSPRoom> visibleRooms = new HashSet<BSPRoom>();
        private int lastVisibleRoomsCount = -1;

        private void Start()
        {
            visualizer = new BSPVisualizer(planeObject, environmentParent);
            builder = new BSPBuilder(visualizer);

            builder.BuildDummyLevel();
            bspRoot = builder.RootNode;

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
            visualizer.DrawPortalOutlines(builder.AllPortals);

            ApplyFrustumCulling();

            if (visibleRooms.Count != lastVisibleRoomsCount)
            {
                Debug.Log($"Visible Rooms: {visibleRooms.Count}");
                lastVisibleRoomsCount = visibleRooms.Count;
            }
        }

        private void ApplyFrustumCulling()
        {
            foreach (GameObject obj in visualizer.allCachedObjects)
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
            // If both the start and end are in the same room, early exit
            BSPRoom startRoom = bspRoot.GetRoom(start);
            BSPRoom endRoom = bspRoot.GetRoom(end);

            if (startRoom == endRoom)
            {
                if (startRoom != null) visibleRooms.Add(startRoom);
                return true;
            }

            // If the ray is very short, we check if we hit a portal or a wall
            if (Vec3.Distance(start, end) < 0.01f)
            {
                if (startRoom != null) visibleRooms.Add(startRoom);
                bool passedPortal = false;

                if (startRoom != null)
                {
                    //we check if were hitting a wall
                    foreach (Wall w in startRoom.walls)
                    {
                        float distToPlane = Mathf.Abs(Vec3.Dot(w.plane.normal, start) + w.plane.distance);
                        // if we are hitting a wall, we check if were hitting a portal
                        if (distToPlane < 0.05f)
                        {
                            foreach (Portal p in w.portals)
                            {
                                Vec3 normal = w.plane.normal;
                                Vec3 worldUp = new Vec3(0, 1, 0);

                                if (Mathf.Abs(normal.y) > 0.99f)
                                {
                                    worldUp = new Vec3(1, 0, 0);
                                }

                                Vec3 rightVec = Vec3.Cross(worldUp, normal).normalized;
                                Vec3 upPlaneVec = Vec3.Cross(normal, rightVec).normalized;

                                float localStartX = Vec3.Dot(start, rightVec);
                                float localStartY = Vec3.Dot(start, upPlaneVec);

                                float c0X = Vec3.Dot(p.corners[0], rightVec);
                                float c0Y = Vec3.Dot(p.corners[0], upPlaneVec);

                                float c1X = Vec3.Dot(p.corners[1], rightVec);
                                float c1Y = Vec3.Dot(p.corners[1], upPlaneVec);

                                float minX = Mathf.Min(c0X, c1X);
                                float maxX = Mathf.Max(c0X, c1X);
                                float minY = Mathf.Min(c0Y, c1Y);
                                float maxY = Mathf.Max(c0Y, c1Y);

                                // If the ray hits inside the bounds of the portal, we check passedPortal
                                if (localStartX >= minX - 0.05f && localStartX <= maxX + 0.05f &&
                                    localStartY >= minY - 0.05f && localStartY <= maxY + 0.05f)
                                {
                                    passedPortal = true;
                                    hitPortal = true;

                                    if (p.nextRoom != null) visibleRooms.Add(p.nextRoom);

                                    break;
                                }
                            }
                        }
                        // If we did hit a portal, we stop checking
                        if (passedPortal) break;
                    }
                }

                if (!passedPortal) outBlockPoint = start;
                return passedPortal;
            }

            // Find the room in the mid point
            Vec3 midPoint = Vec3.Lerp(start, end, 0.5f);
            bool canContinue = FindRoomsInSegment(start, midPoint, ref outBlockPoint, ref hitPortal);

            // If start to midPoint is true (they connect) we check the following half
            if (canContinue)
            {
                return FindRoomsInSegment(midPoint, end, ref outBlockPoint, ref hitPortal);
            }

            return false;
        }
    }
}