public class MapItem{
    public ItemStack Item { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public MapItem(ItemStack item, int x, int y)
    {
        Item = item;
        X = x;
        Y = y;
    }
}
