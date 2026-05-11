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

        VisualizeBoundingBox(boundingPlanes);

        for (int i = 0; i < cells.Count; i++)
        {
            VisualizeCell(cells[i], i);
        }
    }

    public void VisualizeBoundingBox(MyPlane[] boundingPlanes)
    {
        // Visually show the bounding box
    }

    public void VisualizeCell(VoronoiCell cell, int id)
    {
        // 1. Crear el material con el color asignado desde el Manager
        Material cellMaterial = new Material(Shader.Find("Standard"));
        cellMaterial.color = manager.cellColors[id];

        // 2. Instanciar la "arcilla" inicial (nuestra Bounding Box)
        GameObject clay = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Destruimos el collider por defecto del cubo para que no interfiera físicamente aún
        Object.DestroyImmediate(clay.GetComponent<Collider>());

        clay.transform.position = Vector3.zero;

        // Asumimos que Vec3 tiene casteo implícito a Vector3. Si te da error de casteo, 
        // cámbialo a: new Vector3(manager.maxSize.x, manager.maxSize.y, manager.maxSize.z)
        clay.transform.localScale = manager.maxSize;
        clay.name = "Voronoi Mesh " + id;
        clay.transform.parent = manager.cellParent;
        clay.GetComponent<MeshRenderer>().material = cellMaterial;

        // 3. Cortar la arcilla plano por plano
        // Comenzamos desde el índice 6, porque la arcilla YA ES la Bounding Box.
        for (int j = 6; j < cell.planes.Count; j++)
        {
            MyPlane p = cell.planes[j];

            // EzySlice utiliza Vector3 nativo de Unity para hacer el corte
            Vector3 planePos = p.normal * p.distance;
            Vector3 planeNormal = p.normal;

            // Realizamos el corte
            SlicedHull hull = clay.Slice(planePos, planeNormal);

            if (hull != null)
            {
                // La magia matemática: en CalculatePlane hicimos que la normal apunte
                // desde la semilla 'objetivo' HACIA nuestra semilla actual (cell.seed).
                // Eso significa que el espacio "adentro" de la celda SIEMPRE es el lado "Upper".
                GameObject upper = hull.CreateUpperHull(clay, cellMaterial);

                if (upper != null)
                {
                    // Configuramos la nueva porción
                    upper.transform.parent = manager.cellParent;
                    upper.name = "Voronoi Mesh " + id;

                    // Destruimos la arcilla vieja y los residuos del corte
                    Object.DestroyImmediate(clay);

                    // La nueva porción se convierte en nuestra arcilla actual para el siguiente corte
                    clay = upper;
                }
            }
        }

        // Opcional: Si en el futuro necesitas que tus Targets reboten o detecten colisión 
        // física contra la malla terminada, puedes añadirle un MeshCollider aquí:
        // clay.AddComponent<MeshCollider>().convex = true;
    }
}