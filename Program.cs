using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileChronicle;

internal class Program
{
    private static Configuration? _config;
    private static MacroManager? _macroManager;

    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        _config = Configuration.Load();
        _macroManager = MacroManager.Load();

        if (args.Length == 0)
        {
            return await RunInteractiveMode();
        }

        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "snapshot" => await RunSnapshotAsync(args),
            "diff" => await RunDiffAsync(args),
            "compare" => await RunCompareAsync(args),
            "watch" => await RunWatchAsync(args),
            "interactive" or "i" => await RunInteractiveMode(),
            "config" => RunConfig(args),
            "macro" => RunMacro(args),
            "help" or "--help" or "-h" => ShowHelpReturn(),
            _ => ShowHelpReturn()
        };
    }

    private static int ShowHelpReturn()
    {
        ShowHelp();
        return 1;
    }

    private static void ShowHelp()
    {
        WriteColorLine("""
        ╔══════════════════════════════════════════════════════════════╗
        ║  FileChronicle - Advanced Directory Snapshot & Diff Tool    ║
        ╚══════════════════════════════════════════════════════════════╝
        """, ConsoleColor.Cyan);

        Console.WriteLine("""
        
        Commands:
          snapshot <directory> <output.json> [options]
              Create a snapshot of a directory
              Options:
                --include <pattern>    Include only matching files (e.g., *.cs)
                --exclude <pattern>    Exclude matching files (e.g., *.tmp)
                --format <json|csv>    Output format (default: json)
                --no-hash             Skip hash computation (faster)
                --progress            Show progress during operation

          diff <oldSnapshot.json> <newSnapshot.json> [options]
              Compare two snapshots
              Options:
                --output <file>       Export diff to file
                --format <json|csv|html>  Output format
                --show-unchanged      Include unchanged files
                --detailed            Show detailed file information

          compare <dir1> <dir2> [options]
              Directly compare two directories
              Options:
                --include <pattern>
                --exclude <pattern>
                --detailed
                --progress

          watch <directory> [options]
              Monitor directory for changes in real-time
              Options:
                --include <pattern>
                --exclude <pattern>
                --interval <seconds>  Check interval (default: 5)

          interactive (or i)
              Launch interactive mode with control modules

          macro [save|list|run|delete]
              Manage command macros (shortcuts)
              Examples:
                macro save mysnap "snapshot C:\Projects snapshots\latest.json"
                macro list
                macro run mysnap
                macro delete mysnap

          config [list|set|reset]
              Manage configuration
              Examples:
                config list
                config set defaultFormat html
                config set defaultWatchInterval 10
                config set coloredOutput true
                config reset

          help
              Show this help message

        Examples:
          FileChronicle snapshot C:\Projects output.json
          FileChronicle snapshot C:\Projects output.json --exclude *.tmp --exclude bin\**
          FileChronicle diff old.json new.json --detailed --format html
          FileChronicle compare C:\Projects\v1 C:\Projects\v2
          FileChronicle watch C:\Projects --interval 10
          FileChronicle interactive
        """);
    }

    private static async Task<int> RunInteractiveMode()
    {
        WriteColorLine("\n╔══════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
        WriteColorLine("║        FileChronicle - Interactive Mode                      ║", ConsoleColor.Cyan);
        WriteColorLine("╚══════════════════════════════════════════════════════════════╝\n", ConsoleColor.Cyan);
        
        Console.WriteLine("Interactive Mode with Control Modules");
        Console.WriteLine("Type 'help' for commands, 'macros' to see saved macros, 'exit' to quit.\n");

        // Show quick access macros if available
        var macros = _macroManager?.GetAllMacros();
        if (macros?.Any() == true)
        {
            WriteColorLine("📌 Quick Access Macros:", ConsoleColor.Cyan);
            foreach (var macro in macros.Take(5))
            {
                Console.WriteLine($"   @{macro.Key} → {macro.Value}");
            }
            if (macros.Count > 5)
                Console.WriteLine($"   ... and {macros.Count - 5} more (type 'macros' to see all)");
            Console.WriteLine();
        }

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            WriteColorLine("\n\nOperation cancelled. Type 'exit' to quit.", ConsoleColor.Yellow);
        };

        var interactiveContext = new InteractiveContext();

        while (!cts.Token.IsCancellationRequested)
        {
            WriteColor("FileChronicle> ", ConsoleColor.Green);
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                WriteColorLine("\nGoodbye!", ConsoleColor.Cyan);
                return 0;
            }

            // Check for macro execution (starts with @)
            if (input.StartsWith("@"))
            {
                var macroName = input[1..];
                if (_macroManager != null && _macroManager.TryGetMacro(macroName, out var macroCommand))
                {
                    WriteColorLine($"→ Executing macro: {macroCommand}", ConsoleColor.DarkGray);
                    input = macroCommand;
                }
                else
                {
                    WriteColorLine($"❌ Macro not found: {macroName}", ConsoleColor.Red);
                    WriteColorLine("   Use 'macros' to list all available macros.", ConsoleColor.Gray);
                    Console.WriteLine();
                    continue;
                }
            }

            // Check for special interactive commands
            if (input.Equals("macros", StringComparison.OrdinalIgnoreCase))
            {
                ShowMacros();
                Console.WriteLine();
                continue;
            }

            if (input.Equals("context", StringComparison.OrdinalIgnoreCase))
            {
                interactiveContext.Display();
                Console.WriteLine();
                continue;
            }

            if (input.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
            {
                HandleContextSet(input, interactiveContext);
                Console.WriteLine();
                continue;
            }

            // Expand context variables in input
            input = interactiveContext.ExpandVariables(input);

            var args = ParseCommandLine(input);
            if (args.Length == 0)
                continue;

            try
            {
                var command = args[0].ToLowerInvariant();
                var result = command switch
                {
                    "snapshot" => await RunSnapshotAsync(args),
                    "diff" => await RunDiffAsync(args),
                    "compare" => await RunCompareAsync(args),
                    "watch" => await RunWatchAsync(args),
                    "macro" => RunMacro(args),
                    "config" => RunConfig(args),
                    "help" => ShowHelpReturn(),
                    "clear" or "cls" => ClearScreen(),
                    _ => UnknownCommand(command)
                };

                if (command != "watch")
                {
                    Console.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                WriteColorLine("\n❌ Operation cancelled.", ConsoleColor.Yellow);
                Console.WriteLine();
                cts = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                WriteColorLine($"\n❌ Error: {ex.Message}", ConsoleColor.Red);
                if (ex.InnerException != null)
                    WriteColorLine($"   Details: {ex.InnerException.Message}", ConsoleColor.Red);
                Console.WriteLine();
            }
        }

        return 0;
    }

    private static void ShowMacros()
    {
        WriteColorLine("\n📌 Saved Macros:", ConsoleColor.Cyan);
        var macros = _macroManager?.GetAllMacros();
        
        if (macros?.Any() != true)
        {
            Console.WriteLine("   No macros saved yet.");
            Console.WriteLine("\n   Create a macro with:");
            Console.WriteLine("   macro save <name> \"<command>\"");
            return;
        }

        foreach (var macro in macros)
        {
            WriteColorLine($"   @{macro.Key}", ConsoleColor.Green);
            Console.WriteLine($"      → {macro.Value}");
        }

        Console.WriteLine($"\n   Total: {macros.Count} macro(s)");
        Console.WriteLine("   Execute with: @<name>");
    }

    private static void HandleContextSet(string input, InteractiveContext context)
    {
        var parts = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            WriteColorLine("❌ Usage: set <variable> <value>", ConsoleColor.Red);
            Console.WriteLine("   Example: set dir C:\\Projects");
            return;
        }

        var variable = parts[1];
        var value = parts[2].Trim('"');
        
        context.SetVariable(variable, value);
        WriteColorLine($"✅ Set ${variable} = {value}", ConsoleColor.Green);
    }

    private static int RunMacro(string[] args)
    {
        if (_macroManager == null)
        {
            WriteColorLine("❌ Macro manager not initialized.", ConsoleColor.Red);
            return 1;
        }

        if (args.Length < 2)
        {
            ShowMacros();
            return 0;
        }

        var subCommand = args[1].ToLowerInvariant();

        switch (subCommand)
        {
            case "save":
                if (args.Length < 4)
                {
                    WriteColorLine("❌ Usage: macro save <name> \"<command>\"", ConsoleColor.Red);
                    Console.WriteLine("   Example: macro save mysnap \"snapshot C:\\Projects output.json\"");
                    return 1;
                }
                var macroName = args[2];
                var macroCommand = args[3];
                _macroManager.SaveMacro(macroName, macroCommand);
                _macroManager.Save();
                WriteColorLine($"✅ Macro '@{macroName}' saved successfully.", ConsoleColor.Green);
                break;

            case "list":
                ShowMacros();
                break;

            case "run":
                if (args.Length < 3)
                {
                    WriteColorLine("❌ Usage: macro run <name>", ConsoleColor.Red);
                    return 1;
                }
                var runName = args[2];
                if (_macroManager.TryGetMacro(runName, out var command))
                {
                    WriteColorLine($"→ Executing: {command}", ConsoleColor.DarkGray);
                    WriteColorLine("   Note: Run this in interactive mode for full execution.", ConsoleColor.Yellow);
                }
                else
                {
                    WriteColorLine($"❌ Macro not found: {runName}", ConsoleColor.Red);
                    return 1;
                }
                break;

            case "delete":
                if (args.Length < 3)
                {
                    WriteColorLine("❌ Usage: macro delete <name>", ConsoleColor.Red);
                    return 1;
                }
                var deleteName = args[2];
                if (_macroManager.DeleteMacro(deleteName))
                {
                    _macroManager.Save();
                    WriteColorLine($"✅ Macro '@{deleteName}' deleted.", ConsoleColor.Green);
                }
                else
                {
                    WriteColorLine($"❌ Macro not found: {deleteName}", ConsoleColor.Red);
                    return 1;
                }
                break;

            default:
                WriteColorLine($"❌ Unknown macro command: {subCommand}", ConsoleColor.Red);
                Console.WriteLine("   Available: save, list, run, delete");
                return 1;
        }

        return 0;
    }

    private static int ClearScreen()
    {
        Console.Clear();
        return 0;
    }

    private static int UnknownCommand(string command)
    {
        WriteColorLine($"❌ Unknown command: {command}", ConsoleColor.Red);
        Console.WriteLine("Type 'help' for available commands.");
        return 1;
    }

    private static string[] ParseCommandLine(string input)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else
            {
                currentToken.Append(c);
            }
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens.ToArray();
    }

    private static async Task<int> RunSnapshotAsync(string[] args)
    {
        if (args.Length < 3)
        {
            WriteColorLine("❌ Error: snapshot requires <directory> and <output.json>.", ConsoleColor.Red);
            return 1;
        }

        var directory = args[1];
        var output = args[2];

        var options = ParseSnapshotOptions(args);
        ApplyConfigDefaults(options);

        if (!Directory.Exists(directory))
        {
            WriteColorLine("❌ Directory does not exist.", ConsoleColor.Red);
            return 1;
        }

        // Validate output path
        var outputDir = Path.GetDirectoryName(Path.GetFullPath(output));
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            try
            {
                Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                WriteColorLine($"❌ Cannot create output directory: {ex.Message}", ConsoleColor.Red);
                return 1;
            }
        }

        WriteColorLine($"\n📸 Creating snapshot for: {directory}", ConsoleColor.Cyan);
        if (options.IncludePatterns.Any())
            Console.WriteLine($"   Include: {string.Join(", ", options.IncludePatterns)}");
        if (options.ExcludePatterns.Any())
            Console.WriteLine($"   Exclude: {string.Join(", ", options.ExcludePatterns)}");
        Console.WriteLine();

        try
        {
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            var entries = new List<FileEntry>();
            int processed = 0;
            int skipped = 0;
            var totalFiles = files.Length;

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            foreach (var file in files)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    WriteColorLine("\n\n❌ Operation cancelled by user.", ConsoleColor.Yellow);
                    return 1;
                }

                try
                {
                    var relativePath = Path.GetRelativePath(directory, file);

                    if (!ShouldIncludeFile(relativePath, options))
                    {
                        skipped++;
                        continue;
                    }

                    var info = new FileInfo(file);
                    string? hash = null;

                    if (!options.NoHash)
                    {
                        hash = await ComputeSha256Async(file, cts.Token);
                    }

                    entries.Add(new FileEntry
                    {
                        RelativePath = NormalizePath(relativePath),
                        Length = info.Length,
                        LastWriteUtc = info.LastWriteTimeUtc,
                        Sha256 = hash ?? string.Empty
                    });

                    processed++;

                    if (options.ShowProgress)
                    {
                        WriteColorLine($"  ✓ [{processed + skipped}/{totalFiles}] {relativePath}", ConsoleColor.Gray);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    WriteColorLine($"  ⚠ Access denied: {file}", ConsoleColor.Yellow);
                    skipped++;
                }
                catch (Exception ex)
                {
                    WriteColorLine($"  ⚠ Failed to process {file}: {ex.Message}", ConsoleColor.Yellow);
                    skipped++;
                }
            }

            var snapshot = new Snapshot
            {
                CreatedAtUtc = DateTime.UtcNow,
                RootDirectory = Path.GetFullPath(directory),
                Files = entries,
                Options = options
            };

            await WriteOutputAsync(output, snapshot, options.Format);

            Console.WriteLine();
            WriteColorLine($"✅ Snapshot written to: {output}", ConsoleColor.Green);
            Console.WriteLine($"   Files tracked : {entries.Count}");
            Console.WriteLine($"   Files skipped : {skipped}");
            Console.WriteLine($"   Total size    : {FormatBytes(entries.Sum(e => e.Length))}");

            return 0;
        }
        catch (OperationCanceledException)
        {
            WriteColorLine("\n\n❌ Operation cancelled.", ConsoleColor.Yellow);
            return 1;
        }
        catch (Exception ex)
        {
            WriteColorLine($"\n❌ Error creating snapshot: {ex.Message}", ConsoleColor.Red);
            return 1;
        }
    }

    private static async Task<int> RunDiffAsync(string[] args)
    {
        if (args.Length < 3)
        {
            WriteColorLine("❌ Error: diff requires <oldSnapshot.json> and <newSnapshot.json>.", ConsoleColor.Red);
            return 1;
        }

        var oldPath = args[1];
        var newPath = args[2];

        var options = ParseDiffOptions(args);

        if (!File.Exists(oldPath) || !File.Exists(newPath))
        {
            WriteColorLine("❌ Snapshot file(s) not found.", ConsoleColor.Red);
            return 1;
        }

        try
        {
            var oldJson = await File.ReadAllTextAsync(oldPath);
            var newJson = await File.ReadAllTextAsync(newPath);

            var oldSnap = JsonSerializer.Deserialize<Snapshot>(oldJson);
            var newSnap = JsonSerializer.Deserialize<Snapshot>(newJson);

            if (oldSnap is null || newSnap is null)
            {
                WriteColorLine("❌ Failed to deserialize snapshot(s).", ConsoleColor.Red);
                return 1;
            }

            var diff = ComputeDiff(oldSnap, newSnap, options);

            DisplayDiff(diff, oldPath, newPath, options);

            if (!string.IsNullOrEmpty(options.OutputFile))
            {
                await ExportDiffAsync(diff, options.OutputFile, options.Format);
                WriteColorLine($"\n✅ Diff exported to: {options.OutputFile}", ConsoleColor.Green);
            }

            return 0;
        }
        catch (JsonException ex)
        {
            WriteColorLine($"❌ Invalid JSON in snapshot file: {ex.Message}", ConsoleColor.Red);
            return 1;
        }
        catch (Exception ex)
        {
            WriteColorLine($"❌ Error processing diff: {ex.Message}", ConsoleColor.Red);
            return 1;
        }
    }

    private static async Task<int> RunCompareAsync(string[] args)
    {
        if (args.Length < 3)
        {
            WriteColorLine("❌ Error: compare requires <dir1> and <dir2>.", ConsoleColor.Red);
            return 1;
        }

        var dir1 = args[1];
        var dir2 = args[2];

        if (!Directory.Exists(dir1) || !Directory.Exists(dir2))
        {
            WriteColorLine("❌ One or both directories do not exist.", ConsoleColor.Red);
            return 1;
        }

        var options = ParseSnapshotOptions(args);
        ApplyConfigDefaults(options);

        WriteColorLine($"\n🔍 Comparing directories:", ConsoleColor.Cyan);
        Console.WriteLine($"   Dir 1: {dir1}");
        Console.WriteLine($"   Dir 2: {dir2}\n");

        try
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            var snap1 = await CreateSnapshotAsync(dir1, options, cts.Token);
            var snap2 = await CreateSnapshotAsync(dir2, options, cts.Token);

            var diffOptions = new DiffOptions { Detailed = args.Contains("--detailed") };
            var diff = ComputeDiff(snap1, snap2, diffOptions);

            DisplayDiff(diff, dir1, dir2, diffOptions);

            return 0;
        }
        catch (OperationCanceledException)
        {
            WriteColorLine("\n\n❌ Operation cancelled.", ConsoleColor.Yellow);
            return 1;
        }
        catch (Exception ex)
        {
            WriteColorLine($"❌ Error comparing directories: {ex.Message}", ConsoleColor.Red);
            return 1;
        }
    }

    private static async Task<int> RunWatchAsync(string[] args)
    {
        if (args.Length < 2)
        {
            WriteColorLine("❌ Error: watch requires <directory>.", ConsoleColor.Red);
            return 1;
        }

        var directory = args[1];

        if (!Directory.Exists(directory))
        {
            WriteColorLine("❌ Directory does not exist.", ConsoleColor.Red);
            return 1;
        }

        var options = ParseSnapshotOptions(args);
        ApplyConfigDefaults(options);
        int interval = _config?.DefaultWatchInterval ?? 5;

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "--interval" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out var parsedInterval))
                    interval = parsedInterval;
            }
        }

        WriteColorLine($"\n👁 Watching directory: {directory}", ConsoleColor.Cyan);
        Console.WriteLine($"   Check interval: {interval} seconds");
        Console.WriteLine("   Press Ctrl+C to stop\n");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Snapshot? previousSnapshot = null;

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var currentSnapshot = await CreateSnapshotAsync(directory, options, cts.Token);

                if (previousSnapshot != null)
                {
                    var diffOptions = new DiffOptions { Detailed = false };
                    var diff = ComputeDiff(previousSnapshot, currentSnapshot, diffOptions);

                    if (diff.Added.Any() || diff.Removed.Any() || diff.Changed.Any())
                    {
                        var timestamp = DateTime.Now.ToString("HH:mm:ss");
                        WriteColorLine($"\n[{timestamp}] Changes detected:", ConsoleColor.Yellow);

                        foreach (var file in diff.Added)
                            WriteColorLine($"  + {file.RelativePath}", ConsoleColor.Green);

                        foreach (var file in diff.Removed)
                            WriteColorLine($"  - {file.RelativePath}", ConsoleColor.Red);

                        foreach (var pair in diff.Changed)
                            WriteColorLine($"  * {pair.New.RelativePath}", ConsoleColor.Yellow);
                    }
                }

                previousSnapshot = currentSnapshot;
                await Task.Delay(TimeSpan.FromSeconds(interval), cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            WriteColorLine("\n\n✅ Watch mode stopped.", ConsoleColor.Cyan);
        }
        catch (Exception ex)
        {
            WriteColorLine($"\n❌ Error in watch mode: {ex.Message}", ConsoleColor.Red);
            return 1;
        }

        return 0;
    }

    private static int RunConfig(string[] args)
    {
        var config = Configuration.Load();

        if (args.Length < 2)
        {
            config.Display();
            return 0;
        }

        var subCommand = args[1].ToLowerInvariant();

        switch (subCommand)
        {
            case "list":
                config.Display();
                break;

            case "set":
                if (args.Length < 4)
                {
                    WriteColorLine("❌ Usage: config set <key> <value>", ConsoleColor.Red);
                    return 1;
                }
                try
                {
                    config.Set(args[2], args[3]);
                    config.Save();
                    WriteColorLine($"✅ Configuration updated: {args[2]} = {args[3]}", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    WriteColorLine($"❌ {ex.Message}", ConsoleColor.Red);
                    return 1;
                }
                break;

            case "reset":
                config = new Configuration();
                config.Save();
                WriteColorLine("✅ Configuration reset to defaults.", ConsoleColor.Green);
                break;

            default:
                WriteColorLine($"❌ Unknown config command: {subCommand}", ConsoleColor.Red);
                return 1;
        }

        return 0;
    }

    private static void ApplyConfigDefaults(SnapshotOptions options)
    {
        if (_config == null)
            return;

        // Apply default excludes if no patterns specified
        if (!options.ExcludePatterns.Any() && _config.DefaultExcludePatterns.Any())
        {
            options.ExcludePatterns.AddRange(_config.DefaultExcludePatterns);
        }

        // Apply default format if not specified
        if (options.Format == "json" && !string.IsNullOrEmpty(_config.DefaultFormat))
        {
            options.Format = _config.DefaultFormat;
        }
    }

    private static SnapshotOptions ParseSnapshotOptions(string[] args)
    {
        var options = new SnapshotOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--include":
                    if (i + 1 < args.Length)
                        options.IncludePatterns.Add(args[++i]);
                    break;

                case "--exclude":
                    if (i + 1 < args.Length)
                        options.ExcludePatterns.Add(args[++i]);
                    break;

                case "--format":
                    if (i + 1 < args.Length)
                        options.Format = args[++i].ToLowerInvariant();
                    break;

                case "--no-hash":
                    options.NoHash = true;
                    break;

                case "--progress":
                    options.ShowProgress = true;
                    break;
            }
        }

        return options;
    }

    private static DiffOptions ParseDiffOptions(string[] args)
    {
        var options = new DiffOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--output":
                    if (i + 1 < args.Length)
                        options.OutputFile = args[++i];
                    break;

                case "--format":
                    if (i + 1 < args.Length)
                        options.Format = args[++i].ToLowerInvariant();
                    break;

                case "--show-unchanged":
                    options.ShowUnchanged = true;
                    break;

                case "--detailed":
                    options.Detailed = true;
                    break;
            }
        }

        return options;
    }

    private static bool ShouldIncludeFile(string relativePath, SnapshotOptions options)
    {
        // Normalize path for matching
        var normalizedPath = NormalizePath(relativePath);

        if (options.IncludePatterns.Any())
        {
            if (!options.IncludePatterns.Any(pattern => MatchesPattern(normalizedPath, pattern)))
                return false;
        }

        if (options.ExcludePatterns.Any())
        {
            if (options.ExcludePatterns.Any(pattern => MatchesPattern(normalizedPath, pattern)))
                return false;
        }

        return true;
    }

    private static bool MatchesPattern(string path, string pattern)
    {
        // Normalize both path and pattern
        path = NormalizePath(path);
        pattern = NormalizePath(pattern);

        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace(@"\*\*", ".*")
            .Replace(@"\*", "[^/]*")
            .Replace(@"\?", ".") + "$";

        return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        // Normalize path separators to forward slashes for consistent matching
        return path.Replace('\\', '/');
    }

    private static async Task<Snapshot> CreateSnapshotAsync(string directory, SnapshotOptions options, CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        var entries = new List<FileEntry>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var relativePath = Path.GetRelativePath(directory, file);

                if (!ShouldIncludeFile(relativePath, options))
                    continue;

                var info = new FileInfo(file);
                string? hash = null;

                if (!options.NoHash)
                {
                    hash = await ComputeSha256Async(file, cancellationToken);
                }

                entries.Add(new FileEntry
                {
                    RelativePath = NormalizePath(relativePath),
                    Length = info.Length,
                    LastWriteUtc = info.LastWriteTimeUtc,
                    Sha256 = hash ?? string.Empty
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Silently skip files that can't be accessed
            }
            catch (Exception)
            {
                // Silently skip files that can't be processed
            }
        }

        return new Snapshot
        {
            CreatedAtUtc = DateTime.UtcNow,
            RootDirectory = Path.GetFullPath(directory),
            Files = entries,
            Options = options
        };
    }

    private static DiffResult ComputeDiff(Snapshot oldSnap, Snapshot newSnap, DiffOptions options)
    {
        var oldDict = oldSnap.Files.ToDictionary(f => f.RelativePath, StringComparer.OrdinalIgnoreCase);
        var newDict = newSnap.Files.ToDictionary(f => f.RelativePath, StringComparer.OrdinalIgnoreCase);

        var added = new List<FileEntry>();
        var removed = new List<FileEntry>();
        var changed = new List<(FileEntry Old, FileEntry New)>();
        var unchanged = new List<FileEntry>();

        foreach (var kvp in newDict)
        {
            if (!oldDict.TryGetValue(kvp.Key, out var oldFile))
            {
                added.Add(kvp.Value);
            }
            else if (!string.Equals(oldFile.Sha256, kvp.Value.Sha256, StringComparison.OrdinalIgnoreCase) ||
                     oldFile.Length != kvp.Value.Length)
            {
                changed.Add((oldFile, kvp.Value));
            }
            else if (options.ShowUnchanged)
            {
                unchanged.Add(kvp.Value);
            }
        }

        foreach (var kvp in oldDict)
        {
            if (!newDict.ContainsKey(kvp.Key))
            {
                removed.Add(kvp.Value);
            }
        }

        return new DiffResult
        {
            Added = added,
            Removed = removed,
            Changed = changed,
            Unchanged = unchanged,
            OldSnapshot = oldSnap,
            NewSnapshot = newSnap
        };
    }

    private static void DisplayDiff(DiffResult diff, string oldPath, string newPath, DiffOptions options)
    {
        WriteColorLine("\n╔══════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
        WriteColorLine("║              FileChronicle Diff Report                       ║", ConsoleColor.Cyan);
        WriteColorLine("╚══════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        
        Console.WriteLine($"\nOld: {oldPath}");
        Console.WriteLine($"     {diff.OldSnapshot.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"New: {newPath}");
        Console.WriteLine($"     {diff.NewSnapshot.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        WriteColorLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", ConsoleColor.DarkGray);

        WriteColorLine($"\n📊 Summary:", ConsoleColor.Cyan);
        WriteColorLine($"   Added   : {diff.Added.Count} files", ConsoleColor.Green);
        WriteColorLine($"   Removed : {diff.Removed.Count} files", ConsoleColor.Red);
        WriteColorLine($"   Changed : {diff.Changed.Count} files", ConsoleColor.Yellow);
        Console.WriteLine();

        if (diff.Added.Any())
        {
            WriteColorLine($"➕ Added Files ({diff.Added.Count}):", ConsoleColor.Green);
            foreach (var f in diff.Added)
            {
                if (options.Detailed)
                    Console.WriteLine($"   + {f.RelativePath} ({FormatBytes(f.Length)})");
                else
                    WriteColorLine($"   + {f.RelativePath}", ConsoleColor.Green);
            }
            Console.WriteLine();
        }

        if (diff.Removed.Any())
        {
            WriteColorLine($"➖ Removed Files ({diff.Removed.Count}):", ConsoleColor.Red);
            foreach (var f in diff.Removed)
            {
                if (options.Detailed)
                    Console.WriteLine($"   - {f.RelativePath} ({FormatBytes(f.Length)})");
                else
                    WriteColorLine($"   - {f.RelativePath}", ConsoleColor.Red);
            }
            Console.WriteLine();
        }

        if (diff.Changed.Any())
        {
            WriteColorLine($"✏️  Changed Files ({diff.Changed.Count}):", ConsoleColor.Yellow);
            foreach (var pair in diff.Changed)
            {
                if (options.Detailed)
                {
                    WriteColorLine($"   * {pair.Old.RelativePath}", ConsoleColor.Yellow);
                    Console.WriteLine($"     Size: {FormatBytes(pair.Old.Length)} → {FormatBytes(pair.New.Length)} ({FormatSizeDiff(pair.Old.Length, pair.New.Length)})");
                    Console.WriteLine($"     Modified: {pair.New.LastWriteUtc:yyyy-MM-dd HH:mm:ss} UTC");
                }
                else
                {
                    WriteColorLine($"   * {pair.Old.RelativePath}", ConsoleColor.Yellow);
                }
            }
            Console.WriteLine();
        }

        if (options.ShowUnchanged && diff.Unchanged.Any())
        {
            WriteColorLine($"✓ Unchanged Files ({diff.Unchanged.Count}):", ConsoleColor.Gray);
            foreach (var f in diff.Unchanged.Take(10))
            {
                WriteColorLine($"   = {f.RelativePath}", ConsoleColor.Gray);
            }
            if (diff.Unchanged.Count > 10)
                Console.WriteLine($"   ... and {diff.Unchanged.Count - 10} more");
            Console.WriteLine();
        }

        var totalChanges = diff.Added.Count + diff.Removed.Count + diff.Changed.Count;
        long sizeChange = diff.Added.Sum(f => f.Length) - diff.Removed.Sum(f => f.Length) +
                         diff.Changed.Sum(p => p.New.Length - p.Old.Length);

        WriteColorLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", ConsoleColor.DarkGray);
        Console.WriteLine($"\nTotal changes: {totalChanges}");
        Console.WriteLine($"Size change  : {FormatSignedBytes(sizeChange)}");
    }

    private static async Task ExportDiffAsync(DiffResult diff, string outputFile, string format)
    {
        var outputDir = Path.GetDirectoryName(Path.GetFullPath(outputFile));
        if (!string.IsNullOrEmpty(outputDir))
            Directory.CreateDirectory(outputDir);

        switch (format)
        {
            case "json":
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(diff, options);
                await File.WriteAllTextAsync(outputFile, json);
                break;

            case "csv":
                var csv = new StringBuilder();
                csv.AppendLine("Status,FilePath,OldSize,NewSize,LastModified");
                foreach (var f in diff.Added)
                    csv.AppendLine($"Added,\"{f.RelativePath}\",,{f.Length},{f.LastWriteUtc:yyyy-MM-dd HH:mm:ss}");
                foreach (var f in diff.Removed)
                    csv.AppendLine($"Removed,\"{f.RelativePath}\",{f.Length},,{f.LastWriteUtc:yyyy-MM-dd HH:mm:ss}");
                foreach (var p in diff.Changed)
                    csv.AppendLine($"Changed,\"{p.New.RelativePath}\",{p.Old.Length},{p.New.Length},{p.New.LastWriteUtc:yyyy-MM-dd HH:mm:ss}");
                await File.WriteAllTextAsync(outputFile, csv.ToString());
                break;

            case "html":
                var html = GenerateHtmlReport(diff);
                await File.WriteAllTextAsync(outputFile, html);
                break;

            default:
                throw new ArgumentException($"Unknown format: {format}");
        }
    }

    private static string GenerateHtmlReport(DiffResult diff)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'><title>FileChronicle Diff Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        sb.AppendLine("h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }");
        sb.AppendLine("h2 { color: #34495e; margin-top: 30px; }");
        sb.AppendLine(".summary { background: white; padding: 20px; border-radius: 5px; margin: 20px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine(".file-list { background: white; padding: 15px; border-radius: 5px; margin: 10px 0; }");
        sb.AppendLine(".added { color: #27ae60; font-weight: bold; }");
        sb.AppendLine(".removed { color: #e74c3c; font-weight: bold; }");
        sb.AppendLine(".changed { color: #f39c12; font-weight: bold; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; background: white; }");
        sb.AppendLine("th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("th { background: #3498db; color: white; }");
        sb.AppendLine("tr:hover { background: #f8f9fa; }");
        sb.AppendLine(".meta { color: #7f8c8d; font-size: 0.9em; }");
        sb.AppendLine("</style></head><body>");
        
        sb.AppendLine("<h1>📊 FileChronicle Diff Report</h1>");
        sb.AppendLine($"<p class='meta'><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        sb.AppendLine($"<p class='meta'><strong>Old Snapshot:</strong> {diff.OldSnapshot.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine($"<p class='meta'><strong>New Snapshot:</strong> {diff.NewSnapshot.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC</p>");
        
        sb.AppendLine("<div class='summary'>");
        sb.AppendLine($"<h2>Summary</h2>");
        sb.AppendLine($"<p><span class='added'>Added:</span> {diff.Added.Count} files</p>");
        sb.AppendLine($"<p><span class='removed'>Removed:</span> {diff.Removed.Count} files</p>");
        sb.AppendLine($"<p><span class='changed'>Changed:</span> {diff.Changed.Count} files</p>");
        
        long sizeChange = diff.Added.Sum(f => f.Length) - diff.Removed.Sum(f => f.Length) +
                         diff.Changed.Sum(p => p.New.Length - p.Old.Length);
        sb.AppendLine($"<p><strong>Total size change:</strong> {FormatSignedBytes(sizeChange)}</p>");
        sb.AppendLine("</div>");

        if (diff.Added.Any())
        {
            sb.AppendLine("<h2 class='added'>➕ Added Files</h2>");
            sb.AppendLine("<table><tr><th>File Path</th><th>Size</th><th>Modified</th></tr>");
            foreach (var f in diff.Added)
                sb.AppendLine($"<tr><td>{System.Web.HttpUtility.HtmlEncode(f.RelativePath)}</td><td>{FormatBytes(f.Length)}</td><td>{f.LastWriteUtc:yyyy-MM-dd HH:mm:ss}</td></tr>");
            sb.AppendLine("</table>");
        }

        if (diff.Removed.Any())
        {
            sb.AppendLine("<h2 class='removed'>➖ Removed Files</h2>");
            sb.AppendLine("<table><tr><th>File Path</th><th>Size</th><th>Modified</th></tr>");
            foreach (var f in diff.Removed)
                sb.AppendLine($"<tr><td>{System.Web.HttpUtility.HtmlEncode(f.RelativePath)}</td><td>{FormatBytes(f.Length)}</td><td>{f.LastWriteUtc:yyyy-MM-dd HH:mm:ss}</td></tr>");
            sb.AppendLine("</table>");
        }

        if (diff.Changed.Any())
        {
            sb.AppendLine("<h2 class='changed'>✏️ Changed Files</h2>");
            sb.AppendLine("<table><tr><th>File Path</th><th>Old Size</th><th>New Size</th><th>Size Change</th><th>Modified</th></tr>");
            foreach (var p in diff.Changed)
                sb.AppendLine($"<tr><td>{System.Web.HttpUtility.HtmlEncode(p.New.RelativePath)}</td><td>{FormatBytes(p.Old.Length)}</td><td>{FormatBytes(p.New.Length)}</td><td>{FormatSizeDiff(p.Old.Length, p.New.Length)}</td><td>{p.New.LastWriteUtc:yyyy-MM-dd HH:mm:ss}</td></tr>");
            sb.AppendLine("</table>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static async Task WriteOutputAsync(string outputPath, Snapshot snapshot, string format)
    {
        var outputDir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(outputDir))
            Directory.CreateDirectory(outputDir);

        switch (format)
        {
            case "json":
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(snapshot, options);
                await File.WriteAllTextAsync(outputPath, json);
                break;

            case "csv":
                var csv = new StringBuilder();
                csv.AppendLine("RelativePath,Length,LastWriteUtc,Sha256");
                foreach (var file in snapshot.Files)
                    csv.AppendLine($"\"{file.RelativePath}\",{file.Length},{file.LastWriteUtc:yyyy-MM-dd HH:mm:ss},{file.Sha256}");
                await File.WriteAllTextAsync(outputPath, csv.ToString());
                break;

            default:
                throw new ArgumentException($"Unknown format: {format}");
        }
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken = default)
    {
        const int bufferSize = 81920; // 80KB buffer
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
        using var sha = SHA256.Create();
        
        var buffer = new byte[bufferSize];
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            sha.TransformBlock(buffer, 0, bytesRead, null, 0);
        }
        
        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha.Hash!);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private static string FormatSignedBytes(long bytes)
    {
        var sign = bytes >= 0 ? "+" : "";
        return sign + FormatBytes(Math.Abs(bytes));
    }

    private static string FormatSizeDiff(long oldSize, long newSize)
    {
        var diff = newSize - oldSize;
        var percent = oldSize > 0 ? (diff * 100.0 / oldSize) : 0;
        return $"{FormatSignedBytes(diff)} ({percent:+0.##;-0.##}%)";
    }

    private static void WriteColor(string text, ConsoleColor color)
    {
        if (_config?.ColoredOutput ?? true)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = oldColor;
        }
        else
        {
            Console.Write(text);
        }
    }

    private static void WriteColorLine(string text, ConsoleColor color)
    {
        if (_config?.ColoredOutput ?? true)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }
        else
        {
            Console.WriteLine(text);
        }
    }
}

public sealed class Snapshot
{
    public DateTime CreatedAtUtc { get; set; }
    public string RootDirectory { get; set; } = string.Empty;
    public List<FileEntry> Files { get; set; } = new();
    public SnapshotOptions? Options { get; set; }
}

public sealed class FileEntry
{
    public string RelativePath { get; set; } = string.Empty;
    public long Length { get; set; }
    public DateTime LastWriteUtc { get; set; }
    public string Sha256 { get; set; } = string.Empty;
}

public sealed class SnapshotOptions
{
    public List<string> IncludePatterns { get; set; } = new();
    public List<string> ExcludePatterns { get; set; } = new();
    public string Format { get; set; } = "json";
    public bool NoHash { get; set; }
    public bool ShowProgress { get; set; }
}

public sealed class DiffOptions
{
    public string OutputFile { get; set; } = string.Empty;
    public string Format { get; set; } = "json";
    public bool ShowUnchanged { get; set; }
    public bool Detailed { get; set; }
}

public sealed class DiffResult
{
    public List<FileEntry> Added { get; set; } = new();
    public List<FileEntry> Removed { get; set; } = new();
    public List<(FileEntry Old, FileEntry New)> Changed { get; set; } = new();
    public List<FileEntry> Unchanged { get; set; } = new();
    public Snapshot OldSnapshot { get; set; } = null!;
    public Snapshot NewSnapshot { get; set; } = null!;
}

public sealed class Configuration
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FileChronicle",
        "config.json");

    public string DefaultFormat { get; set; } = "json";
    public List<string> DefaultExcludePatterns { get; set; } = new() { "*.tmp", "*.log", ".git/**", "bin/**", "obj/**" };
    public int DefaultWatchInterval { get; set; } = 5;
    public bool ColoredOutput { get; set; } = true;

    public static Configuration Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();
            }
        }
        catch
        {
            // If config can't be loaded, return defaults
        }
        return new Configuration();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save configuration: {ex.Message}");
        }
    }

    public void Display()
    {
        Console.WriteLine("\n📋 Current Configuration:");
        Console.WriteLine($"   Config file: {ConfigPath}");
        Console.WriteLine($"   Default format        : {DefaultFormat}");
        Console.WriteLine($"   Default watch interval: {DefaultWatchInterval}s");
        Console.WriteLine($"   Colored output        : {ColoredOutput}");
        Console.WriteLine($"   Default excludes      : {string.Join(", ", DefaultExcludePatterns)}");
        Console.WriteLine();
    }

    public void Set(string key, string value)
    {
        switch (key.ToLowerInvariant())
        {
            case "defaultformat":
                if (!new[] { "json", "csv", "html" }.Contains(value.ToLowerInvariant()))
                    throw new ArgumentException("Format must be json, csv, or html");
                DefaultFormat = value.ToLowerInvariant();
                break;
            case "defaultwatchinterval":
                if (int.TryParse(value, out var interval) && interval > 0)
                    DefaultWatchInterval = interval;
                else
                    throw new ArgumentException("Watch interval must be a positive integer");
                break;
            case "coloredoutput":
                if (bool.TryParse(value, out var colored))
                    ColoredOutput = colored;
                else
                    throw new ArgumentException("ColoredOutput must be true or false");
                break;
            default:
                throw new ArgumentException($"Unknown configuration key: {key}. Valid keys: defaultFormat, defaultWatchInterval, coloredOutput");
        }
    }
}

