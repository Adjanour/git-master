using GitMaster.Models;
using Spectre.Console;

namespace GitMaster.Services;

public class LessonRenderer
{
    public void RenderLesson(Lesson lesson)
    {
        // Clear screen and show lesson title
        AnsiConsole.Clear();
        
        var rule = new Rule($"[bold blue]{lesson.Title}[/]")
        {
            Style = Style.Parse("blue")
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Render theory section
        RenderTheory(lesson.Theory);
        
        // Render examples
        if (lesson.Examples.Any())
        {
            AnsiConsole.WriteLine();
            var examplesRule = new Rule("[bold yellow]📚 Examples[/]")
            {
                Style = Style.Parse("yellow")
            };
            AnsiConsole.Write(examplesRule);
            
            foreach (var example in lesson.Examples)
            {
                RenderExample(example);
            }
        }

        // Render quiz
        if (lesson.Quiz.Questions.Any())
        {
            AnsiConsole.WriteLine();
            var quizRule = new Rule("[bold green]🧠 Knowledge Check[/]")
            {
                Style = Style.Parse("green")
            };
            AnsiConsole.Write(quizRule);
            
            RenderQuiz(lesson.Quiz);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private void RenderTheory(string theory)
    {
        if (string.IsNullOrEmpty(theory))
            return;

        var panel = new Panel(FormatText(theory))
        {
            Header = new PanelHeader("[bold cyan]📖 Theory[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };
        
        AnsiConsole.Write(panel);
    }

    private void RenderExample(LessonExample example)
    {
        AnsiConsole.WriteLine();
        
        // Example title and description
        AnsiConsole.MarkupLine($"[bold yellow]▶ {Markup.Escape(example.Title)}[/]");
        if (!string.IsNullOrEmpty(example.Description))
        {
            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(example.Description)}[/]");
        }
        AnsiConsole.WriteLine();

        // Create a grid layout for side-by-side display
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(2));
        grid.AddColumn(new GridColumn().NoWrap());

        // Code section
        var codePanel = new Panel(FormatCodeBlock(example.Code))
        {
            Header = new PanelHeader("[bold green]💻 Code[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("green")
        };

        // Output section
        var outputPanel = new Panel(FormatCodeBlock(example.Output))
        {
            Header = new PanelHeader("[bold blue]📤 Output[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue")
        };

        grid.AddRow(codePanel, outputPanel);
        AnsiConsole.Write(grid);

        // Explanation
        if (!string.IsNullOrEmpty(example.Explanation))
        {
            AnsiConsole.WriteLine();
            var explanationPanel = new Panel(FormatText(example.Explanation))
            {
                Header = new PanelHeader("[bold magenta]💡 Explanation[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("magenta")
            };
            AnsiConsole.Write(explanationPanel);
        }

        // ASCII Diagram
        if (example.Diagram.Any())
        {
            AnsiConsole.WriteLine();
            RenderDiagram(example.Diagram);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key for next example...[/]");
        Console.ReadKey(true);
    }

    private void RenderDiagram(List<string> diagramLines)
    {
        var diagramText = string.Join("\n", diagramLines);
        
        var diagramPanel = new Panel(FormatDiagram(diagramText))
        {
            Header = new PanelHeader("[bold cyan]📊 Diagram[/]"),
            Border = BoxBorder.Double,
            BorderStyle = Style.Parse("cyan")
        };
        
        AnsiConsole.Write(diagramPanel);
    }

    private void RenderQuiz(Quiz quiz)
    {
        var correctAnswers = 0;
        
        for (int i = 0; i < quiz.Questions.Count; i++)
        {
            var question = quiz.Questions[i];
            AnsiConsole.WriteLine();
            
            // Question header
            AnsiConsole.MarkupLine($"[bold yellow]Question {i + 1}:[/] {Markup.Escape(question.Question)}");
            AnsiConsole.WriteLine();

            // Options
            var options = question.Options.Select((option, index) => 
                $"{(char)('A' + index)}) {option}").ToArray();

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose your answer:")
                    .AddChoices(options)
                    .HighlightStyle(Style.Parse("green"))
            );

            var selectedIndex = options.ToList().FindIndex(o => o == selectedOption);
            
            // Check answer
            if (selectedIndex == question.CorrectAnswer)
            {
                AnsiConsole.MarkupLine("[bold green]✅ Correct![/]");
                correctAnswers++;
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold red]❌ Incorrect. The correct answer is {(char)('A' + question.CorrectAnswer)})[/]");
            }

            // Show explanation
            if (!string.IsNullOrEmpty(question.Explanation))
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[dim]💡 {Markup.Escape(question.Explanation)}[/]");
            }

            AnsiConsole.WriteLine();
            if (i < quiz.Questions.Count - 1)
            {
                AnsiConsole.MarkupLine("[dim]Press any key for next question...[/]");
                Console.ReadKey(true);
            }
        }

        // Quiz results
        AnsiConsole.WriteLine();
        var percentage = (double)correctAnswers / quiz.Questions.Count * 100;
        var resultColor = percentage >= 80 ? "green" : percentage >= 60 ? "yellow" : "red";
        
        var resultPanel = new Panel($"[bold {resultColor}]Quiz Results: {correctAnswers}/{quiz.Questions.Count} ({percentage:F0}%)[/]")
        {
            Border = BoxBorder.Double,
            BorderStyle = Style.Parse(resultColor)
        };
        
        AnsiConsole.Write(resultPanel);
    }

    private string FormatText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        // Process markdown-like formatting
        text = text.Replace("**", "[bold]").Replace("**", "[/]");
        text = text.Replace("`", "[green]").Replace("`", "[/]");
        
        // Handle headers
        var lines = text.Split('\n');
        var formattedLines = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("### "))
            {
                formattedLines.Add($"[bold yellow]{Markup.Escape(trimmedLine[4..])}[/]");
            }
            else if (trimmedLine.StartsWith("## "))
            {
                formattedLines.Add($"[bold cyan]{Markup.Escape(trimmedLine[3..])}[/]");
            }
            else if (trimmedLine.StartsWith("# "))
            {
                formattedLines.Add($"[bold blue]{Markup.Escape(trimmedLine[2..])}[/]");
            }
            else if (trimmedLine.StartsWith("- "))
            {
                formattedLines.Add($"  [yellow]•[/] {Markup.Escape(trimmedLine[2..])}");
            }
            else if (!string.IsNullOrEmpty(trimmedLine))
            {
                formattedLines.Add(Markup.Escape(line));
            }
            else
            {
                formattedLines.Add("");
            }
        }
        
