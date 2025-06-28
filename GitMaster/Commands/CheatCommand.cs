using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using GitMaster.Services;

namespace GitMaster.Commands;

public class CheatCommand : Command<CheatCommand.Settings>
{
    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "[topic]")]
        [Description("Git topic to show cheat sheet for, or 'search' for fuzzy search")]
        public string? Topic { get; init; }

        [CommandArgument(1, "[search-term]")]
        [Description("Search term when using 'search' as topic")]
        public string? SearchTerm { get; init; }

        [CommandOption("-f|--format")]
        [Description("Output format (table/list/markdown)")]
        [DefaultValue("table")]
        public string Format { get; init; } = "table";

        [CommandOption("-s|--search")]
        [Description("Search for specific commands within the topic")]
        public string? Search { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var cheatSheetService = new CheatSheetService();
            var renderer = new CheatSheetRenderer();

            if (settings.Verbose)
            {
                AnsiConsole.MarkupLine($"[dim]Topic: {settings.Topic}[/]");
                AnsiConsole.MarkupLine($"[dim]Format: {settings.Format}[/]");
                if (!string.IsNullOrEmpty(settings.Search))
                {
                    AnsiConsole.MarkupLine($"[dim]Search filter: {settings.Search}[/]");
                }
            }

            // Handle different command patterns
            if (string.IsNullOrEmpty(settings.Topic))
            {
                // Show all topics
                var topics = cheatSheetService.GetAllTopics();
                renderer.RenderTopicsList(topics);
            }
            else if (settings.Topic.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                // Global search mode: gitmaster cheat search "rebase"
                if (string.IsNullOrEmpty(settings.SearchTerm))
                {
                    renderer.RenderError("Search term is required. Usage: gitmaster cheat search \"term\"");
                    return 1;
                }
                
                var searchResults = cheatSheetService.SearchCommands(settings.SearchTerm);
                renderer.RenderSearchResults(searchResults, settings.SearchTerm);
            }
            else
            {
                // Topic-specific view
                var topic = cheatSheetService.GetTopic(settings.Topic);
                
                if (topic == null)
                {
                    renderer.RenderError($"Topic '{settings.Topic}' not found.");
                    
                    // Suggest similar topics
                    var suggestions = cheatSheetService.FindSimilarTopics(settings.Topic);
                    renderer.RenderSuggestions(suggestions, settings.Topic);
                    
                    return 1;
                }

                // If search filter is provided, filter commands within the topic
                if (!string.IsNullOrEmpty(settings.Search))
                {
                    var filteredCommands = cheatSheetService.SearchCommandsInTopic(settings.Topic, settings.Search);
                    renderer.RenderSearchResults(filteredCommands, settings.Search);
                }
                else
                {
                    renderer.RenderTopic(settings.Topic, topic, settings.Format);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            var renderer = new CheatSheetRenderer();
            renderer.RenderError($"Failed to load cheat sheet: {ex.Message}");
            
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(ex);
            }
            
            return 1;
        }
    }
}
