using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

using Vector2 = Godot.Vector2;

namespace Game.Scripts;

public partial class Ally : CharacterBody2D
{
	[Export] RichTextLabel _responseField = null!;
	[Export] public PathFindingMovement PathFindingMovement = null!;
	[Export] private Label _nameLabel = null!;
	private Motivation _motivation = null!;
	private Health _health = null!;
	protected Game.Scripts.Core _core = null!;
	private static readonly Inventory SsInventory = new Inventory(36);

	[Export] private int _visionRadius = 300;
	[Export] private int _interactionRadius = 150;
	private bool _interactOnArrival, _busy, _reached, _harvest, _returning;

	[Export] public Chat Chat = null!;
	public Map? Map;
	Chat? _chatNode;
	[Export] public VisibleForAI[] AlwaysVisible = [];
	private GenerativeAI.Methods.ChatSession? _chat;
	private Location? positionOnScreen = null!;
	private GeminiService? _geminiService;
	private readonly List<string> _interactionHistory = [];
	[Export] private int _maxHistory = 5; // Number of interactions to keep

	//Enum with states for ally in darkness, in bigger or smaller circle for map damage system
	public enum AllyState
	{
		Darkness,
		SmallCircle,
		BigCircle
	}
	public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;


	public override void _Ready()
	{
		Map = GetTree().Root.GetNode<Map>("Node2D");
		_chatNode = GetNode<Chat>("Chat");
		_geminiService = _chatNode.GeminiService;
		_chat = _geminiService!.Chat;
		base._Ready();
		_motivation = GetNode<Motivation>("Motivation");
		_health = GetNode<Health>("Health");
		Chat.ResponseReceived += HandleResponse;

		_core = GetNode<Core>("%Core");
		Chat.Visible = false;
		PathFindingMovement.ReachedTarget += HandleTargetReached;
		Chat.ResponseReceived += HandleResponse;
	}
	
	/*private void PositionChat()
	{
		Vector2 chatSize = GetSize();  // Get the actual size of the node at runtime.

		// Get the screen size (viewport size)
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;

		// Get the current size of the Chat (LineEdit) node using RectMinSize
		Vector2 chatSize = RectMinSize;

		// Position the Chat node (LineEdit) at the bottom-left with a margin of 10
		Position = new Vector2(10, screenSize.Y - chatSize.Y - 10);
	}*/

	private async void HandleTargetReached()
	{
		if (_interactOnArrival)
		{
			Interactable? interactable = GetCurrentlyInteractables().FirstOrDefault();
			interactable?.Trigger(this);
			_interactOnArrival = false;

			GD.Print("Interacted");
			List<VisibleForAI> visibleItems = GetCurrentlyVisible().Concat(AlwaysVisible).ToList();
			string visibleItemsFormatted = string.Join<VisibleForAI>("\n", visibleItems);
			string completeInput = $"Currently Visible:\n\n{visibleItemsFormatted}\n\n";

			string originalSystemPrompt = Chat.SystemPrompt;
			Chat.SystemPrompt =
				"In the following you'll get a list of things you see with coordinates. Respond by telling the commander just what might be important or ask clarifying questions on what to do next. \n";
			string? arrivalResponse = await _geminiService!.MakeQuery(completeInput);
			List<(string, string)> responseGroups = ExtractRelevantLines(arrivalResponse!);
			foreach ((string, string) response in responseGroups)
			{
				if (response.Item1 == "RESPONSE")
				{
					GD.Print(response.Item2);
				}
			}

			RichTextLabel label = GetNode<RichTextLabel>("ResponseField");
			label.Text += "\n" + arrivalResponse;

			Chat.SystemPrompt = originalSystemPrompt;
		}
		else
		{
			GD.Print("DEBUG: Nothing interacted :"+_interactOnArrival);
		}
	}

	public List<VisibleForAI> GetCurrentlyVisible()
	{
		IEnumerable<VisibleForAI> visibleForAiNodes =
			GetTree().GetNodesInGroup(VisibleForAI.GroupName).OfType<VisibleForAI>();
		return visibleForAiNodes.Where(node => GlobalPosition.DistanceTo(node.GlobalPosition) <= _visionRadius)
			.ToList();
	}

	public List<Interactable> GetCurrentlyInteractables()
	{
		IEnumerable<Interactable> interactable =
			GetTree().GetNodesInGroup(Interactable.GroupName).OfType<Interactable>();
		return interactable.Where(node => GlobalPosition.DistanceTo(node.GlobalPosition) <= _interactionRadius)
			.ToList();
	}

	public void SetAllyInDarkness()
	{
		// Berechne den Abstand zwischen Ally und Core
		Vector2 distance = this.Position - _core.Position;
		float distanceLength = distance.Length(); // Berechne die Länge des Vektors

		// If ally further away than big circle, he is in the darkness
		if (distanceLength > Core.LightRadiusBiggerCircle)
		{
			CurrentState = AllyState.Darkness;
		}
		//if ally not in darkness and closer than the small Light Radius, he is in small circle
		else if (distanceLength < Core.LightRadiusSmallerCircle)
		{
			CurrentState = AllyState.SmallCircle;
		}
		//if ally not in darkness and not in small circle, ally is in big circle
		else
		{
			CurrentState = AllyState.BigCircle;
		}

	}

	public override void _PhysicsProcess(double delta)
	{
		//Check where ally is (darkness, bigger, smaller)
		SetAllyInDarkness();

		UpdateTarget();

		_reached = GlobalPosition.DistanceTo(PathFindingMovement.TargetPosition) < 150;


		if (_harvest && _reached) // Harvest logic
		{
			Harvest();
		}
	}

