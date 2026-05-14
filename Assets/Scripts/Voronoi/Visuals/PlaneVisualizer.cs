using CustomMath;
using System.Collections.Generic;
using UnityEngine;

public class VisualVoronoiCell
{
    public Transform parent;

    private GameObject seed;
    private List<GameObject> planes;

    public void SetSeed(GameObject seed) { this.seed = seed; }
    public void SetPlanes(List<GameObject> planes) { this.planes = planes; }
    public void AddPlane(GameObject plane) { this.planes.Add(plane); }

    public VisualVoronoiCell(Transform parent) { this.parent = parent; this.planes = new List<GameObject>(); }
}

public class PlaneVisualizer
{
    private Voronoi manager;

    private List<VisualVoronoiCell> visualCells = new List<VisualVoronoiCell>();

    public PlaneVisualizer(Voronoi manager)
    {
        this.manager = manager;
    }

    public void Initialize(List<VoronoiCell> cells, MyPlane[] boundingPlanes)
    {
        Debug.Log("Initialized Plane Visualizer");

        // Unity's plane is 10x10 units by default (we make it the size of our bounding box)
        manager.planeObject.transform.localScale = (manager.maxSize / 10);

        VisualizeBoundingBox(boundingPlanes);

        for (int i = 0; i < cells.Count; i++)
        {
            VisualizeCell(cells[i], i);

            for (int j = 6; j < cells[i].planes.Count; j++)
            {
                AddVisualPlane(cells[i].planes[j], i);
            }
        }
    }

    public void VisualizeBoundingBox(MyPlane[] boundingPlanes)
    {
        // Visually show the bounding box
        for (int i = 0; i < 6; i++)
        {
            string name = "Bounding plane" + i.ToString();

            CreateVisualPlane(boundingPlanes[i], manager.planeParent, manager.boundsColor, name);

            //Create a second one (flipped) so that its visible from the front & back
            CreateVisualPlane(boundingPlanes[i].flipped, manager.planeParent, manager.boundsColor, name);
        }
    }

    public void VisualizeCell(VoronoiCell cell, int id)
    {
        Debug.Log("visualize: instance " + id);

        GameObject instance = Object.Instantiate(manager.cellHolder.gameObject, manager.cellParent);
        instance.name = "Voronoi Cell " + id.ToString();

        VisualVoronoiCell newCell = new VisualVoronoiCell(instance.transform);
        newCell.SetSeed(CreateVisualSeed(cell.seed, instance.transform, manager.cellColors[id]));

        visualCells.Add(newCell);
    }

    public void AddVisualPlane(MyPlane plane, int cellId)
    {
        Transform parent = visualCells[cellId].parent;

        visualCells[cellId].AddPlane(CreateVisualPlane(plane, parent, manager.cellColors[cellId]));
    }

    public Color GetCellColor(int cellId)
    {
        if (cellId >= 0 && cellId < visualCells.Count)
            return manager.cellColors[cellId];
        return new Color(1.0f, 1.0f, 1.0f, 1.0f); // default white 
    }

    private GameObject CreateVisualSeed(Vec3 seed, Transform parent, Color color = new Color())
    {
        Debug.Log("Created seed " + seed.ToString());
        GameObject seedObj = Object.Instantiate(manager.seedObject, parent);

        seedObj.transform.name = "Seed " + seed.ToString();
        seedObj.transform.position = seed;

        seedObj.GetComponent<MeshRenderer>().material.color = color;

        return seedObj;
    }

    private GameObject CreateVisualPlane(MyPlane plane, Transform parent, Color color = new Color(), string name = "")
    {
        Debug.Log("Created plane " + plane.normal.ToString());
        GameObject planeObj = Object.Instantiate(manager.planeObject, parent);
        if (name == "") planeObj.transform.name = plane.normal.ToString();
        else planeObj.transform.name = name;
        planeObj.transform.position = new Vec3(-plane.normal * plane.distance);
        planeObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, plane.normal);

        planeObj.GetComponent<MeshRenderer>().material.color = color;

        return planeObj;
    }
}