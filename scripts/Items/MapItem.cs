using Game.Scripts;

namespace Game.Scripts.Items;
public class MapItem{
    public Itemstack Item { get; set; }
    public Location Location { get; set; }

    public MapItem(Itemstack item, int x, int y)
    {
        Item = item;
        Location = new Location(x, y);
    }
    public MapItem(Itemstack item, Location loc)
    {
        Item = item;
        Location = loc;
    }
}
