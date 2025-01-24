using System.Collections.Generic;

using Godot;

public partial class IntroScene : Control
{
    // Nodes
    private PanelContainer? panelContainer; // Container for holding the text box UI
    private Label? label; // Label for displaying dialog text
    private Camera2D? mainCamera; // Reference to the main camera
    private ColorRect? blackoutRect; // For the "waking up" effect (screen blackout)

    // Dialog text lines
    private readonly List<string> dialogLines = new List<string>
    {
        "Wake up!",
        "Please! Wake up!",
        "We need you... Wake up!",
        "You are finally awake! I was losing hope...",
        "Something terrible has happened! All the light has disappeared.",
        "The darkness is dangerous and harmful. We can’t endure it for long...",
        "You are our last hope!",
        "I sense that there is something in the nearby village...",
        "I know you can’t move outside the core...",
        "But your allies can be your hands, eyes, and ears in this world. You are connected.",
        "You may have forgotten everything, but they are your faithful companions and will listen to your command."
    };

    // Camera positions for specific lines
    private readonly Dictionary<int, Vector2> cameraPositions = new Dictionary<int, Vector2>
    {
        { 7, new Vector2(5207, -4350) }, // Camera moves to position 1 on the 7th line
		{ 9, new Vector2(0, 0) }         // Camera moves back to default on the 9th line
	};

    // Typing effect variables
    private int currentLineIndex = 0; // Index of the current dialog line
    private string currentText = ""; // Gradually built text for the typing effect
    private bool isTyping = false; // Indicates if the typing effect is in progress
    private Timer? typingTimer; // Timer to control the typing speed
    private float typingSpeed = 0.05f; // Seconds per character

    // "Waking up" effect state
    private int wakeUpState = 0; // 0 = fully black, 1 = partially visible, 2 = fully black again, 3 = fully visible

    // Additional variables for camera shake
    private Timer? shakeTimer; // Timer for controlling camera shake
    private int shakeDuration = 30; // Number of shake movements
    private float shakeIntensity = 50.0f; // Intensity of the shake

    public override void _Ready()
    {
        // Find the main camera
        mainCamera = GetNodeOrNull<Camera2D>("../Ally/Camera2D");
        if (mainCamera == null)
        {
            GD.PrintErr("Main camera not found!");
        }

        // Find the PanelContainer and Label for the text box
        panelContainer = GetNode<PanelContainer>("PanelContainer");
        if (panelContainer == null)
        {
            GD.PrintErr("PanelContainer not found!");
        }

        label = panelContainer?.GetNodeOrNull<Label>("Label");
        if (label == null)
        {
            GD.PrintErr("Label not found!");
        }

        // Find the ColorRect for the blackout effect
        blackoutRect = GetNode<ColorRect>("ColorRect");
        if (blackoutRect == null)
        {
            GD.PrintErr("Blackout ColorRect not found!");
        }
        else
        {
            blackoutRect.MouseFilter = Control.MouseFilterEnum.Ignore; // Ensure it does not block input
            blackoutRect.Color = new Color(0, 0, 0, 1); // Set to fully black
        }

        // Initialize the typing timer
        typingTimer = new Timer();
        typingTimer.WaitTime = typingSpeed;
        typingTimer.OneShot = false;
        typingTimer.Connect("timeout", new Callable(this, nameof(OnTypingTimerTimeout)));
        AddChild(typingTimer);

        // Initialize the shake timer
        shakeTimer = new Timer();
        shakeTimer.WaitTime = 0.1f; // Time between each shake movement
        shakeTimer.OneShot = false; // Repeat until shake is done
        shakeTimer.Connect("timeout", new Callable(this, nameof(OnShakeTimerTimeout)));
        AddChild(shakeTimer);

        // Pause the game initially
        GetTree().Paused = true;

        // Display the first line of dialog
        ShowCurrentLine();
    }

    public override void _Process(double delta)
    {
        // Adjust the position of the text box to follow the camera
        if (mainCamera != null && panelContainer != null)
        {
            Vector2 cameraScreenPosition = mainCamera.GlobalPosition;
            panelContainer.GlobalPosition = cameraScreenPosition + new Vector2(-900, 200); // Offset to position the text box
        }
    }

    private void ShowCurrentLine()
    {
        if (label != null)
        {
            currentText = ""; // Reset the text
            label.Text = ""; // Clear the label
            isTyping = true; // Activate the typing effect
            typingTimer?.Start(); // Start the timer

            // Change camera position if needed
            if (mainCamera != null && cameraPositions.ContainsKey(currentLineIndex))
            {
                mainCamera.Position = cameraPositions[currentLineIndex];
            }
        }
    }

    private void OnTypingTimerTimeout()
    {
        if (label != null)
        {
            string fullText = dialogLines[currentLineIndex];

            // Append the next character
            if (currentText.Length < fullText.Length)
            {
                currentText += fullText[currentText.Length];
                label.Text = currentText; // Update the label
            }
            else
            {
                // Stop the timer when the entire text is displayed
                typingTimer?.Stop();
                isTyping = false; // Deactivate the typing effect
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") ||
            (@event is InputEventMouseButton clickEvent && clickEvent.ButtonIndex == MouseButton.Left && clickEvent.Pressed))
        {
            if (wakeUpState == 0)
            {
                SetBlackoutAlpha(0.9f); // Adjust this value for partial visibility
                wakeUpState = 1;
                ShowNextLine();
                return;
            }
            else if (wakeUpState == 1)
            {
                SetBlackoutAlpha(1.0f); // Fully black
                wakeUpState = 2;
                ShowNextLine();
                return;
            }
            else if (wakeUpState == 2)
            {
                SetBlackoutAlpha(0.0f); // Fully visible
                wakeUpState = 3;

                // Start camera shake when waking up
                StartCameraShake();

                ShowNextLine();
                return;
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= dialogLines.Count) // If all lines have been displayed
        {
            FinishDialog();
        }
        else
        {
            ShowCurrentLine(); // Display the next line
        }
    }

    private void FinishDialog()
    {
        if (panelContainer != null)
        {
            panelContainer.Visible = false; // Hide the text box
        }

        // Reset the camera to follow the player
        if (mainCamera != null)
        {
            mainCamera.Position = mainCamera.GetParent<Node2D>().Position; // Reset to the parent node's position
            mainCamera.MakeCurrent(); // Reactivate camera follow
        }

        // Resume the game
        GetTree().Paused = false;
    }

    private void SetBlackoutAlpha(float alpha)
    {
        if (blackoutRect != null)
        {
            Color currentColor = blackoutRect.Color;
            blackoutRect.Color = new Color(currentColor.R8 / 255.0f, currentColor.G8 / 255.0f, currentColor.B8 / 255.0f, alpha); // Create a new color with updated alpha
        }
    }

    private void StartCameraShake()
    {
        // Reset shake parameters and start the timer
        shakeDuration = 10; // Reset the shake duration
        shakeIntensity = 20.0f; // Adjust intensity if needed
        shakeTimer?.Start();
    }

    private void OnShakeTimerTimeout()
    {
        if (mainCamera != null && shakeDuration > 0)
        {
            // Apply a small random offset to the camera position
            Vector2 randomOffset = new Vector2(
                GD.Randf() * shakeIntensity - shakeIntensity / 2,
                GD.Randf() * shakeIntensity - shakeIntensity / 2
            );
            mainCamera.Offset = randomOffset;

            shakeDuration--;
        }
        else
        {
            // Stop shaking and reset camera offset
            shakeTimer.Stop();
            if (mainCamera != null)
            {
                mainCamera.Offset = Vector2.Zero;
            }
        }
    }
}
