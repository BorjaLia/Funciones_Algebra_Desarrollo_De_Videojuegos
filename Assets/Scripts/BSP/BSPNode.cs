using CustomMath;
using System.Collections.Generic;
using UnityEngine;

namespace BSP
{
    public class BSPNode
    {
        public MyPlane divider;
        public BSPNode front;
        public BSPNode back;

        public BSPNode() { }

        public BSPNode(MyPlane divider, BSPNode front, BSPNode back)
        {
            this.divider = divider;
            this.front = front;
            this.back = back;
        }

        public virtual BSPRoom GetRoom(Vec3 point)
        {
            if (divider.GetSide(point)) return front.GetRoom(point);
            else return back.GetRoom(point);
        }
    }

    public class Portal
    {
        public Vec3[] corners = new Vec3[2];

        public BSPRoom nextRoom;

        public Vec3 rightVec;
        public Vec3 upPlaneVec;
        public float minX, maxX, minY, maxY;

        public void SetupLocalSpace(Vec3 wallNormal)
        {
            Vec3 worldUp = new Vec3(0, 1, 0);

            if (Mathf.Abs(wallNormal.y) > 0.99f)
            {
                worldUp = new Vec3(1, 0, 0);
            }

            rightVec = Vec3.Cross(worldUp, wallNormal).normalized;
            upPlaneVec = Vec3.Cross(wallNormal, rightVec).normalized;

            float c0X = Vec3.Dot(corners[0], rightVec);
            float c0Y = Vec3.Dot(corners[0], upPlaneVec);

            float c1X = Vec3.Dot(corners[1], rightVec);
            float c1Y = Vec3.Dot(corners[1], upPlaneVec);

            minX = Mathf.Min(c0X, c1X);
            maxX = Mathf.Max(c0X, c1X);
            minY = Mathf.Min(c0Y, c1Y);
            maxY = Mathf.Max(c0Y, c1Y);
        }
    }

    public class Wall
    {
        public MyPlane plane;
        public List<Portal> portals;

        public Wall(MyPlane plane)
        {
            this.plane = plane;
            this.portals = new List<Portal>();
        }

        public void AddPortal(Portal portal) { portals.Add(portal); }
    }

    public class BSPRoom : BSPNode
    {
        public List<Wall> walls;
        public List<GameObject> cachedObjects;

        public BSPRoom(List<Wall> walls, List<GameObject> objects)
        {
            this.walls = walls;
            this.cachedObjects = objects;
        }
        public override BSPRoom GetRoom(Vec3 point)
        {
            return this;
        }
    }
}