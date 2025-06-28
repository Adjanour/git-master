using GitMaster.Models;
using Spectre.Console;
using System.Diagnostics;
using YamlDotNet.Serialization;

namespace GitMaster.Services;

public interface IPracticeService
{
    Task<PracticeScenario?> LoadScenarioAsync(string scenarioName);
    Task<List<string>> GetAvailableScenariosAsync();
    Task<PracticeSession> StartScenarioAsync(string scenarioName, string? sandboxPath = null);
    Task<ObjectiveResult> EvaluateCurrentObjectiveAsync(PracticeSession session);
    Task<bool> SetupScenarioAsync(PracticeSession session);
    bool IsSessionCompleted(PracticeSession session);
    void DisplayObjective(PracticeSession session);
    void DisplaySessionSummary(PracticeSession session);
}

public class PracticeService : IPracticeService
{
    private readonly IGitRepositoryService _gitService;
    private readonly string _assetsPath;

    public PracticeService(IGitRepositoryService gitService)
    {
        _gitService = gitService;
        _assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "practice");
    }

    public async Task<PracticeScenario?> LoadScenarioAsync(string scenarioName)
    {
        try
        {
            var scenarioPath = Path.Combine(_assetsPath, $"{scenarioName}.yml");
            if (!File.Exists(scenarioPath))
            {
                return null;
            }

            var yamlContent = await File.ReadAllTextAsync(scenarioPath);
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<PracticeScenario>(yamlContent);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return null;
        }
    }

    public async Task<List<string>> GetAvailableScenariosAsync()
    {
        if (!Directory.Exists(_assetsPath))
        {
            return new List<string>();
        }

        var scenarios = new List<string>();
        var yamlFiles = Directory.GetFiles(_assetsPath, "*.yml");

        foreach (var file in yamlFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            scenarios.Add(fileName);
        }

        return scenarios;
    }

    public async Task<PracticeSession> StartScenarioAsync(string scenarioName, string? sandboxPath = null)
    {
        var scenario = await LoadScenarioAsync(scenarioName);
        if (scenario == null)
        {
            throw new ArgumentException($"Scenario '{scenarioName}' not found");
        }

        // Create sandbox directory
        sandboxPath ??= Path.Combine(Path.GetTempPath(), "GitMaster", "Practice", $"{scenarioName}_{DateTime.Now:yyyyMMdd_HHmmss}");
        
        if (Directory.Exists(sandboxPath))
        {
            Directory.Delete(sandboxPath, recursive: true);
        }

        var session = new PracticeSession
        {
            ScenarioName = scenarioName,
            SandboxPath = sandboxPath,
            Scenario = scenario,
            StartTime = DateTime.Now
        };

        return session;
    }

    public async Task<bool> SetupScenarioAsync(PracticeSession session)
    {
        try
        {
            AnsiConsole.Status()
                .Start("Setting up practice scenario...", ctx =>
                {
                    // Create and initialize repository
                    _gitService.InitializeRepository(session.SandboxPath);
                    
                    // Configure git user for the sandbox
                    _gitService.ExecuteGitCommand(session.SandboxPath, "config user.name \"GitMaster Practice\"");
                    _gitService.ExecuteGitCommand(session.SandboxPath, "config user.email \"practice@gitmaster.dev\"");

                    // Execute setup steps
                    foreach (var step in session.Scenario.Setup)
                    {
                        ctx.Status($"Executing: {step.Description}");
                        ExecuteCommand(session.SandboxPath, step.Command);
                        Thread.Sleep(100); // Small delay for visual feedback
                    }
                });

            AnsiConsole.MarkupLine("[green]✓[/] Scenario setup completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Failed to setup scenario: {ex.Message}");
            return false;
        }
    }

    public async Task<ObjectiveResult> EvaluateCurrentObjectiveAsync(PracticeSession session)
    {
        if (session.CurrentObjectiveIndex >= session.Scenario.Objectives.Count)
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Completed,
                Message = "All objectives completed!",
                ShouldAdvance = false
            };
        }

        var objective = session.Scenario.Objectives[session.CurrentObjectiveIndex];
        var result = await EvaluateObjectiveAsync(session, objective);

        if (result.Status == ObjectiveStatus.Completed && result.ShouldAdvance)
        {
            session.CompletedObjectives.Add(objective.Id);
            session.CurrentObjectiveIndex++;
            
            if (session.CurrentObjectiveIndex >= session.Scenario.Objectives.Count)
            {
                session.IsCompleted = true;
            }
        }

        return result;
    }

    private async Task<ObjectiveResult> EvaluateObjectiveAsync(PracticeSession session, PracticeObjective objective)
    {
        try
        {
            // Check based on validation type
            switch (objective.ValidationType?.ToLower())
            {
                case "file_content":
                    return await EvaluateFileContentAsync(session, objective);
                
                default:
                    return await EvaluateCommandResultAsync(session, objective);
            }
        }
        catch (Exception ex)
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Failed,
                Message = $"Evaluation error: {ex.Message}",
                Hint = objective.Hint
            };
        }
    }

    private async Task<ObjectiveResult> EvaluateFileContentAsync(PracticeSession session, PracticeObjective objective)
    {
        if (string.IsNullOrEmpty(objective.TargetFile))
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Failed,
                Message = "Target file not specified for file content validation"
            };
        }

        var filePath = Path.Combine(session.SandboxPath, objective.TargetFile);
        if (!File.Exists(filePath))
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.InProgress,
                Message = $"File {objective.TargetFile} not found",
                Hint = objective.Hint
            };
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        
        if (objective.ExpectedContentContains != null)
        {
            var missingContent = objective.ExpectedContentContains
                .Where(content => !fileContent.Contains(content, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (missingContent.Any())
            {
                return new ObjectiveResult
                {
                    Status = ObjectiveStatus.InProgress,
                    Message = $"File content missing: {string.Join(", ", missingContent)}",
                    Hint = objective.Hint
                };
            }
        }

        return new ObjectiveResult
        {
            Status = ObjectiveStatus.Completed,
            Message = "File content validation passed!",
            ShouldAdvance = true
        };
    }

    private async Task<ObjectiveResult> EvaluateCommandResultAsync(PracticeSession session, PracticeObjective objective)
    {
        switch (objective.ExpectedResult?.ToLower())
        {
            case "merge_conflict":
                return EvaluateMergeConflict(session, objective);
            
            case "shows_conflicts":
                return EvaluateShowsConflicts(session, objective);
            
            case "file_staged":
                return EvaluateFileStaged(session, objective);
            
            case "merge_completed":
                return EvaluateMergeCompleted(session, objective);
            
            default:
                return new ObjectiveResult
                {
                    Status = ObjectiveStatus.InProgress,
                    Message = $"Please run: {objective.Command}",
                    Hint = objective.Hint
                };
        }
    }

    private ObjectiveResult EvaluateMergeConflict(PracticeSession session, PracticeObjective objective)
    {
        if (_gitService.HasMergeConflicts(session.SandboxPath))
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Completed,
                Message = "Merge conflict detected! Good, now you can practice resolving it.",
                ShouldAdvance = true
            };
        }

        if (_gitService.IsMergeInProgress(session.SandboxPath))
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.InProgress,
                Message = "Merge is in progress but no conflicts detected yet.",
                Hint = objective.Hint
            };
        }

        return new ObjectiveResult
        {
            Status = ObjectiveStatus.InProgress,
            Message = "No merge conflict detected. Try running the merge command.",
            Hint = objective.Hint
        };
    }

    private ObjectiveResult EvaluateShowsConflicts(PracticeSession session, PracticeObjective objective)
    {
        var conflictedFiles = _gitService.GetConflictedFiles(session.SandboxPath);
        
        if (conflictedFiles.Any())
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Completed,
                Message = $"Conflicted files detected: {string.Join(", ", conflictedFiles)}",
                ShouldAdvance = true
            };
        }

        return new ObjectiveResult
        {
            Status = ObjectiveStatus.InProgress,
            Message = "No conflicts currently showing. Run git status to check.",
            Hint = objective.Hint
        };
    }

    private ObjectiveResult EvaluateFileStaged(PracticeSession session, PracticeObjective objective)
    {
        // Check if conflicts are resolved (no more conflicted files)
        var conflictedFiles = _gitService.GetConflictedFiles(session.SandboxPath);
        
        if (!conflictedFiles.Any() && _gitService.IsMergeInProgress(session.SandboxPath))
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Completed,
                Message = "File staged and conflicts resolved!",
                ShouldAdvance = true
            };
        }

        return new ObjectiveResult
        {
            Status = ObjectiveStatus.InProgress,
            Message = "File not yet staged or conflicts not resolved.",
            Hint = objective.Hint
        };
    }

    private ObjectiveResult EvaluateMergeCompleted(PracticeSession session, PracticeObjective objective)
    {
        if (!_gitService.IsMergeInProgress(session.SandboxPath) && 
            !_gitService.HasMergeConflicts(session.SandboxPath))
        {
            return new ObjectiveResult
            {
                Status = ObjectiveStatus.Completed,
                Message = "Merge completed successfully!",
                ShouldAdvance = true
            };
        }

        return new ObjectiveResult
        {
            Status = ObjectiveStatus.InProgress,
            Message = "Merge is still in progress. Complete the merge commit.",
            Hint = objective.Hint
        };
    }

    public bool IsSessionCompleted(PracticeSession session)
    {
        return session.IsCompleted;
    }

    public void DisplayObjective(PracticeSession session)
    {
        if (session.CurrentObjectiveIndex >= session.Scenario.Objectives.Count)
        {
            AnsiConsole.MarkupLine("[bold green]🎉 All objectives completed![/]");
            return;
        }

        var objective = session.Scenario.Objectives[session.CurrentObjectiveIndex];
        var progress = session.CurrentObjectiveIndex + 1;
        var total = session.Scenario.Objectives.Count;

        var panel = new Panel($"""
                              [bold]Objective {progress}/{total}:[/]
                              {objective.Goal}
                              
                              [dim]💡 Hint: {objective.Hint}[/]
                              """)
            .Header($"[bold blue]{session.Scenario.Name}[/]")
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);
    }

    public void DisplaySessionSummary(PracticeSession session)
    {
        var duration = DateTime.Now - session.StartTime;
        var completedCount = session.CompletedObjectives.Count;
        var totalCount = session.Scenario.Objectives.Count;
        
        var summary = new Panel($"""
                                [bold green]Practice Session Complete![/]
                                
                                Scenario: [bold]{session.Scenario.Name}[/]
                                Duration: [yellow]{duration.Minutes:D2}:{duration.Seconds:D2}[/]
                                Objectives Completed: [green]{completedCount}/{totalCount}[/]
                                
                                [dim]Sandbox location: {session.SandboxPath}[/]
                                """)
            .Header("[bold green]🎯 Session Summary[/]")
            .BorderColor(Color.Green);

        AnsiConsole.Write(summary);

        if (completedCount == totalCount)
        {
            AnsiConsole.MarkupLine("[bold green]🏆 Congratulations! You've mastered this scenario![/]");
        }
    }

    private void ExecuteCommand(string workingDirectory, string command)
    {
        try
        {
            // Handle different types of commands
            if (command.StartsWith("git "))
            {
                var result = _gitService.ExecuteGitCommand(workingDirectory, command.Substring(4));
                if (!string.IsNullOrEmpty(result) && result.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine($"[yellow]Git output: {result}[/]");
                }
            }
            else
            {
                // Execute as shell command with better Windows support
                var processInfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/c \"{command}\"",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        AnsiConsole.MarkupLine($"[yellow]Command error: {error}[/]");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error executing command '{command}': {ex.Message}[/]");
        }
    }
}
