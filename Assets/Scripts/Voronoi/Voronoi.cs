using CustomMath;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Target
{
    public GameObject obj;

    public int id;

    public int currentSpace;

    public Vec3 lastPos;

    public void SetColor(Color color)
    {
        if(obj.GetComponent<MeshRenderer>())
        {
            obj.GetComponent<MeshRenderer>().material.color = color;
        }
        else
        {
            Debug.Log("No MeshRenderer found!");
        }
    }
}

//[ExecuteInEditMode]
public class Voronoi : MonoBehaviour
{
    public enum Visualizer { Planes, EzySlice }

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

    public List<Color> cellColors = new List<Color>();

    private List<Target> targets = new List<Target>();
    private MyPlane[] boundingPlanes = new MyPlane[6];
    private List<VoronoiCell> cells = new List<VoronoiCell>();

    private PlaneVisualizer planeVisualizer;
    private MeshVisualizer meshVisualizer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cleanup();

        planeVisualizer = new PlaneVisualizer(this);
        meshVisualizer = new MeshVisualizer(this);

        //Get the targets and load them into the target specific list
        LoadTargets();

        if (randomize)
        {
            RandomizeSeeds();
        }

        //Create the 6 bounding planes
        CreateBoundingBox();

        //Add the bounding planes to each of the cells
        for (int i = 0; i < seeds.Count; i++)
        {
            cells.Add(new VoronoiCell(seeds[i]));
            for (int j = 0; j < 5; j++)
            {
                cells[i].AddPlane(boundingPlanes[j]);
            }
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

        //Add a random color to each of the cells
        for (int i = 0; i < cells.Count; i++)
        {
            Color color = Random.ColorHSV();
            color.a = planesAlpha;
            cellColors.Add(color);
        }

        Visualize();
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
                bool isStillInside = true;
                //If we did move, first check if were still in the same space
                for (int j = 0; j < cells[targets[i].currentSpace].planes.Count; j++)
                {
                    if (!cells[targets[i].currentSpace].planes[j].GetSide(targets[i].lastPos)) { isStillInside = false; break; }
                }
                if (isStillInside) continue;
            }

            //If were not in the same space, we check every space
            bool foundNewSpace = false;
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
                    targets[i].SetColor(cellColors[j]);
                    foundNewSpace = true;
                    break;
                }
            }

            // If we arent inside anything, we default to -1
            if (!foundNewSpace)
            {
                print("Target has no space!");
                targets[i].currentSpace = -1;
                targets[i].SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            }
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

    void LoadTargets()
    {
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

    void Visualize()
    {
        switch (currentVisualizer)
        {
            case Visualizer.Planes:
                planeVisualizer.Initialize(cells,boundingPlanes);
                break;
            case Visualizer.EzySlice:
                meshVisualizer.Initialize(cells,boundingPlanes);
                break;
            default:
                break;
        }
    }

    void RandomizeSeeds()
    {
        seeds.Clear();
        for (int i = 0; i < seedAmount; i++)
        {
            seeds.Add(new Vec3(Random.Range(-maxSize.x / 2, maxSize.x / 2), Random.Range(-maxSize.y / 2, maxSize.y / 2), Random.Range(-maxSize.z / 2, maxSize.z / 2)));
        }
    }
}
