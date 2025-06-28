using GitMaster.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace GitMaster.Commands;

public class PracticeCommand : AsyncCommand<PracticeCommand.Settings>
{
    private readonly IPracticeService _practiceService;
    private readonly IPracticeRunner _practiceRunner;

    public PracticeCommand()
    {
        var gitService = new GitRepositoryService();
        _practiceService = new PracticeService(gitService);
        _practiceRunner = new PracticeRunner(_practiceService, gitService);
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("-s|--scenario")]
        [Description("Specific practice scenario to run")]
        public string? Scenario { get; init; }

        [CommandOption("-d|--difficulty")]
        [Description("Difficulty level (beginner/intermediate/advanced)")]
        [DefaultValue("beginner")]
        public string Difficulty { get; init; } = "beginner";

        [CommandOption("-i|--interactive")]
        [Description("Enable interactive mode with guided hints")]
        public bool Interactive { get; init; }

        [CommandOption("--sandbox-path")]
        [Description("Custom path for the practice sandbox (for debugging scenarios)")]
        public string? SandboxPath { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[bold green]GitMaster Practice Mode[/]");
        AnsiConsole.WriteLine();
        
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Repository path: {settings.RepoPath ?? "current directory"}[/]");
            AnsiConsole.MarkupLine($"[dim]Difficulty: {settings.Difficulty}[/]");
            AnsiConsole.MarkupLine($"[dim]Interactive: {settings.Interactive}[/]");
            if (!string.IsNullOrEmpty(settings.SandboxPath))
            {
                AnsiConsole.MarkupLine($"[dim]Sandbox path: {settings.SandboxPath}[/]");
            }
            AnsiConsole.WriteLine();
        }

        try
        {
            if (!string.IsNullOrEmpty(settings.Scenario))
            {
                // Run specific scenario
                await _practiceRunner.RunScenarioAsync(settings.Scenario, settings.Interactive, settings.SandboxPath);
            }
            else
            {
                // Show available scenarios and let user choose
                await ShowScenarioSelectionAsync(settings);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(ex);
            }
            return 1;
        }
        
        return 0;
    }

    private async Task ShowScenarioSelectionAsync(Settings settings)
    {
        var scenarios = await _practiceService.GetAvailableScenariosAsync();
        
        if (!scenarios.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No practice scenarios found in assets/practice directory.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[blue]Available Practice Scenarios:[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("[bold]Scenario[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddColumn("[bold]Difficulty[/]");
        table.AddColumn("[bold]Time[/]");

        foreach (var scenarioName in scenarios)
        {
            var scenario = await _practiceService.LoadScenarioAsync(scenarioName);
            if (scenario != null)
            {
                table.AddRow(
                    scenarioName,
                    scenario.Description,
                    scenario.Difficulty,
                    scenario.EstimatedTime
                );
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var selectedScenario = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select a scenario to start practicing:[/]")
                .AddChoices(scenarios)
        );

        await _practiceRunner.RunScenarioAsync(selectedScenario, settings.Interactive, settings.SandboxPath);
    }
}
