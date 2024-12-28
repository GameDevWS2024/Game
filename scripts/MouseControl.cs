using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

public partial class MouseControl : Control
{
    // The RichTextLabel that will follow the mouse
    private RichTextLabel _richTextLabel = null!;
    private CharacterBody2D _selectedEntity;
    private int clickRadius = 50;
    Node _pathFindingMovement;


    public override void _Ready()
    {
        // Get the RichTextLabel node
        _richTextLabel = GetNode<RichTextLabel>("%MouseLabel");
        _richTextLabel.Visible = false;
        _pathFindingMovement = GetNode<PathFindingMovement>("PathFindingMovementNode");
    }

    public override void _Input(InputEvent @event)
    {
        // Check if the event is a mouse button event and if it's the left button
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            Vector2 mousePosition = GetGlobalMousePosition();
            SelectNearestEntity(mousePosition);
        }
    }

    private void SelectNearestEntity(Vector2 clickPosition)
    {
        CharacterBody2D nearestEntity = null;
        float nearestDistance = float.MaxValue;

        List<CharacterBody2D> _entityGroup = GetTree().GetNodesInGroup("Entities").OfType<CharacterBody2D>().ToList();

        foreach (CharacterBody2D entity in _entityGroup)
        {
            float distance = entity.GlobalPosition.DistanceTo(clickPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEntity = entity;
            }
        }

        // select nearest entity and set coordinates visible
        if (nearestDistance <= clickRadius)
        {
            _richTextLabel.Visible = true;
            _selectedEntity = nearestEntity;
            //check what type the entity is and focus on the text box
            if (_selectedEntity is Ally ally)
            {
                ally._chat.Visible = true;
                ally._chat.GrabFocus();
                GD.Print($"Selected entity: Ally");
            }
            else if (_selectedEntity is CombatAlly combatAlly)
            {
                combatAlly._chat.Visible = true;
                combatAlly._chat.GrabFocus();
                GD.Print($"CombatAlly");
            }
        }
        //if mouseclick was outside of defined radius
        else if (_selectedEntity != null)
        {
            //check type to send ally or combat ally to selected coordinates 
            if (_selectedEntity is Ally ally)
            {
                ally._followPlayer = false;
                ally._pathFindingMovement.TargetPosition = clickPosition;
                ally._pathFindingMovement.gotoCommand = true;
                ally._chat.Visible = false;
            }
            else if (_selectedEntity is CombatAlly combatAlly)
            {
                combatAlly._followPlayer = false;
                combatAlly._pathFindingMovement.TargetPosition = clickPosition;
                combatAlly._pathFindingMovement.gotoCommand = true;
                combatAlly._chat.Visible = false;
            }
            _selectedEntity = null;
            _richTextLabel.Visible = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Set the position of the RichTextLabel to the mouse position
        Vector2 mousePosition = GetGlobalMousePosition();
        _richTextLabel.Text = "x: " + (int)mousePosition.X + " y: " + (int)mousePosition.Y;
        _richTextLabel.Position = mousePosition;
    }
}
