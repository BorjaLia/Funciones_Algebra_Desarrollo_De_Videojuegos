using CustomMath;
using System.Collections.Generic;

public class VoronoiCell
{
    public Vec3 seed;
    public List<MyPlane> planes;

    public VoronoiCell(Vec3 seed) { this.seed = seed; this.planes = new List<MyPlane>(); }
    public void AddPlane(MyPlane plane) { planes.Add(plane); }
}
