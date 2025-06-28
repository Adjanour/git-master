using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitMaster.Commands;

namespace GitMaster;

// Global settings available to all commands
public class GlobalSettings : CommandSettings
{
    [CommandOption("--repo-path")]
    [Description("Path to the Git repository (defaults to current directory)")]
    public string? RepoPath { get; init; }

    [CommandOption("--verbose")]
    [Description("Enable verbose output")]
    public bool Verbose { get; init; }

    [CommandOption("--color")]
    [Description("Enable colored output (auto/always/never)")]
    [DefaultValue("auto")]
    public string Color { get; init; } = "auto";
}

class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        
        app.Configure(config =>
        {
            config.SetApplicationName("gitmaster");
            config.SetApplicationVersion("1.0.0");
            
            // Add commands
            config.AddCommand<PracticeCommand>("practice")
                .WithDescription("Start interactive Git practice sessions");
                
            config.AddCommand<CheatCommand>("cheat")
                .WithDescription("Show Git cheat sheet for specific topics")
                .WithExample(new[] { "cheat", "merge" })
                .WithExample(new[] { "cheat", "rebase" });
                
            config.AddCommand<LearnCommand>("learn")
                .WithDescription("Access structured learning modules")
                .WithExample(new[] { "learn", "basics" })
                .WithExample(new[] { "learn", "branching" });
                
            config.AddCommand<ProgressCommand>("progress")
                .WithDescription("View your learning progress and statistics");
                
            config.AddCommand<ResetProgressCommand>("reset-progress")
                .WithDescription("Reset your learning progress");
                
            config.AddCommand<UpdateCommand>("update")
                .WithDescription("Update GitMaster to the latest version");
        });
        
        return app.Run(args);
    }
}
