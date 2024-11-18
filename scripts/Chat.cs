using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading.Tasks;

using Game.Scripts.AI;

using Godot;
namespace Game.Scripts;

public partial class Chat : LineEdit
{
    [Export] private CharacterBody2D allie;
    [Export(PropertyHint.File, "*.txt")] private string? _systemPromptFile;
    [Export] RichTextLabel _richTextLabel = null!;
    private string? _systemPrompt;
    private GeminiService? _geminiService;
    private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
    private const string ChatPlaceholder = "Type here to chat";
    private const string EnterApiPlaceholder = "Enter API key";
    private const Boolean OutputOnlyResponse = true;
    private Boolean _synthesizeMode = true;
    private Boolean _completed = false, _synthesizing = false, _clearTextNext = false;
    private Task? _task;
    private String _allieresponse = "";
    private Boolean _redirectToAi = true;
    private const string PythonExePath = "C:/Users/leo/AppData/Local/Programs/Python/Python313/python.exe";
    private const string SynthesizeAudioPythonPath = "D:/CS Living Light/scripts/SynthesizeSpeech.py";

    private void DeleteLeftOvers()
    {
        string wavFilePath = "D:/CS Living Light/scripts/output.wav";
        string importFilePath = "D:/CS Living Light/scripts/output.wav.import";
        // Check if output.wav exists and delete it if it does
        if (File.Exists(wavFilePath))
        {
            File.Delete(wavFilePath);
            Console.WriteLine($"{wavFilePath} was deleted.");
        }
        else
        {
            Console.WriteLine($"{wavFilePath} does not exist.");
        }

        // Check if output.wav.import exists and delete it if it does
        if (File.Exists(importFilePath))
        {
            File.Delete(importFilePath);
            Console.WriteLine($"{importFilePath} was deleted.");
        }
        else
        {
            Console.WriteLine($"{importFilePath} does not exist.");
        }
    }


    static String ExtractTextResponse(string input)
    {
        // Ignoriert Groß-/Kleinschreibung
        int index = input.IndexOf("Entity", StringComparison.OrdinalIgnoreCase);

        // Extract the substring up to "Stat " and trim trailing whitespace
        string result = input.Substring(0, index).Trim();
        //  GD.Print($"Extracted Text: '{result}' Original Text: "+input);
        return result;
    }


    private void PrintResponse(string responsePiece)
    {
        _richTextLabel.Text = _richTextLabel.Text + responsePiece;
        if (_richTextLabel.Text.Contains("Entity") && !_synthesizing)
        {
            if (OutputOnlyResponse)
            {
                //GD.Print(_richTextLabel.Text+" was passed to extractor");
                _allieresponse = _richTextLabel.Text.Replace("Response: ", "").Trim().Replace("\"", "");
                _allieresponse = ExtractTextResponse(_allieresponse);
            }

            if (_synthesizeMode)
            {
                SynthAudio(_allieresponse);
                _richTextLabel.Clear();
                _allieresponse = "";
            }
        }
        responsePiece = "";
    }

    string _currentDirectory = Directory.GetCurrentDirectory();
    private string _scriptPath = PutInQuotes(SynthesizeAudioPythonPath);
    string
        _param2 = "en-US-TonyNeural",
        _param3 = "cheerful",
        _param4 = "high"; //string param5 = currentDirectory+"/output.wav";  // currently output.wav (rewrite python file to allow another arg and save to this arg)

    private string[] _voiceStyles = new[] { "cheerful", "sad", "angry", "excited", "friendly", "unfriendly", "hopeful", "terrified" };
    private static string PutInQuotes(string text)
    {
        return "\"" + text + "\"";
    }

