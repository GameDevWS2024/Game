using Godot; 
using Game.Scripts;
using System.Threading.Tasks;

public partial class ButtonControl : Control
{
	[Export] private Camera2D Camera1 = null!;
	[Export] private Camera2D Camera2 = null!;
	[Export] private Chat Ally1Chat = null!;
	[Export] private Chat Ally2Chat = null!;
	[Export] private CharacterBody2D Ally1 = null!;
	[Export] private CharacterBody2D Ally2 = null!;
	[Export] private Button ButtonAlly1 = null!;
	[Export] private Button ButtonAlly2 = null!;
	[Export] private PathFindingMovement Ally1Pathfinder = null!;
	[Export] private PathFindingMovement Ally2Pathfinder = null!;
	[Export] private RichTextLabel Ally1ResponseField = null!;
	[Export] private RichTextLabel Ally2ResponseField = null!;

	private int CurrentCamera = 1; // Tracks which camera is active (1 = Ally1, 2 = Ally2)
	private CharacterBody2D? ActiveCharacter = null; // The currently active character
	private PathFindingMovement? ActivePathfinder = null; // The active character's pathfinding logic
	private Vector2 TargetPosition; // Target position for movement
	private float MoveSpeed = 150f; // Movement speed
	private bool IsMouseOverUI = false; // Tracks if the mouse is hovering over the UI
	private bool isManualMovement = false; // Flag for manual movement
	private bool stopPathfinding = false; // Flag to stop pathfinding

	public override void _Ready()
	{
		base._Ready();

		// Connect the response signal from both chat instances to handle responses
		Ally1Chat.Connect("ResponseReceived", new Callable(this, nameof(HandleResponse)));
		Ally2Chat.Connect("ResponseReceived", new Callable(this, nameof(HandleResponse)));

		// Automatically assign pathfinding components to characters
		Ally1Pathfinder = Ally1.GetNode<PathFindingMovement>("PathFindingMovement");
		Ally2Pathfinder = Ally2.GetNode<PathFindingMovement>("PathFindingMovement");

		// Connect button signals to switch between characters
		ButtonAlly1.Pressed += OnButtonAlly1Pressed;
		ButtonAlly2.Pressed += OnButtonAlly2Pressed;

		// Connect mouse signals for UI elements
		Ally1Chat.Connect("mouse_entered", new Callable(this, nameof(OnMouseEnteredUI)));
		Ally1Chat.Connect("mouse_exited", new Callable(this, nameof(OnMouseExitedUI)));
		Ally2Chat.Connect("mouse_entered", new Callable(this, nameof(OnMouseEnteredUI)));
		Ally2Chat.Connect("mouse_exited", new Callable(this, nameof(OnMouseExitedUI)));

		// Activate Ally1 by default
		SwitchToAlly(1);
	}

	private void OnMouseEnteredUI()
	{
		IsMouseOverUI = true;
	}

