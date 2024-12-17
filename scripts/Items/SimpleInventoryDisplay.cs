using System.Linq;

using Game.Scripts;

using Godot;

public partial class SimpleInventoryDisplay : RichTextLabel
{
    private Inventory? _inventory = null;
    public override void _Ready()
    {
        Core? core = GetTree().GetNodesInGroup(Core.GroupName).OfType<Core>().FirstOrDefault();
        if (core != null)
        {
            _inventory = core.Inventory;
            _inventory.Changed += UpdateDisplay;
        }
    }

    private void UpdateDisplay()
    {
        if (_inventory is not null)
        {
            Text = string.Join("\n", _inventory.Items.Select(item => $"{item.Key.Name}: {item.Value}"));
        }
    }

}
