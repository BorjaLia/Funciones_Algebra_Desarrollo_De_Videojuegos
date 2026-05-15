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
            
            // left rooms data
            BSPRoom roomD = new BSPRoom(new List<Wall>(), new List<GameObject>());
            BSPRoom roomE = new BSPRoom(new List<Wall>(), new List<GameObject>());
            BSPRoom roomF = new BSPRoom(new List<Wall>(), new List<GameObject>());
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

            // right rooms data
            Wall wRightD = new Wall(new MyPlane(new Vec3(-1, 0, 0), 30));
            Wall wLeftD = new Wall(new MyPlane(new Vec3(1, 0, 0), -10));
            Wall wBackD = new Wall(new MyPlane(new Vec3(0, 0, 1), 10));
            Wall wFrontD = new Wall(new MyPlane(new Vec3(0, 0, -1), 10));
            roomD.walls.AddRange(new List<Wall> { wRightD, wLeftD, wBackD, wFrontD });

            Wall wRightE = new Wall(new MyPlane(new Vec3(-1, 0, 0), 30));
            Wall wLeftE = new Wall(new MyPlane(new Vec3(1, 0, 0), -10));
            Wall wBackE = new Wall(new MyPlane(new Vec3(0, 0, 1), -10));
            Wall wFrontE = new Wall(new MyPlane(new Vec3(0, 0, -1), 30));
            roomE.walls.AddRange(new List<Wall> { wRightE, wLeftE, wBackE, wFrontE });

            Wall wRightF = new Wall(new MyPlane(new Vec3(-1, 0, 0), 30));
            Wall wLeftF = new Wall(new MyPlane(new Vec3(1, 0, 0), -10));
            Wall wBackF = new Wall(new MyPlane(new Vec3(0, 0, 1), -30));
            Wall wFrontF = new Wall(new MyPlane(new Vec3(0, 0, -1), 50));
            roomF.walls.AddRange(new List<Wall> { wRightF, wLeftF, wBackF, wFrontF });

            // left wing connections
            AddPortalData(wFrontA, new Vec3(-3, -4, 10), new Vec3(3, 4, 10), roomB);
            AddPortalData(wBackB, new Vec3(-3, -4, 10), new Vec3(3, 4, 10), roomA);

            AddPortalData(wFrontB, new Vec3(-3, -4, 30), new Vec3(3, 4, 30), roomC);
            AddPortalData(wBackC, new Vec3(-3, -4, 30), new Vec3(3, 4, 30), roomB);

            // right wing connections
            AddPortalData(wFrontD, new Vec3(17, -4, 10), new Vec3(23, 4, 10), roomE);
            AddPortalData(wBackE, new Vec3(17, -4, 10), new Vec3(23, 4, 10), roomD);

            AddPortalData(wFrontE, new Vec3(17, -4, 30), new Vec3(23, 4, 30), roomF);
            AddPortalData(wBackF, new Vec3(17, -4, 30), new Vec3(23, 4, 30), roomE);

            // B to E
            AddPortalData(wRightB, new Vec3(10, -4, 18), new Vec3(10, 4, 22), roomE);
            AddPortalData(wLeftE, new Vec3(10, -4, 18), new Vec3(10, 4, 22), roomB);

            // left branch
            BSPNode nodeFrontC = new BSPNode(wFrontC.plane, roomC, outside);
            BSPNode nodeDivB = new BSPNode(wFrontB.plane, roomB, nodeFrontC);
            BSPNode nodeDivA = new BSPNode(wFrontA.plane, roomA, nodeDivB);
            BSPNode nodeBackA = new BSPNode(wBackA.plane, nodeDivA, outside);
            BSPNode nodeLeftBranch = new BSPNode(wLeftA.plane, nodeBackA, outside);

            // right branch
            BSPNode nodeFrontF = new BSPNode(wFrontF.plane, roomF, outside);
            BSPNode nodeDivE = new BSPNode(wFrontE.plane, roomE, nodeFrontF);
            BSPNode nodeDivD = new BSPNode(wFrontD.plane, roomD, nodeDivE);
            BSPNode nodeBackD = new BSPNode(wBackD.plane, nodeDivD, outside);
            BSPNode nodeRightBranch = new BSPNode(wRightE.plane, nodeBackD, outside);

            // bramch root in two
            RootNode = new BSPNode(wRightA.plane, nodeLeftBranch, nodeRightBranch);

            // Colors left wing
            Color colorA = new Color(0.7f, 0.3f, 0.3f, 1f); // Rojo
            Color colorB = new Color(0.3f, 0.7f, 0.3f, 1f); // Verde
            Color colorC = new Color(0.3f, 0.3f, 0.7f, 1f); // Azul

            // Colors right wing
            Color colorD = new Color(0.3f, 0.7f, 0.7f, 1f); // Cyan
            Color colorE = new Color(0.7f, 0.3f, 0.7f, 1f); // Magenta
            Color colorF = new Color(0.7f, 0.7f, 0.3f, 1f); // Amarillo

            Vector2 wallSize = new Vector2(20f, 10f);

            // Visuals left wing
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

            // Visuals right wing
            visualizer.BuildVisualWall(wRightD, new Vec3(30, 0, 0), wallSize, colorD, "wRightD");
            visualizer.BuildVisualWall(wLeftD, new Vec3(10, 0, 0), wallSize, colorD, "wLeftD");
            visualizer.BuildVisualWall(wBackD, new Vec3(20, 0, -10), wallSize, colorD, "wBackD");
            visualizer.BuildVisualWall(wFrontD, new Vec3(20, 0, 10), wallSize, colorD, "wFrontD");

            visualizer.BuildVisualWall(wRightE, new Vec3(30, 0, 20), wallSize, colorE, "wRightE");
            visualizer.BuildVisualWall(wLeftE, new Vec3(10, 0, 20), wallSize, colorE, "wLeftE");
            visualizer.BuildVisualWall(wBackE, new Vec3(20, 0, 10), wallSize, colorE, "wBackE");
            visualizer.BuildVisualWall(wFrontE, new Vec3(20, 0, 30), wallSize, colorE, "wFrontE");

            visualizer.BuildVisualWall(wRightF, new Vec3(30, 0, 40), wallSize, colorF, "wRightF");
            visualizer.BuildVisualWall(wLeftF, new Vec3(10, 0, 40), wallSize, colorF, "wLeftF");
            visualizer.BuildVisualWall(wBackF, new Vec3(20, 0, 30), wallSize, colorF, "wBackF");
            visualizer.BuildVisualWall(wFrontF, new Vec3(20, 0, 50), wallSize, colorF, "wFrontF");

            // Objects left wing
            visualizer.SpawnDummyObject(new Vec3(5, 0, 5), Color.red, roomA);
            visualizer.SpawnDummyObject(new Vec3(-5, 0, 5), Color.red, roomA);
            visualizer.SpawnDummyObject(new Vec3(0, -2, 25), Color.green, roomB);
            visualizer.SpawnDummyObject(new Vec3(5, 2, 28), Color.green, roomB);
            visualizer.SpawnDummyObject(new Vec3(0, 0, 45), Color.blue, roomC);

            // objects right wing
            visualizer.SpawnDummyObject(new Vec3(25, 0, 5), Color.cyan, roomD);
            visualizer.SpawnDummyObject(new Vec3(15, 0, 5), Color.cyan, roomD);
            visualizer.SpawnDummyObject(new Vec3(20, -2, 25), Color.magenta, roomE);
            visualizer.SpawnDummyObject(new Vec3(25, 2, 28), Color.magenta, roomE);
            visualizer.SpawnDummyObject(new Vec3(20, 0, 45), Color.yellow, roomF);
        }

        private void AddPortalData(Wall wall, Vec3 min, Vec3 max, BSPRoom nextRoom)
        {
            Portal p = new Portal();
            p.corners[0] = min;
            p.corners[1] = max;
            p.nextRoom = nextRoom;

            p.SetupLocalSpace(wall.plane.normal);

            wall.AddPortal(p);
            AllPortals.Add(p);
        }
    }
}