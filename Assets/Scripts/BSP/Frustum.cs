using CustomMath;

public class Frustum
{
    public MyPlane[] planes = new MyPlane[6];

    public Frustum(MyPlane[] planes) { this.planes = planes; }
}