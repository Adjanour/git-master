using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace GitMaster.Commands;

public class ResetProgressCommand : Command<ResetProgressCommand.Settings>
{
    public class Settings : GlobalSettings
    {
        [CommandOption("-m|--module")]
        [Description("Reset progress for specific module only")]
        public string? Module { get; init; }

        [CommandOption("-f|--force")]
        [Description("Force reset without confirmation")]
        public bool Force { get; init; }

        [CommandOption("--backup")]
        [Description("Create backup before reset")]
        public bool CreateBackup { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[bold red]GitMaster Progress Reset[/]");
        
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Module filter: {settings.Module ?? "all"}[/]");
            AnsiConsole.MarkupLine($"[dim]Force reset: {settings.Force}[/]");
            AnsiConsole.MarkupLine($"[dim]Create backup: {settings.CreateBackup}[/]");
        }

        // Show warning
        if (string.IsNullOrEmpty(settings.Module))
        {
            AnsiConsole.MarkupLine("[red]⚠️  This will reset ALL your learning progress![/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]⚠️  This will reset progress for module: {settings.Module}[/]");
        }

        // Confirm unless forced
        if (!settings.Force)
        {
            if (!AnsiConsole.Confirm("Are you sure you want to continue?"))
            {
                AnsiConsole.MarkupLine("[green]Reset cancelled.[/]");
                return 0;
            }
        }

        // Create backup if requested
        if (settings.CreateBackup)
        {
            CreateProgressBackup();
        }

        // Perform reset
        PerformReset(settings.Module);
        
        return 0;
    }

    private void CreateProgressBackup()
    {
        AnsiConsole.MarkupLine("[blue]Creating progress backup...[/]");
        
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFile = $"gitmaster_progress_backup_{timestamp}.json";
            
            // Get the progress file path
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var gitMasterPath = Path.Combine(localAppDataPath, "GitMaster");
            var progressPath = Path.Combine(gitMasterPath, "progress.json");
            
            if (File.Exists(progressPath))
            {
                // Create backup in the same directory
                var backupPath = Path.Combine(gitMasterPath, backupFile);
                File.Copy(progressPath, backupPath, overwrite: true);
                
                AnsiConsole.MarkupLine($"[green]✓ Backup created: {backupPath}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]⚠️  No progress file found to backup[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Backup failed: {ex.Message}[/]");
        }
    }

    private void PerformReset(string? module)
    {
        var progressService = new GitMaster.Services.ProgressService();
        
        AnsiConsole.Status()
            .Start("Resetting progress...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("green"));
                
                Thread.Sleep(500); // Brief delay for visual feedback
                
                if (string.IsNullOrEmpty(module))
                {
                    // Reset all progress
                    ctx.Status("Clearing all modules...");
                    Thread.Sleep(300);
                    ctx.Status("Resetting practice data...");
                    Thread.Sleep(300);
                    ctx.Status("Clearing statistics...");
                    Thread.Sleep(300);
                    
                    progressService.ResetAllProgress();
                }
                else
                {
                    // Reset specific module
                    ctx.Status($"Resetting module: {module}...");
                    Thread.Sleep(500);
                    
                    progressService.ResetProgress(module);
                }
            });

        if (string.IsNullOrEmpty(module))
        {
            AnsiConsole.MarkupLine("[green]✓ All progress has been reset![/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]✓ Progress for module '{module}' has been reset![/]");
        }
        
        AnsiConsole.MarkupLine("[dim]You can start fresh with 'gitmaster learn' or 'gitmaster practice'[/]");
    }
}
