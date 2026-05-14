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

        // TODO: room reference
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