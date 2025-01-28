using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenerativeAI.Methods;
using GenerativeAI.Models;
using GenerativeAI.Types;

using Godot;
using System.Collections.Generic;
using System.Threading;

using GenerativeAI.Exceptions;

namespace Game.Scripts.AI;

public partial class GeminiService : Node // Wichtig: Erbt von Node für Signale
{
    [Signal]
    public delegate void GeminiResponseReceivedEventHandler(string response); // Signal-Definition

    public bool Busy = false; // Nicht mehr wirklich benötigt, aber belassen
    private readonly GenerativeModel _model;
    public ChatSession Chat;
    Queue<string> promptQueue = new Queue<string>();
    private bool _isProcessingQueue = false;
    private CancellationTokenSource _queueProcessingCancellationTokenSource;


    public GeminiService(string apiKeyFilePath, string systemPrompt)
    {
        if (string.IsNullOrEmpty(apiKeyFilePath))
        {
            throw new ArgumentNullException(nameof(apiKeyFilePath), "API key file path cannot be null or empty");
        }

        if (!File.Exists(apiKeyFilePath))
        {
            throw new FileNotFoundException($"API key file not found at path: {apiKeyFilePath}");
        }

        try
        {
            // Read the API key from the file, trimming any whitespace
            string apiKey = File.ReadAllText(apiKeyFilePath).Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key file is empty");
            }

            _model = new GenerativeModel(apiKey);

            _model.SafetySettings =
            [
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_HARASSMENT, Threshold = HarmBlockThreshold.BLOCK_NONE
                },
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH, Threshold = HarmBlockThreshold.BLOCK_NONE
                },
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
                    Threshold = HarmBlockThreshold.BLOCK_NONE
                },
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
                    Threshold = HarmBlockThreshold.BLOCK_NONE
                }
            ];

            Chat = _model.StartChat(new StartChatParams());

            // Send system prompt as the first message in the chat history
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                Task.Run(async () => await Chat.SendMessageAsync(systemPrompt))
                    .Wait(); // Run synchronously for initialization. Be cautious in real async scenarios.
            }
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentNullException)
        {
            throw new Exception($"Error reading API key from file: {ex.Message}", ex);
        }

        // Starte den Queue-Verarbeitungsprozess im Hintergrund beim Erstellen des Service
        StartQueueProcessing();
    }


    private async Task ProcessPrompt(string prompt) // Ausgelagert für Übersichtlichkeit
    {
        GD.Print($"Verarbeite Prompt aus Queue: {prompt}");
        
        if (Chat == null) // Null-Prüfung HIER einfügen
        {
            GD.PrintErr("FEHLER: Chat-Objekt ist NULL, bevor SendMessageAsync aufgerufen wird!");
            return; // Beende die Methode, um weitere Fehler zu vermeiden
        }
        
        try
        {
            string? result = await Chat.SendMessageAsync(prompt);
            if (result != null)
            {
                GD.Print($"Gemini Antwort: {result}");
                // *** HIER SIGNAL AUSLÖSEN ***
                EmitSignal(SignalName.GeminiResponseReceived, result); // Signal mit der Antwort auslösen!
                
                // anstatt Signal, rufe hier die HandleResponse Funktion vom Ally direkt auf nachdem async queue abgearbeitet wurde
                Ally ally = GetParent().GetParent().GetParent<Ally>();
                ally.HandleResponse(result);
            }
            else
            {
                GD.PrintErr($"Gemini hat nicht geantwortet auf Prompt: {prompt}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Fehler bei Gemini Anfrage für Prompt '{prompt}': {ex.Message}");
            promptQueue.Enqueue(prompt);
            await Task.Delay(2000);
        }
    }


    private void StartQueueProcessing()
    {
        if (_isProcessingQueue) return;

        _isProcessingQueue = true;
        _queueProcessingCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _queueProcessingCancellationTokenSource.Token;

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (promptQueue.Count > 0)
                {
                    string prompt = promptQueue.Dequeue();
                    await ProcessPrompt(prompt); // Nutze die ausgelagerte ProcessPrompt Methode
                }
                else
                {
                    await Task.Delay(100);
                }
            }

            GD.Print("Queue Verarbeitungsprozess beendet.");
            _isProcessingQueue = false;
        }, cancellationToken);
    }

    public void StopQueueProcessing()
    {
        if (_isProcessingQueue)
        {
            _queueProcessingCancellationTokenSource?.Cancel();
            _queueProcessingCancellationTokenSource?.Dispose();
            _queueProcessingCancellationTokenSource = null;
        }
    }

    public async Task<string?>
        MakeQuery(string input) // MakeQuery gibt jetzt Task<string?> zurück (Task, der evtl. null String liefert)
    {
        promptQueue.Enqueue(input);
        return null; // MakeQuery gibt *sofort* null zurück, da die Antwort asynchron kommt
    }

    public IReadOnlyList<Content> GetChatHistory()
    {
        return Chat.History;
    }

    public void ClearChat()
    {
        Chat = _model.StartChat(new StartChatParams());
    }

    public override void _ExitTree()
    {
        StopQueueProcessing();
    }
}