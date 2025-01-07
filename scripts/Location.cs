using Godot;

namespace Game.Scripts;

public class Location(int x, int y)
{
    private readonly int _x = x;
    private readonly int _y = y;

    public Location(Vector2 vec) : this((int)vec.X, (int)vec.Y)
    {
    }

    public float DistanceTo(Location other)
    {
        return Mathf.Sqrt(Mathf.Pow(other._x - _x, 2) + Mathf.Pow(other._y - _y, 2));
    }

    public Vector2 ToVector2()
    {
        return new Vector2(_x, _y);
    }
}