        return string.Join("\n", formattedLines);
    }

    private string FormatCodeBlock(string code)
    {
        if (string.IsNullOrEmpty(code))
            return "[dim]No code provided[/]";

        // Remove markdown code block markers if present
        var lines = code.Split('\n');
        if (lines.Length > 0 && lines[0].Trim().StartsWith("```"))
        {
            lines = lines.Skip(1).ToArray();
        }
        if (lines.Length > 0 && lines[^1].Trim() == "```")
        {
            lines = lines.Take(lines.Length - 1).ToArray();
        }

        var formattedLines = lines.Select(line =>
        {
            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith("#"))
            {
                return $"[dim]{Markup.Escape(line)}[/]";
            }
            else if (trimmedLine.StartsWith("git "))
            {
                return $"[bold cyan]{Markup.Escape(line)}[/]";
            }
            else
            {
                return Markup.Escape(line);
            }
        });

        return string.Join("\n", formattedLines);
    }

    private string FormatDiagram(string diagram)
    {
        if (string.IsNullOrEmpty(diagram))
            return "";

        // Enhance ASCII diagrams with colors
        var lines = diagram.Split('\n');
        var formattedLines = lines.Select(line =>
        {
            var formatted = line;
            
            //// Color arrows and connections
            //formatted = formatted.Replace("→", "[bold yellow]→[/]");
            //formatted = formatted.Replace("▼", "[bold yellow]▼[/]");
            //formatted = formatted.Replace("►", "[bold yellow]►[/]");
            //formatted = formatted.Replace("├", "[bold blue]├[/]");
            //formatted = formatted.Replace("└", "[bold blue]└[/]");
            //formatted = formatted.Replace("│", "[bold blue]│[/]");
            //formatted = formatted.Replace("─", "[bold blue]─[/]");
            
            //// Color git terms
            //if (formatted.Contains("main"))
            //    formatted = formatted.Replace("main", "[bold green]main[/]");
            //if (formatted.Contains("feature"))
            //    formatted = formatted.Replace("feature", "[bold cyan]feature[/]");
            //if (formatted.Contains("HEAD"))
            //    formatted = formatted.Replace("HEAD", "[bold red]HEAD[/]");
                
            return formatted;
        });

        return string.Join("\n", formattedLines);
    }
}
