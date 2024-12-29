using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

public partial class MouseControl : Control
{
    // The RichTextLabel that will follow the mouse
    private RichTextLabel _richTextLabel = null!;
    private CharacterBody2D _selectedEntityGoto;
    private CharacterBody2D _selectedEntityChat;
    private int _clickRadius = 50;
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
        // Check if the event is a mouse button event and if it's the left button and select entity for goto
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            EntityGoto(GetGlobalMousePosition());
        }
        // Check if the event is a mouse button event and if it's the right button and open chat up
        else if (@event is InputEventMouseButton mouseEvent1 && mouseEvent1.Pressed && mouseEvent1.ButtonIndex == MouseButton.Right)
        {
            ChatPopUp(GetGlobalMousePosition());
        }
    }

    private Tuple<CharacterBody2D, float> SearchNearestEntity(Vector2 clickPosition)
    {
        CharacterBody2D nearestEntity = null;
        float nearestDistance = float.MaxValue;

        List<CharacterBody2D> entityGroup = GetTree().GetNodesInGroup("Entities").OfType<CharacterBody2D>().ToList();

        foreach (CharacterBody2D entity in entityGroup)
        {
            float distance = entity.GlobalPosition.DistanceTo(clickPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEntity = entity;
            }
        }
        return Tuple.Create(nearestEntity, nearestDistance);
    }
    //chat pop up function on right click
    private void ChatPopUp(Vector2 clickPosition)
    {
        Tuple<CharacterBody2D, float> resultTuple = SearchNearestEntity(clickPosition);
        CharacterBody2D nearestEntity = resultTuple.Item1;
        float nearestDistance = resultTuple.Item2;

        // select nearest entity and set coordinates visible
        if (nearestDistance <= _clickRadius && _selectedEntityChat == null)
        {
            _selectedEntityChat = nearestEntity;
            //check what type the entity is and focus on the text box
            if (_selectedEntityChat is Ally ally)
            {
                ally.Chat.Visible = true;
                ally.Chat.GrabFocus();
            }
            else if (_selectedEntityChat is CombatAlly combatAlly)
            {
                combatAlly.Chat.Visible = true;
                combatAlly.Chat.GrabFocus();
            }
        }
        //if mouseclick was outside of defined radius
        else if (_selectedEntityChat != null)
        {
            //check type to send ally or combat ally to selected coordinates 
            if (_selectedEntityChat is Ally ally)
            {
                ally.Chat.Visible = false;
            }
            else if (_selectedEntityChat is CombatAlly combatAlly)
            {
                combatAlly.Chat.Visible = false;
            }
            _selectedEntityChat = null;
        }
    }

    //Select nearest Entity to mouseclick
    private void EntityGoto(Vector2 clickPosition)
    {
        Tuple<CharacterBody2D, float> resultTuple = SearchNearestEntity(clickPosition);
        CharacterBody2D nearestEntity = resultTuple.Item1;
        float nearestDistance = resultTuple.Item2;

        // select nearest entity and set coordinates visible
        if (nearestDistance <= _clickRadius)
        {
            _richTextLabel.Visible = true;
            _selectedEntityGoto = nearestEntity;
        }
        //if mouseclick was outside of defined radius
        else if (_selectedEntityGoto != null)
        {
            //check type to send ally or combat ally to selected coordinates 
            if (_selectedEntityGoto is Ally ally)
            {
                ally.FollowPlayer = false;
                ally.PathFindingMovement.TargetPosition = clickPosition;
            }
            else if (_selectedEntityGoto is CombatAlly combatAlly)
            {
                combatAlly.FollowPlayer = false;
                combatAlly.PathFindingMovement.TargetPosition = clickPosition;
            }
            _selectedEntityGoto = null;
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