    private void SynthAudio(string response)
    {
        DeleteLeftOvers();
        if (_synthesizing)
        {
            return;
        }
        _synthesizing = true;

        string param1 = PutInQuotes(response);
        Process process = new Process();
        process.StartInfo = new ProcessStartInfo(PutInQuotes(PythonExePath), SynthesizeAudioPythonPath)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            Arguments = $"{_scriptPath} \"{response}\" \"{_param2}\" \"{_param3}\" \"{_param4}\"",
        };
        string commandLine =
            _scriptPath + " " + param1 + " " + PutInQuotes(_param2) + " " + PutInQuotes(_param3) + " " +
            PutInQuotes(_param4);
        GD.Print("this would be called in terminal: python " + commandLine);
        try
        {
            process.Start(); // GD.Print("Python script started.");
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd(); // Optionally, wait for the process to finish and check the exit code
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                GD.Print("Python script exited with code:", process.ExitCode + ". With: " + output + " And " + error);
            }
            Task playTask = PlaySoundAndDeleteAsync(ProjectSettings.GlobalizePath("res://scripts/output.wav"));
            _synthesizing = false;
            _richTextLabel.Clear();
        }
        catch (Exception e)
        {
            GD.Print("Error starting Python script:", e.Message);
        }
    }
    static async Task PlaySoundAndDeleteAsync(string filePath)
    {

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Audio file not found", filePath);
        }

        await Task.Run(() =>
        {
            using (SoundPlayer soundPlayer = new SoundPlayer(filePath))
            {
                soundPlayer.PlaySync(); // Play the sound synchronously in a separate thread
            }

            // Delete the file after playback
            File.Delete(filePath);
        });
    }


    public override void _Ready()
    {
        string systemPromptAbsolutePath = ProjectSettings.GlobalizePath(_systemPromptFile);
        try
        {
            _systemPrompt = File.ReadAllText(systemPromptAbsolutePath);
        }
        catch (Exception ex)
        {
            GD.Print($"Failed to load systemPrompt. {ex.Message}");
            GD.Print(_systemPrompt);
        }
        InitializeGeminiService();
    }

    private void InitializeGeminiService()
    {
        try
        {
            _geminiService = new GeminiService(_apiKeyPath);

            if (_systemPrompt != null)
            {
                _geminiService.SetSystemPrompt(_systemPrompt);
            }

            PlaceholderText = ChatPlaceholder;
        }

        catch (Exception ex)
        {
            GD.Print(ex.Message);
            PlaceholderText = EnterApiPlaceholder;
        }
    }

    private async void OnTextSubmitted(String input)
    {
        GD.Print($"Submitted text: {input}");

        if (input.Equals("follow"))
        {
            GD.Print("switched follow mode");
            Node node = GetTree().Root.GetNode<CharacterBody2D>("TileMapLayer/Region/MeshInstance2D/allie1");
            GD.Print(node.Name);
            node.Call("SetFollowPlayer", true);
             _richTextLabel.Clear();
            _richTextLabel.Text = "switched follow mode to " + (string)node.Call("getFollowPlayer");
        }
        else if (input.Contains("style"))
        {
            input.Replace("style ", "");
            string[] words = input.Split(' ');
            _param3 = words[1];
            if (words.Length > 1)
            {
                input = string.Join(" ", words, 1, words.Length - 1);
                GD.Print("style: " + _param3 + " Text: " + input);
                words = new[] { "" };
            }
            else
            {
                input = "";
                DeleteLeftOvers();
                _richTextLabel.Clear();
            }
        }

        if (input.Contains("mode"))
        {
            String mode = input.Replace("mode ", "");
            string[] words = input.Split(' ');
            _param4 = words[1];
            if (words.Length > 1)
            {
                input = string.Join(" ", words, 2, words.Length - 1);
                GD.Print($"mode: {_param4} Text: {input}");
            }
            else
            {
                GD.Print("Error");
            }
        }

        if (_redirectToAi)
        {
            //String gameContext = "Game Context:\nThe player is currently not in any danger, low on resources, and at the beginning of the game. The allies are a bit suspicious and don’t always follow the commander’s commands. The ally is a funny and inspiring character.\nPlayer Input:  „";
            //  String gameContext = "";
            //  input = gameContext + input + "“\nAI Response (Always ends with
            //_END):\n";

            if (_geminiService != null)
            {
                _completed = false;
                _task = _geminiService.MakeQuerry(input, PrintResponse);
                // _geminiService.getChatHistory
                // await _task!;
            }
            else
            {
                await File.WriteAllTextAsync(_apiKeyPath, input.Trim());
                InitializeGeminiService();
                _richTextLabel.Text = "";
                Clear();
            }
        }
        Clear();
    }
}