	private void UpdateTarget()
	{
		if (_harvest)
		{
			if (_returning)
			{
				PointLight2D cl = _core.GetNode<PointLight2D>("CoreLight");
				Vector2 targ = new Vector2(0, 500); // cl.GlobalPosition;
													// Target = core
				PathFindingMovement.TargetPosition = _core.GlobalPosition;
				GD.Print("Target position (should be CORE): " + PathFindingMovement.TargetPosition.ToString());
			}
			else
			{
				Location nearestLocation = Map.GetNearestItemLocation(new Location(GlobalPosition))!;
				//GD.Print("going to nearest loc("+nearestLocation.X +", "+nearestLocation.Y+") from "+ GlobalPosition.X + " " + GlobalPosition.Y);    //Target = nearest item
				PathFindingMovement.TargetPosition = nearestLocation.ToVector2();

			}
		}
	}

	private void HandleResponse(string response)
	{
		GD.Print("DEBUG: HandleResponse");
		List<(string, string)> matches = ExtractRelevantLines(response);
		string richtext = "";
		foreach ((string op, string content) in matches)
		{
			const string part = "";
			richtext += FormatPart(part, op, content);

			// differentiate what to do based on command op
			switch (op)
			{
				case "MOTIVATION": // set motivation from output
					_motivation.SetMotivation(content.ToInt());
					break;
				case "INTERACT":
					SetInteractOnArrival(true);
					GD.Print("DEBUG: INTERACT Match");
					break;
				case "GOTO AND INTERACT":
					SetInteractOnArrival(true);
					Goto(content);
					break;
				case "GOTO":
					GD.Print("DEBUG: GOTO Match");
					Goto(content);
					break;
				case "HARVEST"
					when !_busy && Map.Items.Count > 0
					: // if harvest command and not walking somewhere and items on map
					GD.Print("harvesting");
					Harvest();
					break;
				case "STOP": // stop command stops ally from doing anything
					_harvest = false;
					_busy = false;
					break;
				default:
					GD.Print("DEBUG: NO MATCH FOR : "+op);
					break;
			}
		}

		_responseField.ParseBbcode(richtext); // formatted text into response field
	}

	private void SetInteractOnArrival(bool interactOnArrival)
	{
		GD.Print("DEBUG: SetInteractOnArrival: " + interactOnArrival);
		_interactOnArrival = interactOnArrival;
	}

	private void Goto(String content)
	{
		GD.Print("DEBUG: goto function called ");
		const string goToPattern = @"^\s*\(\s*(-?\d+)\s*,\s*(-?\d+)\s*\)\s*$";
		Match goToMatch = Regex.Match(content.Trim(), goToPattern);
		GD.Print("DEBUG: going to test goto");

		if (goToMatch.Success)
		{
			int x = int.Parse(goToMatch.Groups[1].Value), y = int.Parse(goToMatch.Groups[2].Value);
			GD.Print("DEBUG: going to "+new Vector2(x, y));
			GetNode<PathFindingMovement>("PathFindingMovement").GoTo(new Vector2(x, y));
		}
		else
		{
			GD.Print($"goto couldn't match the position, content was: '{content}'");
		}
	}

	private static string FormatPart(string part, string op, string content)
	{
		return part += op switch // format response based on different ops or response types
		{
			"THOUGHT" => "[i]" + content + "[/i]\n",
			"RESPONSE" or "COMMAND" or "STOP" => "[b]" + content + "[/b]\n",
			_ => content + "\n"
		};
	}

	private static List<(string, string)> ExtractRelevantLines(string response)
	{
		string[] lines = response.Split('\n').Where(line => line.Length > 0).ToArray();
		List<(string, string)> matches = [];

		// Add commands to be extracted here
		List<String> ops =
		[
			"MOTIVATION",
			"THOUGHT",
			"RESPONSE",
			"REMEMBER",
			"GOTO AND INTERACT",
			"GOTO",
			"INTERACT",
			"HARVEST",
			"FOLLOW",
			"STOP"
		];

		foreach (string line in lines)
		{
			foreach (string op in ops)
			{
				string pattern = op + @"[\s:]+.*"; // \b matcht eine Wortgrenze
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
				Match match = regex.Match(line);
				if (!match.Success)
				{
					continue;
				}

				matches.Add((op, match.Value[(op.Length + 1)..].Trim())); // Extract the operand
				break;
			}
		}

		response = "";
		return matches;
	}

	private void Harvest()
	{
		if (!_returning)
		{
			// extract the nearest item and add to inventory (pickup)
			if (SsInventory.HasSpace()) // if inventory has space
			{
				GD.Print("harvesting...");
				Itemstack item = Map.ExtractNearestItemAtLocation(new Location(GlobalPosition));
				GD.Print(item.Material + " amount: " + item.Amount);
				SsInventory.AddItem(item); // add item to inventory
				SsInventory.Print();
			} // if inventory has no space don't harvest it
			else
			{
				GD.Print("No space");
			}

			_returning = true;
		}
		else
		{
			// Empty inventory into the core

			foreach (Itemstack item in SsInventory.GetItems())
			{
				if (item.Material == Game.Scripts.Items.Material.None)
				{
					continue;
				}

				Core.IncreaseScale();
				GD.Print("Increased scale");
			}

			SsInventory.Clear();
			_busy = false; // Change busy state  
			_harvest = false; // Change harvest state
			_returning = false; // Change returning state
		}
	}
}
