using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using GitMaster.Services;
using GitMaster.Models;
using System.Text.Json;

namespace GitMaster.Commands;

public class ProgressCommand : Command<ProgressCommand.Settings>
{
    private readonly ProgressService _progressService;

    public ProgressCommand()
    {
        _progressService = new ProgressService();
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("-d|--detailed")]
        [Description("Show detailed progress breakdown")]
        public bool Detailed { get; init; }

        [CommandOption("-m|--module")]
        [Description("Show progress for specific module")]
        public string? Module { get; init; }

        [CommandOption("--export")]
        [Description("Export progress to file (json/csv)")]
        public string? ExportFormat { get; init; }

        [CommandOption("-c|--commands")]
        [Description("Show command usage statistics")]
        public bool Commands { get; init; }

        [CommandOption("-p|--practice")]
        [Description("Show practice session statistics")]
        public bool Practice { get; init; }

        [CommandOption("-s|--streaks")]
        [Description("Show streak information")]
        public bool Streaks { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var progressData = _progressService.GetProgressData();
        
        AnsiConsole.MarkupLine("[bold blue]GitMaster Progress Report[/]");
        
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Data location: %LOCALAPPDATA%\\GitMaster\\progress.json[/]");
            AnsiConsole.MarkupLine($"[dim]Last updated: {progressData.LastUpdated:yyyy-MM-dd HH:mm:ss}[/]");
            AnsiConsole.MarkupLine($"[dim]Detailed view: {settings.Detailed}[/]");
            if (!string.IsNullOrEmpty(settings.Module))
            {
                AnsiConsole.MarkupLine($"[dim]Module filter: {settings.Module}[/]");
            }
        }

        ShowOverallProgress(progressData);
        
        if (settings.Streaks)
        {
            ShowStreakProgress(progressData);
        }
        
        if (settings.Commands)
        {
            ShowCommandStats(progressData);
        }
        
        if (settings.Practice)
        {
            ShowPracticeStats(progressData);
        }
        
        if (settings.Detailed)
        {
            ShowDetailedProgress(progressData, settings.Module);
        }

        if (!string.IsNullOrEmpty(settings.ExportFormat))
        {
            ExportProgress(progressData, settings.ExportFormat);
        }
        
