using GitMaster.Models;
using Spectre.Console;

namespace GitMaster.Services;

public interface IPracticeRunner
{
    Task RunScenarioAsync(string scenarioName, bool interactive, string? sandboxPath = null);
    Task RunInteractiveSessionAsync(PracticeSession session);
}

public class PracticeRunner : IPracticeRunner
{
    private readonly IPracticeService _practiceService;
    private readonly IGitRepositoryService _gitService;
    private readonly ProgressService _progressService;
    private readonly List<string> _hintsUsed;

    public PracticeRunner(IPracticeService practiceService, IGitRepositoryService gitService)
    {
        _practiceService = practiceService;
        _gitService = gitService;
        _progressService = new ProgressService();
        _hintsUsed = new List<string>();
    }

    public async Task RunScenarioAsync(string scenarioName, bool interactive, string? sandboxPath = null)
    {
        try
        {
            // Start progress tracking
            _progressService.StartPracticeSession(scenarioName);
            _hintsUsed.Clear();
            
            // Load and start the scenario
            var session = await _practiceService.StartScenarioAsync(scenarioName, sandboxPath);
            
            // Display scenario introduction
            DisplayScenarioIntroduction(session.Scenario);
            
            // Setup the practice environment
            var setupSuccess = await _practiceService.SetupScenarioAsync(session);
            if (!setupSuccess)
            {
                AnsiConsole.MarkupLine("[red]Failed to setup practice scenario. Exiting.[/]");
                // Record failed session
                _progressService.CompletePracticeSession(scenarioName, 0, session.Scenario.Objectives.Count, false, _hintsUsed);
                return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Practice repository created at:[/] [dim]{session.SandboxPath}[/]");
            AnsiConsole.WriteLine();

            if (interactive)
            {
                await RunInteractiveSessionAsync(session);
            }
            else
            {
                await RunNonInteractiveSessionAsync(session);
            }

            // Display final summary
            _practiceService.DisplaySessionSummary(session);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    public async Task RunInteractiveSessionAsync(PracticeSession session)
    {
        AnsiConsole.MarkupLine("[bold yellow]🎯 Interactive Practice Mode[/]");
        AnsiConsole.MarkupLine("Follow the objectives one by one. The system will monitor your progress!");
        AnsiConsole.WriteLine();

        while (!_practiceService.IsSessionCompleted(session))
        {
            // Display current objective
            _practiceService.DisplayObjective(session);
            AnsiConsole.WriteLine();

            // Wait for user to attempt the objective
            var shouldContinue = await WaitForUserProgressAsync(session);
            if (!shouldContinue)
            {
                break;
            }

            AnsiConsole.WriteLine();
        }

        // Calculate completion status
        var isCompleted = _practiceService.IsSessionCompleted(session);
        var completedObjectives = session.CompletedObjectives.Count;
        var totalObjectives = session.Scenario.Objectives.Count;
        
        // Record progress
        _progressService.CompletePracticeSession(
            session.ScenarioName, 
            completedObjectives, 
            totalObjectives, 
            isCompleted, 
            _hintsUsed
        );
        
        if (isCompleted)
        {
            AnsiConsole.MarkupLine("[bold green]🎉 Congratulations! You've completed all objectives![/]");
        }
    }

    private async Task RunNonInteractiveSessionAsync(PracticeSession session)
    {
        AnsiConsole.MarkupLine("[bold blue]📖 Practice Setup Complete[/]");
        AnsiConsole.MarkupLine("The practice environment has been set up. Here are your objectives:");
        AnsiConsole.WriteLine();

        // Display all objectives
        var table = new Table();
        table.AddColumn("[bold]Step[/]");
        table.AddColumn("[bold]Objective[/]");
        table.AddColumn("[bold]Hint[/]");

        for (int i = 0; i < session.Scenario.Objectives.Count; i++)
        {
            var objective = session.Scenario.Objectives[i];
            table.AddRow(
                $"{i + 1}",
                objective.Goal,
                objective.Hint
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine($"[bold]Practice in:[/] {session.SandboxPath}");
        AnsiConsole.MarkupLine("[dim]Use 'cd' to navigate to the practice directory and start working![/]");
    }

    private async Task<bool> WaitForUserProgressAsync(PracticeSession session)
    {
        var lastResult = new ObjectiveResult { Status = ObjectiveStatus.InProgress };
        
        while (true)
        {
            // Evaluate current objective
            var result = await _practiceService.EvaluateCurrentObjectiveAsync(session);

            // Only display feedback if status changed or it's the first time
            if (result.Status != lastResult.Status || result.Message != lastResult.Message)
            {
                DisplayObjectiveResult(result);
            }

            lastResult = result;

            // Check if objective is completed
            if (result.Status == ObjectiveStatus.Completed && result.ShouldAdvance)
            {
                AnsiConsole.MarkupLine("[green]✓ Objective completed![/]");
                return true; // Continue to next objective
            }

            if (result.Status == ObjectiveStatus.Failed)
            {
                var shouldRetry = AnsiConsole.Confirm("Would you like to try again?");
                if (!shouldRetry)
                {
                    return false; // Exit practice session
                }
            }

            // Wait a bit before checking again
            await Task.Delay(2000);
        }
    }

    private void DisplayScenarioIntroduction(PracticeScenario scenario)
    {
        var intro = new Panel($"""
                              [bold]{scenario.Name}[/]
                              
                              {scenario.Description}
                              
                              [dim]Difficulty:[/] [yellow]{scenario.Difficulty}[/]
                              [dim]Category:[/] [blue]{scenario.Category}[/]
                              [dim]Estimated Time:[/] [green]{scenario.EstimatedTime}[/]
                              """)
            .Header("[bold blue]🎯 Practice Scenario[/]")
            .BorderColor(Color.Blue);

        AnsiConsole.Write(intro);
        AnsiConsole.WriteLine();
    }

    private void DisplayObjectiveResult(ObjectiveResult result)
    {
        var statusColor = result.Status switch
        {
            ObjectiveStatus.Completed => "green",
            ObjectiveStatus.Failed => "red",
            ObjectiveStatus.InProgress => "yellow",
            _ => "dim"
        };

        var statusIcon = result.Status switch
        {
            ObjectiveStatus.Completed => "✓",
            ObjectiveStatus.Failed => "✗",
            ObjectiveStatus.InProgress => "⋯",
            _ => "○"
        };

        AnsiConsole.MarkupLine($"[{statusColor}]{statusIcon} {result.Message}[/]");

        if (!string.IsNullOrEmpty(result.Hint) && result.Status != ObjectiveStatus.Completed)
        {
            AnsiConsole.MarkupLine($"[dim]💡 {result.Hint}[/]");
            
            // Track hint usage
            if (!_hintsUsed.Contains(result.Hint))
            {
                _hintsUsed.Add(result.Hint);
            }
        }
    }
}
