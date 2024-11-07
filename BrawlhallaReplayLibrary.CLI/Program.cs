using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BrawlhallaReplayLibrary.CLI;

public class Program
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };
    static Program()
    {
        JSON_OPTIONS.Converters.Add(new JsonStringEnumConverter());
    }


    [DoesNotReturn]
    static void ExitWithMessage(string message, int exitCode = 1)
    {
        Console.WriteLine(message, exitCode == 0 ? Console.Out : Console.In);
        Environment.Exit(exitCode);
    }

    [DoesNotReturn]
    static void ShowHelp()
    {
        ExitWithMessage(
    @"Usage: replay-parser [mode]
modes:
    -E/--extract [path to replay file] <file path to create>
    -C/--create [path to json file] <file path to create>
    --help: show this message
"
        , 0);
    }

    public static void Main(string[] args)
    {

        if (args.Length == 0) ShowHelp();

        try
        {
            string mode = args[0];

            if (mode == "-E" || mode == "--extract")
            {
                if (args.Length < 2) ExitWithMessage($"Too many arguments to {mode}");
                if (args.Length > 3) ExitWithMessage($"Too many arguments to {mode}");
                string replayPath = args[1];
                if (!File.Exists(replayPath))
                    ExitWithMessage("Given replay path is invalid");

                string outputPath = args.Length == 3 ? args[2] : Path.ChangeExtension(replayPath, ".json");

                Replay replay = null!;
                try
                {
                    using FileStream file = File.OpenRead(replayPath);
                    replay = Replay.Load(file);
                }
                catch (Exception e)
                {
                    ExitWithMessage($"Error while parsing replay: {e}");
                }

                try
                {
                    using FileStream outFile = File.OpenWrite(outputPath);
                    JsonSerializer.Serialize(outFile, replay, JSON_OPTIONS);
                }
                catch (Exception e)
                {
                    ExitWithMessage($"Error writing replay: {e}");
                }
            }
            else if (mode == "-C" || mode == "--create")
            {
                if (args.Length < 2) ExitWithMessage($"Too many arguments to {mode}");
                if (args.Length > 3) ExitWithMessage($"Too many arguments to {mode}");
                string jsonPath = args[1];
                if (!File.Exists(jsonPath))
                    ExitWithMessage("Given file path is invalid");

                string outputPath = args.Length == 3 ? args[2] : Path.ChangeExtension(jsonPath, ".replay");

                Replay replay = null!;
                try
                {
                    using FileStream file = File.OpenRead(jsonPath);
                    Replay? replay_ = JsonSerializer.Deserialize<Replay>(file, JSON_OPTIONS);
                    if (replay_ is null)
                        ExitWithMessage("Failed to parse replay");
                    replay = replay_;
                }
                catch (Exception e)
                {
                    ExitWithMessage($"Error parsing json: {e}");
                }

                try
                {
                    using FileStream outFile = File.OpenWrite(outputPath);
                    replay.Save(outFile);
                }
                catch (Exception e)
                {
                    ExitWithMessage($"Error writing replay: {e}");
                }

            }
            else if (mode == "--help")
            {
                ShowHelp();
            }
            else
            {
                ExitWithMessage($"Invalid mode {mode}. Use --help to see a list of available modes.");
            }
        }
        catch (Exception e)
        {
            ExitWithMessage($"Unhandled error: {e}");
        }
    }
}