public sealed class MacroManager
{
    private static readonly string MacroPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FileChronicle",
        "macros.json");

    public Dictionary<string, string> Macros { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static MacroManager Load()
    {
        try
        {
            if (File.Exists(MacroPath))
            {
                var json = File.ReadAllText(MacroPath);
                var manager = JsonSerializer.Deserialize<MacroManager>(json);
                if (manager != null)
                {
                    // Ensure case-insensitive dictionary
                    manager.Macros = new Dictionary<string, string>(manager.Macros, StringComparer.OrdinalIgnoreCase);
                    return manager;
                }
            }
        }
        catch
        {
            // If macros can't be loaded, return new instance
        }
        return new MacroManager();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(MacroPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(MacroPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save macros: {ex.Message}");
        }
    }

    public void SaveMacro(string name, string command)
    {
        Macros[name] = command;
    }

    public bool TryGetMacro(string name, out string command)
    {
        return Macros.TryGetValue(name, out command!);
    }

    public bool DeleteMacro(string name)
    {
        return Macros.Remove(name);
    }

    public Dictionary<string, string> GetAllMacros()
    {
        return new Dictionary<string, string>(Macros, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class InteractiveContext
{
    private readonly Dictionary<string, string> _variables = new(StringComparer.OrdinalIgnoreCase);

    public InteractiveContext()
    {
        // Set default variables
        _variables["cwd"] = Directory.GetCurrentDirectory();
        _variables["timestamp"] = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _variables["date"] = DateTime.Now.ToString("yyyyMMdd");
    }

    public void SetVariable(string name, string value)
    {
        _variables[name] = value;
    }

    public string? GetVariable(string name)
    {
        return _variables.TryGetValue(name, out var value) ? value : null;
    }

    public string ExpandVariables(string input)
    {
        var result = input;
        
        // Replace $variable with values
        foreach (var kvp in _variables)
        {
            var placeholder = $"${kvp.Key}";
            result = result.Replace(placeholder, kvp.Value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    public void Display()
    {
        Console.WriteLine("\n📋 Context Variables:");
        if (!_variables.Any())
        {
            Console.WriteLine("   No variables set.");
            return;
        }

        foreach (var kvp in _variables.OrderBy(x => x.Key))
        {
            Console.WriteLine($"   ${kvp.Key} = {kvp.Value}");
        }

        Console.WriteLine("\n   Use variables with: $variablename");
        Console.WriteLine("   Set variables with: set <name> <value>");
    }
}
