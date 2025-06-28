using GitMaster.Models;
using Spectre.Console;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;

namespace GitMaster.Services;

public class LessonService
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly string _lessonsPath;

    public LessonService()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
            
        _lessonsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "lessons");
    }

    public List<LearningModule> GetAvailableModules()
    {
        var indexPath = Path.Combine(_lessonsPath, "index.yml");
        if (!File.Exists(indexPath))
        {
            return GetFallbackModules();
        }

        try
        {
            var indexContent = File.ReadAllText(indexPath);
            var indexData = _yamlDeserializer.Deserialize<Dictionary<string, List<dynamic>>>(indexContent);
            
            var modules = new List<LearningModule>();
            
            if (indexData.ContainsKey("modules"))
            {
                foreach (var moduleData in indexData["modules"])
                {
                    var module = new LearningModule
                    {
                        Title = moduleData["title"]?.ToString() ?? "",
                        Description = moduleData["description"]?.ToString() ?? "",
                        Level = moduleData["level"]?.ToString() ?? "",
                        Duration = moduleData["duration"]?.ToString() ?? "",
                        Order = int.TryParse(moduleData["order"]?.ToString(), out int order) ? order : 0
                    };
                    modules.Add(module);
                }
            }
            
            return modules.OrderBy(m => m.Order).ToList();
        }
        catch
        {
            return GetFallbackModules();
        }
    }

    private List<LearningModule> GetFallbackModules()
    {
        return new List<LearningModule>
        {
            new() { Title = "Git Fundamentals", Description = "Learn basic Git concepts", Level = "Beginner", Duration = "2-3 hours", Order = 1 },
            new() { Title = "Branch Management", Description = "Master Git branching", Level = "Intermediate", Duration = "1-2 hours", Order = 2 },
            new() { Title = "Team Collaboration", Description = "Work with remotes and teams", Level = "Intermediate", Duration = "2-3 hours", Order = 3 },
            new() { Title = "Git Workflows", Description = "Learn popular workflows", Level = "Intermediate", Duration = "1-2 hours", Order = 4 },
            new() { Title = "Advanced Techniques", Description = "Master advanced features", Level = "Advanced", Duration = "3-4 hours", Order = 5 },
            new() { Title = "Common Problems", Description = "Troubleshoot Git issues", Level = "All levels", Duration = "1-2 hours", Order = 6 }
        };
    }

    public Lesson? LoadLesson(string moduleName)
    {
        var indexPath = Path.Combine(_lessonsPath, "index.yml");
        if (!File.Exists(indexPath))
        {
            return null;
        }

        try
        {
            var indexContent = File.ReadAllText(indexPath);
            var indexData = _yamlDeserializer.Deserialize<Dictionary<string, List<dynamic>>>(indexContent);
            
            if (!indexData.ContainsKey("modules"))
                return null;

            var moduleInfo = indexData["modules"]
                .FirstOrDefault(m => m["name"]?.ToString() == moduleName);
            
            if (moduleInfo == null)
                return null;

            var fileName = moduleInfo["file"]?.ToString();
            if (string.IsNullOrEmpty(fileName))
                return null;

            var filePath = Path.Combine(_lessonsPath, fileName);
            if (!File.Exists(filePath))
                return null;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".md" => ParseMarkdownLesson(filePath),
                ".json" => ParseJsonLesson(filePath),
                ".yml" or ".yaml" => ParseYamlLesson(filePath),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private Lesson? ParseMarkdownLesson(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var lines = content.Split('\n');
            
            var lesson = new Lesson();
            var currentSection = "";
            var currentExample = new LessonExample();
            var currentQuestion = new QuizQuestion();
            var exampleContent = new List<string>();
            var diagramContent = new List<string>();
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith("# "))
                {
                    lesson.Title = trimmedLine[2..].Trim();
                }
                else if (trimmedLine == "## Theory")
                {
                    currentSection = "theory";
                }
                else if (trimmedLine == "## Examples")
                {
                    currentSection = "examples";
                }
                else if (trimmedLine == "## Quiz")
                {
                    currentSection = "quiz";
                }
                else if (trimmedLine.StartsWith("### Example "))
                {
                    if (!string.IsNullOrEmpty(currentExample.Title))
                    {
                        lesson.Examples.Add(currentExample);
                    }
                    currentExample = new LessonExample 
                    { 
                        Title = trimmedLine[4..].Trim() 
                    };
                }
                else if (trimmedLine.StartsWith("### Question "))
                {
                    if (!string.IsNullOrEmpty(currentQuestion.Question))
                    {
                        lesson.Quiz.Questions.Add(currentQuestion);
                    }
                    currentQuestion = new QuizQuestion();
                }
                else if (currentSection == "theory" && !string.IsNullOrEmpty(trimmedLine))
                {
                    lesson.Theory += line + "\n";
                }
                else if (currentSection == "examples")
                {
                    ParseExampleSection(line, currentExample, exampleContent, diagramContent);
                }
                else if (currentSection == "quiz")
                {
                    ParseQuizSection(line, currentQuestion);
                }
            }
            
            // Add the last example and question if they exist
            if (!string.IsNullOrEmpty(currentExample.Title))
            {
                lesson.Examples.Add(currentExample);
            }
            if (!string.IsNullOrEmpty(currentQuestion.Question))
            {
                lesson.Quiz.Questions.Add(currentQuestion);
            }
            
            return lesson;
        }
        catch
        {
            return null;
        }
    }

    private void ParseExampleSection(string line, LessonExample example, List<string> exampleContent, List<string> diagramContent)
    {
        var trimmedLine = line.Trim();
        
        if (trimmedLine.StartsWith("**Description**:"))
        {
            example.Description = trimmedLine[16..].Trim();
        }
        else if (trimmedLine.StartsWith("**Code**:"))
        {
            exampleContent.Clear();
        }
        else if (trimmedLine.StartsWith("**Output**:"))
        {
            example.Code = string.Join("\n", exampleContent).Trim();
            exampleContent.Clear();
        }
        else if (trimmedLine.StartsWith("**Explanation**:"))
        {
            example.Output = string.Join("\n", exampleContent).Trim();
            example.Explanation = trimmedLine[17..].Trim();
            exampleContent.Clear();
        }
        else if (trimmedLine.StartsWith("**Diagram**:"))
        {
            diagramContent.Clear();
        }
        else if (trimmedLine.StartsWith("```") && diagramContent.Count > 0)
        {
            example.Diagram = new List<string>(diagramContent);
            diagramContent.Clear();
        }
        else if (exampleContent.Count > 0 || diagramContent.Count > 0)
        {
            if (diagramContent.Count > 0)
                diagramContent.Add(line);
            else
                exampleContent.Add(line);
        }
        else if (trimmedLine.StartsWith("```"))
        {
            if (example.Output == "")
                exampleContent.Add("");
            else
                diagramContent.Add("");
        }
    }

    private void ParseQuizSection(string line, QuizQuestion question)
    {
        var trimmedLine = line.Trim();
        
        if (!trimmedLine.StartsWith("**") && !trimmedLine.StartsWith("A)") && 
            !trimmedLine.StartsWith("B)") && !trimmedLine.StartsWith("C)") && 
            !trimmedLine.StartsWith("D)") && !string.IsNullOrEmpty(trimmedLine))
        {
            question.Question = trimmedLine;
        }
        else if (trimmedLine.StartsWith("A)") || trimmedLine.StartsWith("B)") || 
                 trimmedLine.StartsWith("C)") || trimmedLine.StartsWith("D)"))
        {
            question.Options.Add(trimmedLine[2..].Trim());
        }
        else if (trimmedLine.StartsWith("**Correct Answer**:"))
        {
            var answerLetter = trimmedLine[19..].Trim();
            question.CorrectAnswer = answerLetter switch
            {
                "A" => 0,
                "B" => 1, 
                "C" => 2,
                "D" => 3,
                _ => 0
            };
        }
        else if (trimmedLine.StartsWith("**Explanation**:"))
        {
            question.Explanation = trimmedLine[16..].Trim();
        }
    }

    private Lesson? ParseJsonLesson(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            
            var lesson = new Lesson
            {
                Title = jsonData?.GetValueOrDefault("title")?.ToString() ?? "",
                Theory = jsonData?.GetValueOrDefault("theory")?.ToString() ?? ""
            };

            if (jsonData.ContainsKey("examples"))
            {
                var examplesJson = JsonSerializer.Serialize(jsonData["examples"]);
                var examples = JsonSerializer.Deserialize<List<LessonExample>>(examplesJson);
                lesson.Examples = examples ?? new List<LessonExample>();
            }

            if (jsonData.ContainsKey("quiz"))
            {
                var quizJson = JsonSerializer.Serialize(jsonData["quiz"]);
                var quiz = JsonSerializer.Deserialize<Quiz>(quizJson);
                lesson.Quiz = quiz ?? new Quiz();
            }

            return lesson;
        }
        catch
        {
            return null;
        }
    }

    private Lesson? ParseYamlLesson(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            
            // Parse YAML and convert to lesson
            var yamlData = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);
            
            var lesson = new Lesson
            {
                Title = yamlData.GetValueOrDefault("title")?.ToString() ?? "",
                Theory = yamlData.GetValueOrDefault("theory")?.ToString() ?? ""
            };

            if (yamlData.ContainsKey("examples"))
            {
                var examplesYaml = _yamlDeserializer.Deserialize<List<LessonExample>>(
                    JsonSerializer.Serialize(yamlData["examples"]));
                lesson.Examples = examplesYaml ?? new List<LessonExample>();
            }

            if (yamlData.ContainsKey("quiz"))
            {
                var quizYaml = _yamlDeserializer.Deserialize<Quiz>(
                    JsonSerializer.Serialize(yamlData["quiz"]));
                lesson.Quiz = quizYaml ?? new Quiz();
            }

            return lesson;
        }
        catch
        {
            return null;
        }
    }
}
