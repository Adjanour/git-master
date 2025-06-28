using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using GitMaster.Services;
using GitMaster.Models;

namespace GitMaster.Commands;

public class LearnCommand : Command<LearnCommand.Settings>
{
    private readonly LessonService _lessonService;
    private readonly LessonRenderer _lessonRenderer;
    private readonly ProgressService _progressService;

    public LearnCommand()
    {
        _lessonService = new LessonService();
        _lessonRenderer = new LessonRenderer();
        _progressService = new ProgressService();
    }
    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "[module]")]
        [Description("Learning module to access")]
        public string? Module { get; init; }

        [CommandOption("-l|--list")]
        [Description("List all available learning modules")]
        public bool ListModules { get; init; }

        [CommandOption("-r|--resume")]
        [Description("Resume from where you left off")]
        public bool Resume { get; init; }

        [CommandOption("-c|--chapter")]
        [Description("Start from a specific chapter")]
        public int? Chapter { get; init; }

        [CommandOption("-n|--next")]
        [Description("Continue to the next lesson in order")]
        public bool Next { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[bold magenta]GitMaster Learning Center[/]");
        
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Module: {settings.Module}[/]");
            AnsiConsole.MarkupLine($"[dim]Resume: {settings.Resume}[/]");
            if (settings.Chapter.HasValue)
            {
                AnsiConsole.MarkupLine($"[dim]Starting chapter: {settings.Chapter.Value}[/]");
            }
        }

        if (settings.Next)
        {
            var availableModules = _lessonService.GetAvailableModules();
            var nextModule = _progressService.GetNextModule(availableModules);
            StartLearningModule(nextModule, false, null);
        }
        else if (settings.ListModules || string.IsNullOrEmpty(settings.Module))
        {
            ShowAvailableModules();
        }
        else
        {
            StartLearningModule(settings.Module, settings.Resume, settings.Chapter);
        }
        
        return 0;
    }

    private void ShowAvailableModules()
    {
        var modules = _lessonService.GetAvailableModules();
        var overallProgress = _progressService.GetOverallProgressPercentage(modules);
        
        AnsiConsole.MarkupLine($"[yellow]Available learning modules[/] [dim]({overallProgress}% complete)[/]");
        AnsiConsole.WriteLine();
        
        var table = new Table();
        table.AddColumn("Module");
        table.AddColumn("Description");
        table.AddColumn("Duration");
        table.AddColumn("Level");
        table.AddColumn("Progress");
        
        foreach (var module in modules)
        {
            var moduleName = GetModuleNameFromTitle(module.Title);
            var progress = _progressService.GetProgress(moduleName);
            var progressText = progress.IsCompleted ? "[green]✓ Complete[/]" : 
                              progress.CompletedLessons.Any() ? $"[yellow]{progress.CompletedLessons.Count} lessons[/]" : 
                              "[dim]Not started[/]";
            
            table.AddRow(
                moduleName,
                module.Description,
                module.Duration,
                module.Level,
                progressText
            );
        }
        
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Usage:[/]");
        AnsiConsole.MarkupLine("[dim]  gitmaster learn <module>  - Start specific module[/]");
        AnsiConsole.MarkupLine("[dim]  gitmaster learn --next    - Continue next lesson[/]");
        AnsiConsole.MarkupLine("[dim]  gitmaster learn --resume  - Resume current module[/]");
    }

    private void StartLearningModule(string module, bool resume, int? chapter)
    {
        var lesson = _lessonService.LoadLesson(module);
        if (lesson == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: Module '{module}' not found.[/]");
            AnsiConsole.MarkupLine("[dim]Use 'gitmaster learn --list' to see available modules.[/]");
            return;
        }

        var progress = _progressService.GetProgress(module);
        
        if (resume && progress.CurrentLesson > 0)
        {
            AnsiConsole.MarkupLine($"[green]Resuming module '{module}' from lesson {progress.CurrentLesson}...[/]");
        }
        else if (chapter.HasValue)
        {
            AnsiConsole.MarkupLine($"[blue]Starting module '{module}' from lesson {chapter.Value}...[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[blue]Starting module '{module}' from the beginning...[/]");
            progress.CurrentLesson = 0;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to begin...[/]");
        Console.ReadKey(true);

        // Record lesson start time
        var lessonStartTime = DateTime.Now;
        
        // Render the lesson
        _lessonRenderer.RenderLesson(lesson);
        
        // Calculate lesson completion time
        var lessonEndTime = DateTime.Now;
        var timeSpent = lessonEndTime - lessonStartTime;
        
        // Update progress with detailed tracking
        _progressService.UpdateProgress(module, 0, true);
        _progressService.MarkModuleCompleted(module);
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold green]🎉 Congratulations! You've completed the '{lesson.Title}' module![/]");
        AnsiConsole.WriteLine();
        
        // Show next steps
        var availableModules = _lessonService.GetAvailableModules();
        var nextModule = _progressService.GetNextModule(availableModules);
        
        if (nextModule != module)
        {
            AnsiConsole.MarkupLine($"[yellow]Next up: {nextModule}[/]");
            AnsiConsole.MarkupLine("[dim]Run 'gitmaster learn --next' to continue your learning journey.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold cyan]🎯 You've completed all available modules! Great job![/]");
            AnsiConsole.MarkupLine("[dim]Check back for new content or practice with 'gitmaster practice'.[/]");
        }
    }
    
    private string GetModuleNameFromTitle(string title)
    {
        return title.ToLowerInvariant() switch
        {
            "git fundamentals" => "basics",
            "branch management" => "branching", 
            "team collaboration" => "collaboration",
            "git workflows" => "workflows",
            "advanced techniques" => "advanced",
            "common problems" => "troubleshooting",
            _ => title.ToLowerInvariant().Replace(" ", "-")
        };
    }
}
