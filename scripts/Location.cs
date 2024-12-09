using Godot;

namespace Game.Scripts;

public class Location(int x, int y)
{
    public int X = x;
    public int Y = y;

    public Location(Vector2 vec) : this((int)vec.X, (int)vec.Y)
    {
    }

    public float DistanceTo(Location other)
    {
        return Mathf.Sqrt(Mathf.Pow(other.X - X, 2) + Mathf.Pow(other.Y - Y, 2));
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
}
