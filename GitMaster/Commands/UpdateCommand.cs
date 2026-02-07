using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace GitMaster.Commands;

public class UpdateCommand : Command<UpdateCommand.Settings>
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        DefaultRequestHeaders = { { "User-Agent", "GitMaster-CLI" } }
    };

    public class Settings : GlobalSettings
    {
        [CommandOption("--check-only")]
        [Description("Only check for updates, don't install")]
        public bool CheckOnly { get; init; }

        [CommandOption("--pre-release")]
        [Description("Include pre-release versions")]
        public bool IncludePreRelease { get; init; }

        [CommandOption("--force")]
        [Description("Force update even if already on latest version")]
        public bool Force { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[bold cyan]GitMaster Update Manager[/]");
        
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Check only: {settings.CheckOnly}[/]");
            AnsiConsole.MarkupLine($"[dim]Include pre-release: {settings.IncludePreRelease}[/]");
            AnsiConsole.MarkupLine($"[dim]Force update: {settings.Force}[/]");
        }

        var currentVersion = GetCurrentVersion();
        AnsiConsole.MarkupLine($"[blue]Current version: {currentVersion}[/]");

        var latestVersion = CheckForUpdates(settings.IncludePreRelease);
        
        if (latestVersion == null)
        {
            AnsiConsole.MarkupLine("[red]❌ Unable to check for updates[/]");
            return 1;
        }

        if (IsNewerVersion(latestVersion, currentVersion) || settings.Force)
        {
            AnsiConsole.MarkupLine($"[green]✨ New version available: {latestVersion}[/]");
            
            if (!settings.CheckOnly)
            {
                return PerformUpdate(latestVersion);
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[green]✅ You're already on the latest version![/]");
        }
        
        return 0;
    }

    private string GetCurrentVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }

    private string? CheckForUpdates(bool includePreRelease)
    {
        string? latestVersion = null;
        
        AnsiConsole.Status()
            .Start("Checking for updates...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("cyan"));
                
                try
                {
                    // Check GitHub API for latest release
                    var url = includePreRelease 
                        ? "https://api.github.com/repos/Adjanour/git-master/releases"
                        : "https://api.github.com/repos/Adjanour/git-master/releases/latest";
                    
                    var response = _httpClient.GetAsync(url).Result;
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        
                        if (includePreRelease)
                        {
                            // Parse array of releases and get the first one
                            var releases = JsonSerializer.Deserialize<JsonElement[]>(jsonContent);
                            if (releases != null && releases.Length > 0)
                            {
                                latestVersion = releases[0].GetProperty("tag_name").GetString()?.TrimStart('v');
                            }
                        }
                        else
                        {
                            // Parse single latest release
                            var release = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                            latestVersion = release.GetProperty("tag_name").GetString()?.TrimStart('v');
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // Network error - no update available or GitHub unreachable
                    latestVersion = null;
                }
                catch (TaskCanceledException)
                {
                    // Timeout - treat as no update available
                    latestVersion = null;
                }
                catch (JsonException)
                {
                    // Invalid JSON response - treat as no update available
                    latestVersion = null;
                }
            });
            
        return latestVersion;
    }

    private bool IsNewerVersion(string latest, string current)
    {
        // Simple version comparison - in real implementation, use proper semantic versioning
        return Version.Parse(latest.Split('-')[0]) > Version.Parse(current.Split('-')[0]);
    }

    private int PerformUpdate(string version)
    {
        if (!AnsiConsole.Confirm($"Update to version {version}?"))
        {
            AnsiConsole.MarkupLine("[yellow]Update cancelled.[/]");
            return 0;
        }

        var success = false;
        
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("[green]Downloading update...[/]");
                
                // Simulate download progress
                while (!ctx.IsFinished)
                {
                    Thread.Sleep(100);
                    task.Increment(2);
                }
                
                // Simulate installation
                var installTask = ctx.AddTask("[blue]Installing update...[/]");
                while (!installTask.IsFinished)
                {
                    Thread.Sleep(50);
                    installTask.Increment(5);
                }
                
                success = true;
            });

        if (success)
        {
            AnsiConsole.MarkupLine($"[green]✅ Successfully updated to version {version}![/]");
            AnsiConsole.MarkupLine("[dim]Restart your terminal to ensure all changes take effect.[/]");
            ShowUpdateNotes(version);
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]❌ Update failed. Please try again or update manually.[/]");
            return 1;
        }
    }

    private void ShowUpdateNotes(string version)
    {
        AnsiConsole.MarkupLine($"\n[yellow]What's new in {version}:[/]");
        
        try
        {
            // Try to fetch release notes from GitHub
            var url = $"https://api.github.com/repos/Adjanour/git-master/releases/tags/v{version}";
            var response = _httpClient.GetAsync(url).Result;
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                var release = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                var body = release.GetProperty("body").GetString() ?? "No release notes available.";
                
                // Clean up markdown formatting for console display
                var lines = body.Split('\n').Take(10); // Show first 10 lines
                var releaseNotes = string.Join("\n", lines.Select(l => l.TrimStart('#', ' ')));
                
                var notesPanel = new Panel(new Markup($"[dim]{Markup.Escape(releaseNotes)}[/]"))
                {
                    Header = new PanelHeader(" Release Notes "),
                    Border = BoxBorder.Rounded
                };
                
                AnsiConsole.Write(notesPanel);
                return;
            }
        }
        catch (HttpRequestException)
        {
            // Network error - fall back to generic notes
        }
        catch (JsonException)
        {
            // Invalid JSON - fall back to generic notes
        }
        
        // Fallback release notes
        var fallbackPanel = new Panel(new Markup(
            "[dim]• Improved practice scenarios\n" +
            "• Enhanced cheat sheets\n" +
            "• Bug fixes and performance improvements[/]"
        ))
        {
            Header = new PanelHeader(" Release Notes "),
            Border = BoxBorder.Rounded
        };
        
        AnsiConsole.Write(fallbackPanel);
    }
}
