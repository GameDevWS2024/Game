using System;

using Game.Scripts.AI;
using Game.Scripts.Interaction;

using Godot;

using Material = Game.Scripts.Items.Material;
public partial class AiNode : Node2D
{
    [ExportGroup("Object Name and Description for AI")]
    public bool IsActive = true;
    [Export] public string ObjectName = "";
    [Export] public string ObjectDescription = "";

    [Export]
    public string ObjectHint = "Tell the commander about the object you just spotted. " +
                                  "You may use the command [GOTO AND INTERACT] on this object if the commander explicitly told you to engage with it." +
                                  "If he didn't, propose that you may interact with it, but don't use the word interact. " +
                                  "Use the appropriate term or verb for: ";

    [ExportGroup("Interaction")]
    [Export(PropertyHint.None, "Can be interacted with?")] public bool Interactable = true;
    [Export(PropertyHint.None, "is removed after interacting?")] public bool RemovedAfter = true;

    [ExportGroup("System Message after interaction")]
    [Export] public string? CustomOverrideMessage;
    [Export] public string?[] ItemAdderMessage = ["You picked up: ", "It could possibly be useful for ..."];
    [Export] public string? DefaultMessage = "Unfortunately, that didn't do anything.";
    [Export] public string EndMessage = "What do you want the commander to know about that?";

    [ExportGroup("Item Adder")]
    [Export] public Material AddedMaterial;
    [Export] public int Amount = 1;

    [ExportGroup("Show scene while in radius (and has item)")]
    [Export] public bool ShowWhileInRadius;
    [Export] public PackedScene? SceneToShow;
    [Export] public int Radius = 100;
    [Export] public Material RequiredMaterial;

    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("trying to get nodes");
        VisibleForAI visibleForAi = GetNode<VisibleForAI>("VisibleForAI");
        Game.Scripts.Interaction.ItemAdder adder = GetNode<Game.Scripts.Interaction.ItemAdder>("ItemAdder");
        ShowWhileInRadius showWhileInRadius = GetNode<ShowWhileInRadius>("ShowWhileInRadius");
        Removeable removable = GetNode<Removeable>("Removeable");
        Interactable interactable = GetNode<Interactable>("Interactable");
        GD.Print("got all nodes");

        if (IsActive)
        {
            visibleForAi.NameForAi = ObjectName;
            visibleForAi.DescriptionForAi = ObjectDescription;

            if (Interactable)
            {
                visibleForAi.DescriptionForAi += " [HINT] " + ObjectHint + "[" + visibleForAi.NameForAi + "] [HINT END].";
                if (!string.IsNullOrEmpty(CustomOverrideMessage)) // voreingestellte System message
                {
                    GD.Print("custom override message");
                    interactable.SystemMessageForAlly = CustomOverrideMessage
                        + EndMessage
                        + "\n";
                }
                else if (AddedMaterial != Game.Scripts.Items.Material.None) // sonst wenn item geadded wird, zusammengesetzte SystemMessage
                {
                    GD.Print("added material system message");
                    adder.Amount = Amount;
                    adder.ItemToAdd = AddedMaterial;

                    string composedMessage = ItemAdderMessage + " " + Amount + " " + AddedMaterial
                                                                   + ((Amount > 1) ? "s " : " ")
                                                                   + (RemovedAfter ? "Now, it's no longer available. " : " ")
                                                                   + EndMessage
                                                                   + "\n";
                    interactable.SystemMessageForAlly = composedMessage;
                    GD.Print(interactable.SystemMessageForAlly + " ***** item adder default system message *****");
                }
                else
                {
                    GD.Print("default system message");
                    interactable.SystemMessageForAlly = DefaultMessage
                                                                   + "\n";
                    adder.QueueFree();
                }
                if (!RemovedAfter)
                {
                    removable.QueueFree();
                }
            }
            else
            {
                interactable.QueueFree();
            }
        }
        else
        {
            visibleForAi.QueueFree();
            adder.QueueFree();
        }


        if (ShowWhileInRadius && SceneToShow != null)
        {
            showWhileInRadius.Radius = Radius;
            showWhileInRadius.NeedsToBeInInventoryName = RequiredMaterial;
            showWhileInRadius.SceneToShow = SceneToShow;
        }
        else
        {
            if (ShowWhileInRadius)
            {
                GD.Print("showWhileInRadius removed because selected Scene is null.");
            }
            showWhileInRadius.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        
        if (!IsActive)
        {
            GetParent().QueueFree();
        }
    }
}
