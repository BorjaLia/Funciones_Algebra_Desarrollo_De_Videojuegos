using CustomMath;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class VoronoiMesh
{
    public Transform parent;

    private GameObject seed;
    private GameObject meshObj;

    public void SetSeed(GameObject seed) { this.seed = seed; }
    public void SetMesh(GameObject meshObj) { this.meshObj = meshObj; }

    public VoronoiMesh(Transform parent) { this.parent = parent; }
}

public class MeshVisualizer
{
    private Voronoi manager;

    private List<VoronoiMesh> visualMeshes = new List<VoronoiMesh>();

    public MeshVisualizer(Voronoi manager)
    {
        this.manager = manager;
    }

    public void Initialize(List<VoronoiCell> cells, MyPlane[] boundingPlanes)
    {
        Debug.Log("Initialized Mesh Visualizer");

        VisualizeBoundingBox(boundingPlanes);

        for (int i = 0; i < cells.Count; i++)
        {
            VisualizeCell(cells[i], i);
        }
    }

    public void VisualizeBoundingBox(MyPlane[] boundingPlanes)
    {
        // Visually show the bounding box
        CreateMesh(manager.maxSize,manager.planeParent,manager.boundsColor, "Bounding volume");
    }

    public void VisualizeCell(VoronoiCell cell, int id)
    {
        Debug.Log("visualize mesh: instance " + id);

        GameObject instance = Object.Instantiate(manager.cellHolder.gameObject, manager.cellParent);
        instance.name = "Voronoi Cell " + id.ToString();

        VoronoiMesh newMesh = new VoronoiMesh(instance.transform);
        newMesh.SetSeed(CreateVisualSeed(cell.seed, instance.transform, manager.cellColors[id]));

        GameObject baseMesh = CreateMesh(manager.maxSize, instance.transform, manager.cellColors[id]);
        baseMesh.name = "Cell Mesh " + id.ToString();

        for (int j = 6; j < cell.planes.Count; j++)
        {
            baseMesh = CutMesh(baseMesh, cell.planes[j]);
        }
        baseMesh.name = "Cell Mesh " + id.ToString();

        newMesh.SetMesh(baseMesh);
        visualMeshes.Add(newMesh);
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

    private GameObject CreateMesh(Vec3 size, Transform parent, Color color, string name = "")
    {
        Debug.Log("Created mesh");

        GameObject meshObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(meshObj.GetComponent<Collider>());

        meshObj.transform.parent = parent;
        meshObj.transform.localPosition = Vec3.Zero;
        meshObj.transform.localScale = size;
        
        meshObj.GetComponent<MeshRenderer>().material = manager.meshMaterial;
        meshObj.GetComponent<MeshRenderer>().material.color = color;

        return meshObj;
    }

    private GameObject CutMesh(GameObject meshObj, MyPlane plane)
    {

        Vec3 planePos = plane.normal * plane.distance;

        Material currentMaterial = meshObj.GetComponent<MeshRenderer>().sharedMaterial;

        SlicedHull hull = meshObj.Slice(planePos, plane.normal, currentMaterial);

        if (hull != null)
        {
            GameObject upper = hull.CreateUpperHull(meshObj, currentMaterial);

            if (upper != null)
            {
                upper.transform.parent = meshObj.transform.parent;
                
                Object.DestroyImmediate(meshObj);
                
                return upper;
            }
        }
        return meshObj;
    }
}