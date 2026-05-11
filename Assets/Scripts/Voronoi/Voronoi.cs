using CustomMath;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


class VoronoiCell
{
    public Vec3 seed;
    public List<MyPlane> planes;

    public VoronoiCell(Vec3 seed) { this.seed = seed; this.planes = new List<MyPlane>(); }
    public void AddPlane(MyPlane plane) { planes.Add(plane); }
}

class VisualVoronoiCell
{
    public Transform parent;

    public Color color = Random.ColorHSV();

    GameObject seed;
    List<GameObject> planes;

    public void SetSeed(GameObject seed) { this.seed = seed; }
    public void SetPlanes(List<GameObject> planes) { this.planes = planes; }
    public void AddPlane(GameObject plane) { this.planes.Add(plane); }

    public VisualVoronoiCell(Transform parent) { this.parent = parent; this.planes = new List<GameObject>(); this.color = Random.ColorHSV(); }
    public VisualVoronoiCell(Transform parent, GameObject seed, List<GameObject> planes) { this.parent = parent; this.seed = seed; this.planes = planes; this.color = Random.ColorHSV(); }
}

class VoronoiMesh
{
    public List<Vec3> vertices;
    public List<int> triangles;
}

[System.Serializable]
public class Target
{
    public GameObject obj;

    public int id;

    public int currentSpace;

    public Vec3 lastPos;
}

//[ExecuteInEditMode]
public class Voronoi : MonoBehaviour
{
    public enum Visualizer { Planes,EzySlice }

    [Header("Visuals")]

    [SerializeField] private Visualizer currentVisualizer;


    [Header("References")]

    [SerializeField] public Transform cellParent;
    [SerializeField] public GameObject seedObject;
    [SerializeField] public Transform planeParent;
    [SerializeField] public GameObject planeObject;
    [SerializeField] public GameObject cellHolder;


    [Header("Objectives")]

    [SerializeField] public List<GameObject> objectives;


    [Header("Bounding box")]

    [SerializeField] public Color boundsColor = new Color();
    [Range(0.0f, 1.0f)] public float planesAlpha = 0.5f;


    [Header("Attributes")]

    [SerializeField] public bool randomize = true;
    [SerializeField] public int seedAmount = 10;
    [SerializeField] public Vec3 maxSize = new Vec3(100, 100, 100);
    

    [Header("Seeds")]

    [SerializeField] public List<Vec3> seeds = new List<Vec3>();


    private List<Target> targets = new List<Target>();

    private MyPlane[] boundingPlanes = new MyPlane[6];

    private List<VoronoiCell> cells = new List<VoronoiCell>();
    private List<VisualVoronoiCell> visualCells = new List<VisualVoronoiCell>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Unity's plane is 10x10 units by default
        planeObject.transform.localScale = (maxSize / 10);

        Cleanup();

        if (randomize)
        {
            seeds.Clear();
            for (int i = 0; i < seedAmount; i++)
            {
                seeds.Add(new Vec3(Random.Range(-maxSize.x / 2, maxSize.x / 2), Random.Range(-maxSize.y / 2, maxSize.y / 2), Random.Range(-maxSize.z / 2, maxSize.z / 2)));
            }
        }

        //Create the 6 bounding planes
        CreateBoundingBox();

        // Visually show the boundong box
        for (int i = 0; i < 6; i++)
        {
            string name = "Bounding plane" + i.ToString();

            CreateVisualPlane(boundingPlanes[i], planeParent, boundsColor, name);

            //Create a second one (flipped) so that its visible from the front & back
            CreateVisualPlane(boundingPlanes[i].flipped, planeParent, boundsColor, name);
        }

        //Add the bounding planes to each of the cells
        for (int i = 0; i < seeds.Count; i++)
        {
            cells.Add(new VoronoiCell(seeds[i]));
            for (int j = 0; j < 5; j++)
            {
                cells[i].AddPlane(boundingPlanes[j]);
            }

            // Visually show the cells seeds and planes
            visualCells.Add(VisualizeCell(cells[i], i));
            visualCells[i].color.a = planesAlpha;
        }

        //Iterate through every seed pair and generate their planes
        for (int i = 0; i < cells.Count; i++)
        {
            for (int j = i + 1; j < cells.Count; j++)
            {
                CalculatePlane(cells[i], i, cells[j].seed);
                CalculatePlane(cells[j], j, cells[i].seed);
            }
        }

