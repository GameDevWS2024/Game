using Godot;
using System.Collections.Generic;

public partial class IntroScene : Control
{
	// Nodes
	private PanelContainer? panelContainer;
	private Label? label;
	private Camera2D? mainCamera; // Referenz zur Kamera in der Hauptszene

	// Dialogtexte
	private List<string> dialogLines = new List<string>
	{
		"Willkommen in der Welt!",
		"Schau dir das Dorf an.",
		"Dies ist der Anfang deines Abenteuers.",
		"Jetzt geht es zurück zur Basis.",
		"Viel Glück!"
	};

	// Kamera-Positionen für bestimmte Zeilen
	private Dictionary<int, Vector2> cameraPositions = new Dictionary<int, Vector2>
	{
		{ 1, new Vector2(5207, -4350) }, // Kamera springt zu Position 1
		{ 3, new Vector2(0, 0) }      // Kamera springt zurück
	};

	// Schreibmaschinen-Effekt Variablen
	private int currentLineIndex = 0;
	private string currentText = ""; // Der schrittweise aufgebaute Text
	private bool isTyping = false; // Gibt an, ob der Schreib-Effekt gerade läuft
	private Timer? typingTimer;
	private float typingSpeed = 0.05f; // Sekunden pro Buchstabe

	public override void _Ready()
	{
		// Kamera suchen
		mainCamera = GetNodeOrNull<Camera2D>("../Ally/Camera2D");
		if (mainCamera == null)
		{
			GD.PrintErr("Kamera wurde nicht gefunden!");
		}
		else
		{
			GD.Print("Kamera erfolgreich gefunden: " + mainCamera.Name);
		}

		// PanelContainer und Label suchen
		panelContainer = GetNode<PanelContainer>("PanelContainer");
		if (panelContainer == null)
		{
			GD.PrintErr("PanelContainer wurde nicht gefunden!");
		}

		label = panelContainer?.GetNodeOrNull<Label>("Label");
		if (label == null)
		{
			GD.PrintErr("Label wurde nicht gefunden!");
		}

		// Timer initialisieren
		typingTimer = new Timer();
		typingTimer.WaitTime = typingSpeed;
		typingTimer.OneShot = false;
		typingTimer.Connect("timeout", new Callable(this, nameof(OnTypingTimerTimeout)));
		AddChild(typingTimer);

		// Spiel pausieren
		GetTree().Paused = true;

		// Zeige den ersten Text
		ShowCurrentLine();
	}

	public override void _Process(double delta)
	{
		// Wenn die Kamera existiert, passe die Position der Textbox an
		if (mainCamera != null && panelContainer != null)
		{
			// Setze die Textbox relativ zur Kamera
			Vector2 cameraScreenPosition = mainCamera.GlobalPosition;
			panelContainer.GlobalPosition = cameraScreenPosition + new Vector2(-900, 200); // Offset nach unten
		}
	}

	private void ShowCurrentLine()
	{
		if (label != null)
		{
			currentText = ""; // Text zurücksetzen
			label.Text = ""; // Label leeren
			isTyping = true; // Schreib-Effekt aktivieren
			typingTimer?.Start(); // Timer starten

			// Kamera-Position ändern
			if (mainCamera != null && cameraPositions.ContainsKey(currentLineIndex))
			{
				mainCamera.Position = cameraPositions[currentLineIndex];
				GD.Print($"Kamera bewegt zu Position: {cameraPositions[currentLineIndex]}");

				// Kamera wieder aktivieren, wenn der Dialog endet
				if (currentLineIndex == dialogLines.Count - 1)
				{
					mainCamera.MakeCurrent();
				}
			}
		}
	}

	private void OnTypingTimerTimeout()
	{
		if (label != null)
		{
			string fullText = dialogLines[currentLineIndex];

			// Füge den nächsten Buchstaben hinzu
			if (currentText.Length < fullText.Length)
			{
				currentText += fullText[currentText.Length];
				label.Text = currentText; // Aktualisiere das Label
			}
			else
			{
				// Stoppe den Timer, wenn der gesamte Text angezeigt wurde
				typingTimer?.Stop();
				isTyping = false; // Schreib-Effekt deaktivieren
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Eingaben ignorieren, solange der Schreib-Effekt aktiv ist
		if (isTyping)
		{
			if (@event.IsActionPressed("ui_accept") ||
				(@event is InputEventMouseButton mouseClick && mouseClick.ButtonIndex == MouseButton.Left && mouseClick.Pressed))
			{
				if (label != null)
				{
					currentText = dialogLines[currentLineIndex]; // Setze den gesamten Text
					label.Text = currentText;
				}
				typingTimer?.Stop(); // Timer stoppen
				isTyping = false; // Schreib-Effekt deaktivieren
			}
			return;
		}

		// Nächste Zeile anzeigen, wenn der Text vollständig ist
		if (@event.IsActionPressed("ui_accept") ||
			(@event is InputEventMouseButton clickEvent && clickEvent.ButtonIndex == MouseButton.Left && clickEvent.Pressed))
		{
			ShowNextLine();
		}
	}

	private void ShowNextLine()
	{
		currentLineIndex++;

		if (currentLineIndex >= dialogLines.Count) // Wenn alle Zeilen angezeigt wurden
		{
			FinishDialog();
		}
		else
		{
			ShowCurrentLine(); // Nächste Zeile anzeigen
		}
	}

	private void FinishDialog()
	{
		if (panelContainer != null)
		{
			panelContainer.Visible = false; // Panel ausblenden
		}

		// Kamera zurücksetzen, damit sie wieder dem Spieler folgt
		if (mainCamera != null)
		{
			mainCamera.Position = mainCamera.GetParent<Node2D>().Position; // Kamera zur Position ihres Parent-Knotens setzen
			mainCamera.MakeCurrent(); // Kamera-Follow wieder aktivieren
		}

		// Spiel fortsetzen
		GetTree().Paused = false;
	}
}