        return 0;
    }

    private void ShowOverallProgress(ProgressData progressData)
    {
        AnsiConsole.MarkupLine("\n[yellow]📊 Overall Progress[/]");
        
        var stats = progressData.Stats;
        var streaks = progressData.Streaks;
        
        var table = new Table();
        table.AddColumn("[bold]Metric[/]");
        table.AddColumn("[bold]Value[/]");
        table.AddColumn("[bold]Details[/]");
        
        // User level and XP
        var nextLevelXP = stats.Level * 1000;
        var currentLevelXP = (stats.Level - 1) * 1000;
        var progressToNext = stats.ExperiencePoints - currentLevelXP;
        var xpNeeded = nextLevelXP - currentLevelXP;
        
        table.AddRow(
            "🎯 Level", 
            $"[green]{stats.Level}[/]", 
            $"[dim]{progressToNext}/{xpNeeded} XP to next level[/]"
        );
        
        table.AddRow(
            "📚 Modules Completed", 
            $"[cyan]{stats.ModulesCompleted}[/]",
            progressData.Modules.Count > 0 ? $"[dim]{progressData.Modules.Count} total[/]" : "[dim]None started[/]"
        );
        
        table.AddRow(
            "🏃‍♂️ Practice Sessions", 
            $"[yellow]{stats.PracticeSessionsCompleted}[/]",
            progressData.Practice.Count > 0 ? $"[dim]{progressData.Practice.Count} scenarios attempted[/]" : "[dim]None attempted[/]"
        );
        
        table.AddRow(
            "🔥 Current Streak", 
            $"[orange1]{streaks.CurrentStreak} days[/]",
            streaks.LongestStreak > 0 ? $"[dim]Best: {streaks.LongestStreak} days[/]" : "[dim]Keep going![/]"
        );
        
        table.AddRow(
            "⏱️ Total Sessions", 
            $"[blue]{stats.TotalSessions}[/]",
            stats.FirstSession != DateTime.MinValue ? $"[dim]Since {stats.FirstSession:MMM dd, yyyy}[/]" : "[dim]Just started![/]"
        );
        
        table.AddRow(
            "📈 Total Score", 
            $"[green]{stats.TotalScore:N0}[/]",
            $"[dim]{stats.ExperiencePoints:N0} XP earned[/]"
        );
        
        AnsiConsole.Write(table);
        
        // Module progress chart
        if (progressData.Modules.Any())
        {
            AnsiConsole.MarkupLine("\n[yellow]📖 Learning Module Progress[/]");
            
            var barChart = new BarChart()
                .Width(60)
                .Label("[green bold]Completion Percentage[/]")
                .CenterLabel();
            
            foreach (var module in progressData.Modules.Values.OrderBy(m => m.ModuleName))
            {
                var completedLessons = module.LessonAttempts.Count(a => a.Completed);
                var percentage = completedLessons > 0 ? Math.Min(100, completedLessons * 20) : 0; // Assume ~5 lessons per module
                
                var color = percentage switch
                {
                    100 => Color.Green,
                    >= 75 => Color.Yellow,
                    >= 50 => Color.Orange1,
                    >= 25 => Color.Red,
                    _ => Color.Grey
                };
                
                barChart.AddItem(FormatModuleName(module.ModuleName), percentage, color);
            }
            
            AnsiConsole.Write(barChart);
        }
    }

    private void ShowStreakProgress(ProgressData progressData)
    {
        AnsiConsole.MarkupLine("\n[yellow]🔥 Streak Progress[/]");
        
        var streaks = progressData.Streaks;
        
        if (streaks.ActivityDates.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No activity recorded yet. Start learning or practicing to build your streak![/]");
            return;
        }
        
        // Show recent activity calendar (last 30 days)
        var today = DateTime.Today;
        var thirtyDaysAgo = today.AddDays(-29);
        
        var grid = new Grid();
        grid.AddColumn();
        
        var calendarGrid = new Grid();
        for (int i = 0; i < 7; i++)
        {
            calendarGrid.AddColumn();
        }
        
        // Add day headers
        calendarGrid.AddRow("[dim]Mon[/]", "[dim]Tue[/]", "[dim]Wed[/]", "[dim]Thu[/]", "[dim]Fri[/]", "[dim]Sat[/]", "[dim]Sun[/]");
        
        // Create activity calendar
        var startDay = thirtyDaysAgo;
        while (startDay.DayOfWeek != DayOfWeek.Monday)
        {
            startDay = startDay.AddDays(-1);
        }
        
        var weeks = new List<List<string>>();
        var currentWeek = new List<string>();
        
        for (var date = startDay; date <= today; date = date.AddDays(1))
        {
            var hasActivity = streaks.ActivityDates.Contains(date);
            var symbol = date > today ? "   " : hasActivity ? "[green]●[/]" : "[dim]○[/]";
            
            currentWeek.Add($" {symbol} ");
            
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                weeks.Add(new List<string>(currentWeek));
                currentWeek.Clear();
            }
        }
        
        if (currentWeek.Any())
        {
            while (currentWeek.Count < 7)
            {
                currentWeek.Add("   ");
            }
            weeks.Add(currentWeek);
        }
        
        foreach (var week in weeks)
        {
            calendarGrid.AddRow(week.ToArray());
        }
        
        AnsiConsole.Write(calendarGrid);
        
        AnsiConsole.MarkupLine($"\n[green]●[/] Active days: {streaks.ActivityDates.Count(d => d >= thirtyDaysAgo && d <= today)}/30");
        
        if (streaks.CurrentStreak > 0)
        {
            AnsiConsole.MarkupLine($"[yellow]🔥 Current streak: {streaks.CurrentStreak} days[/]");
            if (streaks.LongestStreak > streaks.CurrentStreak)
            {
                AnsiConsole.MarkupLine($"[dim]🏆 Personal best: {streaks.LongestStreak} days[/]");
            }
        }
    }

    private void ShowCommandStats(ProgressData progressData)
    {
        AnsiConsole.MarkupLine("\n[yellow]⌨️ Command Usage Statistics[/]");
        
        if (!progressData.Commands.Any())
        {
            AnsiConsole.MarkupLine("[dim]No command usage recorded yet.[/]");
            return;
        }
        
        var topCommands = progressData.Commands.Values
            .OrderByDescending(c => c.UsageCount)
            .Take(10)
            .ToList();
        
        var table = new Table();
        table.AddColumn("[bold]Command[/]");
        table.AddColumn("[bold]Uses[/]");
        table.AddColumn("[bold]Success Rate[/]");
        table.AddColumn("[bold]Topics[/]");
        table.AddColumn("[bold]Last Used[/]");
        
        foreach (var cmd in topCommands)
        {
            var successRate = cmd.UsageCount > 0 
                ? (double)cmd.SuccessfulUses / cmd.UsageCount * 100 
                : 0;
            
            var successColor = successRate switch
            {
                >= 90 => "green",
                >= 70 => "yellow",
                >= 50 => "orange1",
                _ => "red"
            };
            
            table.AddRow(
                $"[cyan]{cmd.CommandName}[/]",
                cmd.UsageCount.ToString(),
                $"[{successColor}]{successRate:F1}%[/]",
                string.Join(", ", cmd.Topics.Take(3)),
                cmd.LastUsed.ToString("MMM dd")
            );
        }
        
        AnsiConsole.Write(table);
        
        // Command usage chart
        if (topCommands.Count > 1)
        {
            var chart = new BarChart()
                .Width(50)
                .Label("[blue bold]Most Used Commands[/]")
                .CenterLabel();
            
            foreach (var cmd in topCommands.Take(5))
            {
                chart.AddItem(cmd.CommandName, cmd.UsageCount, Color.Blue);
            }
            
            AnsiConsole.Write(chart);
        }
    }

    private void ShowPracticeStats(ProgressData progressData)
    {
        AnsiConsole.MarkupLine("\n[yellow]🏃‍♂️ Practice Session Statistics[/]");
        
        if (!progressData.Practice.Any())
        {
            AnsiConsole.MarkupLine("[dim]No practice sessions recorded yet. Try 'gitmaster practice' to get started![/]");
            return;
        }
        
        var table = new Table();
        table.AddColumn("[bold]Scenario[/]");
        table.AddColumn("[bold]Attempts[/]");
        table.AddColumn("[bold]Completed[/]");
        table.AddColumn("[bold]Best Score[/]");
        table.AddColumn("[bold]Best Time[/]");
        table.AddColumn("[bold]Last Attempt[/]");
        
        foreach (var practice in progressData.Practice.Values.OrderByDescending(p => p.LastAttempt))
        {
            var completedAttempts = practice.Attempts.Count(a => a.Completed);
            var completionRate = practice.Attempts.Count > 0 
                ? (double)completedAttempts / practice.Attempts.Count * 100 
                : 0;
            
            var statusColor = practice.Completed ? "green" : completionRate > 50 ? "yellow" : "red";
            var statusIcon = practice.Completed ? "✓" : completionRate > 50 ? "⚡" : "○";
            
            table.AddRow(
                practice.ScenarioName,
                practice.Attempts.Count.ToString(),
                $"[{statusColor}]{statusIcon} {completionRate:F0}%[/]",
                practice.BestScore > 0 ? $"[green]{practice.BestScore}[/]" : "[dim]-[/]",
                practice.BestTime > TimeSpan.Zero ? $"[cyan]{practice.BestTime:mm\\:ss}[/]" : "[dim]-[/]",
                practice.LastAttempt.ToString("MMM dd")
            );
        }
        
        AnsiConsole.Write(table);
        
        // Practice scores chart
        var scenariosWithScores = progressData.Practice.Values
            .Where(p => p.BestScore > 0)
            .OrderByDescending(p => p.BestScore)
            .Take(5)
            .ToList();
        
        if (scenariosWithScores.Any())
        {
            var chart = new BarChart()
                .Width(50)
                .Label("[green bold]Best Scores[/]")
                .CenterLabel();
            
            foreach (var practice in scenariosWithScores)
            {
                var color = practice.BestScore switch
                {
                    >= 90 => Color.Green,
                    >= 70 => Color.Yellow,
                    >= 50 => Color.Orange1,
                    _ => Color.Red
                };
                
                chart.AddItem(practice.ScenarioName, practice.BestScore, color);
            }
            
            AnsiConsole.Write(chart);
        }
    }

    private void ShowDetailedProgress(ProgressData progressData, string? moduleFilter)
    {
        AnsiConsole.MarkupLine("\n[yellow]📖 Detailed Module Progress[/]");
        
        var modules = progressData.Modules.Values.AsEnumerable();
        
        if (!string.IsNullOrEmpty(moduleFilter))
        {
            modules = modules.Where(m => m.ModuleName.Contains(moduleFilter, StringComparison.OrdinalIgnoreCase));
            AnsiConsole.MarkupLine($"[dim]Filtering by: {moduleFilter}[/]");
        }
        
        foreach (var module in modules.OrderBy(m => m.ModuleName))
        {
            AnsiConsole.MarkupLine($"\n[bold cyan]{FormatModuleName(module.ModuleName)}[/]");
            
            var table = new Table();
            table.AddColumn("[bold]Lesson[/]");
            table.AddColumn("[bold]Status[/]");
            table.AddColumn("[bold]Score[/]");
            table.AddColumn("[bold]Time Spent[/]");
            table.AddColumn("[bold]Completed[/]");
            
            var completedLessons = module.LessonAttempts.OrderBy(a => a.LessonIndex).ToList();
            
            if (completedLessons.Any())
            {
                foreach (var lesson in completedLessons)
                {
                    var status = lesson.Completed ? "[green]✓ Complete[/]" : "[yellow]⚡ In Progress[/]";
                    var score = lesson.QuizScore > 0 ? $"[green]{lesson.QuizScore}%[/]" : "[dim]-[/]";
                    var timeSpent = lesson.TimeSpent > TimeSpan.Zero ? $"[cyan]{lesson.TimeSpent:hh\\:mm}[/]" : "[dim]-[/]";
                    var completed = lesson.CompletedTime?.ToString("MMM dd, HH:mm") ?? "[dim]Not finished[/]";
                    
                    table.AddRow(
                        $"Lesson {lesson.LessonIndex + 1}",
                        status,
                        score,
                        timeSpent,
                        completed
                    );
                }
            }
            else
            {
                table.AddRow("[dim]No lessons completed yet[/]", "[dim]-[/]", "[dim]-[/]", "[dim]-[/]", "[dim]-[/]");
            }
            
            AnsiConsole.Write(table);
            
            // Module stats
            var avgScore = completedLessons.Any() ? completedLessons.Average(l => l.QuizScore) : 0;
            var totalTime = module.TotalTimeSpent;
            
            AnsiConsole.MarkupLine($"[dim]Average Score: {avgScore:F1}% | Total Time: {totalTime:hh\\:mm\\:ss} | First Started: {module.FirstAccessed:MMM dd, yyyy}[/]");
        }
    }

    private void ExportProgress(ProgressData progressData, string format)
    {
        AnsiConsole.MarkupLine($"\n[green]📁 Exporting progress to {format.ToUpper()} format...[/]");
        
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var fileName = $"gitmaster-progress-{timestamp}";
            
            switch (format.ToLowerInvariant())
            {
                case "json":
                    var jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{fileName}.json");
                    var jsonContent = JsonSerializer.Serialize(progressData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(jsonPath, jsonContent);
                    AnsiConsole.MarkupLine($"[green]✓ Exported to: {jsonPath}[/]");
                    break;
                    
                case "csv":
                    var csvPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{fileName}.csv");
                    var csvContent = GenerateCsvContent(progressData);
                    File.WriteAllText(csvPath, csvContent);
                    AnsiConsole.MarkupLine($"[green]✓ Exported to: {csvPath}[/]");
                    break;
                    
                default:
                    AnsiConsole.MarkupLine($"[red]❌ Unsupported format: {format}[/]");
                    AnsiConsole.MarkupLine("[dim]Supported formats: json, csv[/]");
                    break;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Export failed: {ex.Message}[/]");
        }
    }
    
    private string GenerateCsvContent(ProgressData progressData)
    {
        var csv = new System.Text.StringBuilder();
        
        // Overall stats
        csv.AppendLine("Type,Name,Value,Details");
        csv.AppendLine($"Stat,Level,{progressData.Stats.Level},");
        csv.AppendLine($"Stat,ExperiencePoints,{progressData.Stats.ExperiencePoints},");
        csv.AppendLine($"Stat,ModulesCompleted,{progressData.Stats.ModulesCompleted},");
        csv.AppendLine($"Stat,PracticeSessionsCompleted,{progressData.Stats.PracticeSessionsCompleted},");
        csv.AppendLine($"Stat,CurrentStreak,{progressData.Streaks.CurrentStreak},");
        csv.AppendLine($"Stat,LongestStreak,{progressData.Streaks.LongestStreak},");
        
        // Module progress
        foreach (var module in progressData.Modules.Values)
        {
            var completedLessons = module.LessonAttempts.Count(a => a.Completed);
            csv.AppendLine($"Module,{module.ModuleName},{completedLessons},Completed: {module.IsCompleted}");
        }
        
        // Practice progress
        foreach (var practice in progressData.Practice.Values)
        {
            csv.AppendLine($"Practice,{practice.ScenarioName},{practice.BestScore},Completed: {practice.Completed}");
        }
        
        return csv.ToString();
    }
    
    private string FormatModuleName(string moduleName)
    {
        return moduleName switch
        {
            "basics" => "Git Fundamentals",
            "branching" => "Branch Management",
            "collaboration" => "Team Collaboration",
            "workflows" => "Git Workflows",
            "advanced" => "Advanced Techniques",
            "troubleshooting" => "Common Problems",
            _ => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(moduleName.Replace("-", " "))
        };
    }
}