        //Get the targets and load them into the target specific list
        for (int i = 0; i < objectives.Count; i++)
        {
            Target target = new Target();

            target.id = i;
            target.obj = objectives[i];
            target.currentSpace = -1;
            target.lastPos = new Vec3();

            targets.Add(target);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            // Only check if we moved
            if (targets[i].lastPos == targets[i].obj.transform.position) return;

            targets[i].lastPos = new Vec3(targets[i].obj.transform.position);

            if (targets[i].currentSpace >= 0) // Only check if in a valid space
            {
                //If we did move, first check if were still in the same space
                for (int j = 0; j < cells[targets[i].currentSpace].planes.Count; j++)
                {
                    if (!cells[targets[i].currentSpace].planes[j].GetSide(targets[i].lastPos)) return;
                }
            }

            //If were not in the same space, we check every space
            for (int j = 0; j < cells.Count; j++)
            {
                bool inside = true;
                for (int k = 0; k < cells[j].planes.Count; k++)
                {
                    if (!cells[j].planes[k].GetSide(targets[i].lastPos)) inside = false;
                }
                if (inside)
                {
                    print("Target changed space!");
                    targets[i].currentSpace = j;
                    targets[i].obj.GetComponent<MeshRenderer>().material.color = visualCells[j].color;
                    return;
                }
            }

            // If we arent inside anything, we default to -1
            print("Target has no space!");
            targets[i].currentSpace = -1;
            targets[i].obj.GetComponent<MeshRenderer>().material.color = new Color();
        }
    }

    void CalculatePlane(VoronoiCell cell, int cellId, Vec3 otherSeed)
    {
        MyPlane newPlane = new MyPlane();

        // Normal is the normalized vector from the current cell's seed & the objective seed
        Vec3 difference = cell.seed - otherSeed;
        newPlane.normal = difference.normalized;

        // Distance is the distance from the origin (0.0f,0.0f,0.0f)
        newPlane.distance = Vector3.Dot(newPlane.normal, ((cell.seed + otherSeed) / 2.0f));

        cell.AddPlane(newPlane);
        Transform parent = visualCells[cellId].parent;
        Color color = visualCells[cellId].color;

        visualCells[cellId].AddPlane(CreateVisualPlane(newPlane, parent, color));
    }

    void Cleanup()
    {
        // Backwards for loop in case we want to use DestryInmmediate
        for (int i = cellParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cellParent.GetChild(i).GameObject());
        }
        for (int i = planeParent.childCount - 1; i >= 0; i--)
        {
            Destroy(planeParent.GetChild(i).GameObject());
        }
    }

    void CreateBoundingBox()
    {
        // Bounding planes

        /// bottom
        boundingPlanes[0] = new MyPlane(Vec3.Up, maxSize.y / 2);

        /// top
        boundingPlanes[1] = new MyPlane(Vec3.Down, maxSize.y / 2);

        /// left
        boundingPlanes[2] = new MyPlane(Vec3.Right, maxSize.x / 2);

        /// right
        boundingPlanes[3] = new MyPlane(Vec3.Left, maxSize.x / 2);

        /// back
        boundingPlanes[4] = new MyPlane(Vec3.Forward, maxSize.z / 2);

        /// front
        boundingPlanes[5] = new MyPlane(Vec3.Back, maxSize.z / 2);
    }

    VisualVoronoiCell VisualizeCell(VoronoiCell cell, int id)
    {
        print("visualize: instance " + id);
        GameObject instance = Instantiate(cellHolder.gameObject, cellParent);
        instance.name = "Voronoi Cell " + id.ToString();

        VisualVoronoiCell newCell = new VisualVoronoiCell(instance.transform);
        newCell.SetSeed(CreateVisualSeed(cell.seed, instance.transform, newCell.color));

        /// Start at 6 to avoid re-draw of the bounding box
        //for (int j = 6; j < cell.planes.Count; j++)
        //{
        //    print("plane made ");
        //    newCell.AddPlane(CreateVisualPlane(cell.planes[j], instance.transform, newCell.color));
        //}

        return newCell;
    }

    GameObject CreateVisualSeed(Vec3 seed, Transform parent, Color color = new Color())
    {
        print("Created seed " + seed.ToString());
        GameObject seedObj = Instantiate(seedObject, parent);
        //if (name == "") seedObj.transform.name = seed.ToString();
        //else seedObj.transform.name = name;
        seedObj.transform.name = "Seed " + seed.ToString();
        seedObj.transform.position = seed;

        seedObj.GetComponent<MeshRenderer>().material.color = color;

        return seedObj;
    }

    GameObject CreateVisualPlane(MyPlane plane, Transform parent, Color color = new Color(), string name = "")
    {
        print("Created plane " + plane.normal.ToString());
        GameObject planeObj = Instantiate(planeObject, parent);
        if (name == "") planeObj.transform.name = plane.normal.ToString();
        else planeObj.transform.name = name;
        planeObj.transform.position = new Vec3(plane.normal * plane.distance);
        planeObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, plane.normal);

        planeObj.GetComponent<MeshRenderer>().material.color = color;

        print("Finished plane");
        return planeObj;
    }
}
