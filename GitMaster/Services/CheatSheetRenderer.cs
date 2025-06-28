using Spectre.Console;
using GitMaster.Models;

namespace GitMaster.Services;

public class CheatSheetRenderer
{
    public void RenderTopicsList(Dictionary<string, Topic> topics)
    {
        AnsiConsole.MarkupLine("[bold cyan]Available Cheat Sheet Topics[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn(new TableColumn("[bold]Topic[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Title[/]"));
        table.AddColumn(new TableColumn("[bold]Description[/]").Width(50));
        table.AddColumn(new TableColumn("[bold]Commands[/]").Centered());

        foreach (var (key, topic) in topics.OrderBy(t => t.Key))
        {
            table.AddRow(
                $"[yellow]{key}[/]",
                $"[bold]{topic.Title}[/]",
                $"[dim]{topic.Description}[/]",
                $"[blue]{topic.Commands.Count}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[dim]Usage: [/][bold]gitmaster cheat <topic>[/][dim] or [/][bold]gitmaster cheat search \"<term>\"[/]");
    }

    public void RenderTopic(string topicName, Topic topic, string format = "table")
    {
        AnsiConsole.MarkupLine($"[bold cyan]{topic.Title}[/]");
        AnsiConsole.MarkupLine($"[dim]{topic.Description}[/]");
        AnsiConsole.WriteLine();

        switch (format.ToLowerInvariant())
        {
            case "list":
                RenderTopicAsList(topic);
                break;
            case "markdown":
                RenderTopicAsMarkdown(topic);
                break;
            case "table":
            default:
                RenderTopicAsTable(topic);
                break;
        }
    }

    public void RenderSearchResults(List<Command> commands, string searchTerm)
    {
        if (!commands.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No commands found matching '[/][bold]{searchTerm}[/][yellow]'[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[bold cyan]Search results for '[/][bold yellow]{searchTerm}[/][bold cyan]'[/] [dim]({commands.Count} found)[/]");
        AnsiConsole.WriteLine();

        foreach (var command in commands)
        {
            RenderCommand(command);
            AnsiConsole.WriteLine();
        }
    }

    private void RenderTopicAsTable(Topic topic)
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn(new TableColumn("[bold]Command[/]").Width(25));
        table.AddColumn(new TableColumn("[bold]Syntax[/]").Width(35));
        table.AddColumn(new TableColumn("[bold]Description[/]").Width(40));

        foreach (var command in topic.Commands)
        {
            table.AddRow(
                HighlightCommand(command.Name),
                HighlightSyntax(command.Syntax),
                command.Description
            );
        }

        AnsiConsole.Write(table);

        // Show examples for each command
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Examples:[/]");
        foreach (var command in topic.Commands)
        {
            if (command.Examples.Any())
            {
                AnsiConsole.MarkupLine($"\n[bold yellow]{command.Name}[/]:");
                RenderExamples(command.Examples);
            }
        }
    }

    private void RenderTopicAsList(Topic topic)
    {
        foreach (var command in topic.Commands)
        {
            RenderCommand(command);
            AnsiConsole.WriteLine();
        }
    }

    private void RenderTopicAsMarkdown(Topic topic)
    {
        foreach (var command in topic.Commands)
        {
            AnsiConsole.MarkupLine($"## {command.Name}");
            AnsiConsole.MarkupLine($"**Syntax:** `{command.Syntax}`");
            AnsiConsole.MarkupLine($"**Description:** {command.Description}");
            
            if (command.Examples.Any())
            {
                AnsiConsole.MarkupLine("**Examples:**");
                foreach (var example in command.Examples)
                {
                    AnsiConsole.MarkupLine($"- `{example.CommandText}` - {example.Description}");
                }
            }
            
            if (command.Tags.Any())
            {
                AnsiConsole.MarkupLine($"**Tags:** {string.Join(", ", command.Tags)}");
            }
            
            AnsiConsole.WriteLine();
        }
    }

    private void RenderCommand(Command command)
    {
        var panel = new Panel(RenderCommandContent(command))
        {
            Header = new PanelHeader(HighlightCommand(command.Name)),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue")
        };

        AnsiConsole.Write(panel);
    }

    private string RenderCommandContent(Command command)
    {
        var content = new List<string>
        {
            $"[bold]Syntax:[/] {HighlightSyntax(command.Syntax)}",
            $"[bold]Description:[/] {command.Description}"
        };

        if (command.Examples.Any())
        {
            content.Add("\n[bold]Examples:[/]");
            foreach (var example in command.Examples)
            {
                content.Add($"  [green]{example.CommandText}[/]");
                content.Add($"  [dim]└─ {example.Description}[/]");
            }
        }

        if (command.Tags.Any())
        {
            var tags = string.Join(" ", command.Tags.Select(tag => $"[dim]#{tag}[/]"));
            content.Add($"\n[bold]Tags:[/] {tags}");
        }

        return string.Join("\n", content);
    }

    private void RenderExamples(List<Example> examples)
    {
        foreach (var example in examples)
        {
            AnsiConsole.MarkupLine($"  [green]{example.CommandText}[/]");
            AnsiConsole.MarkupLine($"  [dim]└─ {example.Description}[/]");
        }
    }

    private string HighlightCommand(string command)
    {
        // Highlight git commands
        if (command.StartsWith("git "))
        {
            var parts = command.Split(' ');
            return $"[bold green]{parts[0]}[/] [bold yellow]{string.Join(" ", parts.Skip(1))}[/]";
        }
        
        return $"[bold yellow]{command}[/]";
    }

    private string HighlightSyntax(string syntax)
    {
        // Just escape any markup characters to avoid conflicts
        return syntax.EscapeMarkup();
    }

    public void RenderError(string message)
    {
        AnsiConsole.MarkupLine($"[bold red]Error:[/] {message}");
    }

    public void RenderSuggestions(List<string> suggestions, string searchTerm)
    {
        if (!suggestions.Any()) return;

        AnsiConsole.MarkupLine($"\n[yellow]Did you mean one of these topics?[/]");
        foreach (var suggestion in suggestions.Take(5))
        {
            AnsiConsole.MarkupLine($"  [blue]gitmaster cheat {suggestion}[/]");
        }
    }
}
