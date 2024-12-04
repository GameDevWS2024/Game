using Godot;

public class Location
{
    public int X;
    public int Y;
    public Location(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Location(Vector2 vec)
    {
        X = (int)vec.X;
        Y = (int)vec.Y;
    }

    public float DistanceTo(Location other)
    {
        return Mathf.Sqrt(Mathf.Pow(other.X - X, 2) + Mathf.Pow(other.Y - Y, 2));
    }

    public Vector2 toVector2()
    {
        return new Vector2(X, Y);
    }
}
