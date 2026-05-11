using CustomMath;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class MeshVisualizer
{
    private Voronoi manager;

    public MeshVisualizer(Voronoi manager)
    {
        this.manager = manager;
    }

    public void Initialize(List<VoronoiCell> cells, MyPlane[] boundingPlanes)
    {
        Debug.Log("Initialized Mesh Visualizer");

        for (int i = 0; i < cells.Count; i++)
        {
            VisualizeCell(cells[i], i);
        }
    }

    public void VisualizeCell(VoronoiCell cell, int id)
    {

    }
}