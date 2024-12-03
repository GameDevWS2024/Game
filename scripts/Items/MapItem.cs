namespace Game.Scripts.Items;
public class MapItem{
    public Itemstack Item { get; set; }
    public Location Location { get; set; }

    public MapItem(Itemstack item, int x, int y)
    {
        Item = item;
        Location.X = x;
        Location.Y = y;
    }
    public MapItem(Itemstack item, Location loc)
    {
        Item = item;
        Location = loc;
    }
}
