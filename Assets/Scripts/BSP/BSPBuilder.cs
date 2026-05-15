using CustomMath;
using System.Collections.Generic;
using UnityEngine;

namespace BSP
{
    public class BSPBuilder
    {
        public BSPNode RootNode { get; private set; }
        public List<Portal> AllPortals { get; private set; } = new List<Portal>();

        private BSPVisualizer visualizer;

        public BSPBuilder(BSPVisualizer visualizer)
        {
            this.visualizer = visualizer;
        }

        public void BuildDummyLevel()
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
            RootNode = new BSPNode(wRightA.plane, nodeLeft, outside);

            Color colorA = new Color(0.7f, 0.3f, 0.3f, 1f);
            Color colorB = new Color(0.3f, 0.7f, 0.3f, 1f);
            Color colorC = new Color(0.3f, 0.3f, 0.7f, 1f);
            Vector2 wallSize = new Vector2(20f, 10f);

            visualizer.BuildVisualWall(wRightA, new Vec3(10, 0, 0), wallSize, colorA, "wRightA");
            visualizer.BuildVisualWall(wLeftA, new Vec3(-10, 0, 0), wallSize, colorA, "wLeftA");
            visualizer.BuildVisualWall(wBackA, new Vec3(0, 0, -10), wallSize, colorA, "wBackA");
            visualizer.BuildVisualWall(wFrontA, new Vec3(0, 0, 10), wallSize, colorA, "wFrontA");

            visualizer.BuildVisualWall(wRightB, new Vec3(10, 0, 20), wallSize, colorB, "wRightB");
            visualizer.BuildVisualWall(wLeftB, new Vec3(-10, 0, 20), wallSize, colorB, "wLeftB");
            visualizer.BuildVisualWall(wBackB, new Vec3(0, 0, 10), wallSize, colorB, "wBackB");
            visualizer.BuildVisualWall(wFrontB, new Vec3(0, 0, 30), wallSize, colorB, "wFrontB");

            visualizer.BuildVisualWall(wRightC, new Vec3(10, 0, 40), wallSize, colorC, "wRightC");
            visualizer.BuildVisualWall(wLeftC, new Vec3(-10, 0, 40), wallSize, colorC, "wLeftC");
            visualizer.BuildVisualWall(wBackC, new Vec3(0, 0, 30), wallSize, colorC, "wBackC");
            visualizer.BuildVisualWall(wFrontC, new Vec3(0, 0, 50), wallSize, colorC, "wFrontC");

            visualizer.SpawnDummyObject(new Vec3(5, 0, 5), Color.red, roomA);
            visualizer.SpawnDummyObject(new Vec3(-5, 0, 5), Color.red, roomA);

            visualizer.SpawnDummyObject(new Vec3(0, -2, 25), Color.green, roomB);
            visualizer.SpawnDummyObject(new Vec3(5, 2, 28), Color.green, roomB);

            visualizer.SpawnDummyObject(new Vec3(0, 0, 45), Color.blue, roomC);
        }

        private void AddPortalData(Wall wall, Vec3 min, Vec3 max, BSPRoom nextRoom)
        {
            Portal p = new Portal();
            p.corners[0] = min;
            p.corners[1] = max;
            p.nextRoom = nextRoom;
            wall.AddPortal(p);
            AllPortals.Add(p);
        }
    }
}