	private void OnMouseExitedUI()
	{
		IsMouseOverUI = false;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Synchronize UI positions with the camera
		Vector2 cameraPosition = (CurrentCamera == 1) ? Camera1.GlobalPosition : Camera2.GlobalPosition;

		Ally1Chat.GlobalPosition = cameraPosition + new Vector2(519, 168);
		Ally2Chat.GlobalPosition = cameraPosition + new Vector2(519, 168);

		ButtonAlly1.GlobalPosition = cameraPosition + new Vector2(400, 300);
		ButtonAlly2.GlobalPosition = cameraPosition + new Vector2(600, 300);

		Ally1ResponseField.GlobalPosition = cameraPosition + new Vector2(519, 280);
		Ally2ResponseField.GlobalPosition = cameraPosition + new Vector2(519, 280);

		UpdateButtonPositions();

		// Handle manual movement if active
		if (ActiveCharacter != null && isManualMovement)
		{
			MoveManually(delta);
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Ignore input if the mouse is over the UI
		if (IsMouseOverUI)
		{
			return;
		}

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
			{
				// Switch to the other character on right-click
				int nextAlly = ActiveCharacter == Ally1 ? 2 : 1;
				SwitchToAlly(nextAlly);
				return;
			}

			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				// Get the global mouse position
				Vector2 mousePosition = GetGlobalMousePosition();

				// Ignore clicks on buttons
				if (ButtonAlly1.GetGlobalRect().HasPoint(mousePosition) || 
					ButtonAlly2.GetGlobalRect().HasPoint(mousePosition))
				{
					return;
				}

				// Set the target position for the active character
				if (ActiveCharacter != null && ActivePathfinder != null)
				{
					TargetPosition = mousePosition;
					ActivePathfinder.TargetPosition = mousePosition;
					isManualMovement = true;
				}
			}
		}
	}

	private void MoveManually(double delta)
	{
		if (ActiveCharacter != null)
		{
			// Move the character toward the target position
			if (ActiveCharacter.GlobalPosition != TargetPosition)
			{
				Vector2 direction = (TargetPosition - ActiveCharacter.GlobalPosition).Normalized();
				ActiveCharacter.GlobalPosition += direction * MoveSpeed * (float)delta;

				// Stop movement when the target is reached
				if (ActiveCharacter.GlobalPosition.DistanceTo(TargetPosition) < 5f)
				{
					ActiveCharacter.GlobalPosition = TargetPosition;
					isManualMovement = false;
					stopPathfinding = false;
				}
			}
		}
	}

	private void OnButtonAlly1Pressed()
	{
		SwitchToAlly(1);
	}

	private void OnButtonAlly2Pressed()
	{
		SwitchToAlly(2);
	}

	private void SwitchToAlly(int allyNumber)
	{
		if (allyNumber == 1)
	{
		Camera1.Enabled = true; // Activate Ally1's camera
		Camera2.Enabled = false; // Deactivate Ally2's camera

		Ally1Chat.Visible = true;
		Ally2Chat.Visible = false;

		Ally1ResponseField.Visible = true;
		Ally2ResponseField.Visible = false;

		ActiveCharacter = Ally1;
		ActivePathfinder = Ally1Pathfinder;
		CurrentCamera = 1;

		// Update button states
		ButtonAlly1.SetPressedNoSignal(true);
		ButtonAlly2.SetPressedNoSignal(false);
	}
	else if (allyNumber == 2)
	{
		Camera1.Enabled = false; // Activate Ally2's camera
		Camera2.Enabled = true; // Deactivate Ally1's camera

		Ally1Chat.Visible = false;
		Ally2Chat.Visible = true;

		Ally1ResponseField.Visible = false;
		Ally2ResponseField.Visible = true;

		ActiveCharacter = Ally2;
		ActivePathfinder = Ally2Pathfinder;
		CurrentCamera = 2;

		// Update button states
		ButtonAlly1.SetPressedNoSignal(false);
		ButtonAlly2.SetPressedNoSignal(true);
	}

		UpdateButtonPositions();
	}

	private void UpdateButtonPositions()
	{
		// Update button positions relative to the active chat's position
		Chat activeChat = CurrentCamera == 1 ? Ally1Chat : Ally2Chat;
		Vector2 chatGlobalPosition = activeChat.GlobalPosition;

		ButtonAlly1.GlobalPosition = chatGlobalPosition - new Vector2(0, 80);
		ButtonAlly2.GlobalPosition = chatGlobalPosition + new Vector2(150, -80);
	}

	private async Task TypeWriterEffect(string fullText, RichTextLabel label, float delay = 0.008f)
	{
		// Display text in a typewriter effect
		label.Text = "";

		for (int i = 0; i < fullText.Length; i++)
		{
			label.Text += fullText[i];
			await ToSignal(GetTree().CreateTimer(delay), "timeout");
		}
	}

	private async void DisplayResponse(string response, int allyNumber)
	{
		// Display the response in the correct response field
		RichTextLabel label = (allyNumber == 1) ? Ally1ResponseField : Ally2ResponseField;
		await TypeWriterEffect(response, label);
	}

	private void HandleResponse(string response)
	{
		// Handle the response received from the chat
		int activeAlly = CurrentCamera;
		DisplayResponse(response, activeAlly);
	}
}
