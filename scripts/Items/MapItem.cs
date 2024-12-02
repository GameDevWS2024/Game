namespace Game.Scripts;
public class MapItem{
    public ItemStack Item { get; set; }
    public Location Location { get; set; }

    public MapItem(ItemStack item, int x, int y)
    {
        Item = item;
        Location.X = x;
        Location.Y = y;
    }
    public MapItem(ItemStack item, Location loc)
    {
        Item = item;
        Location = loc;
    }
}
