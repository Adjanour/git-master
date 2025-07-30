using System.Collections.Immutable;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;
using Microsoft.Extensions.Logging;

namespace GitMaster.Internal.Lessons;

/// <summary>
/// YAML-based implementation of the lesson service that parses assets/lessons/index.yml
/// and referenced Markdown, JSON, and YAML lesson files with graceful fallback handling
/// </summary>
public class YamlLessonService : ILessonService
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly string _lessonsPath;
    private readonly ILogger<YamlLessonService>? _logger;
    private readonly Dictionary<string, DateTime> _completedChapters;

    public YamlLessonService(ILogger<YamlLessonService>? logger = null)
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
            
        _lessonsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "lessons");
        _logger = logger;
        _completedChapters = new Dictionary<string, DateTime>();
    }

    public async Task<ImmutableList<LessonModule>> GetAvailableModulesAsync()
    {
        var indexPath = Path.Combine(_lessonsPath, "index.yml");
        
        if (!File.Exists(indexPath))
        {
            _logger?.LogWarning("Index file not found at {IndexPath}. Using fallback modules.", indexPath);
            return GetFallbackModules();
        }

        try
        {
            var indexContent = await File.ReadAllTextAsync(indexPath);
            var indexData = _yamlDeserializer.Deserialize<Dictionary<string, object>>(indexContent);
            
            if (!indexData.ContainsKey("modules"))
            {
                _logger?.LogWarning("No 'modules' key found in index file. Using fallback modules.");
                return GetFallbackModules();
            }

            var modules = new List<LessonModule>();
            
            if (indexData["modules"] is List<object> modulesList)
            {
                foreach (var moduleData in modulesList)
                {
                    try
                    {
                        var module = CreateModuleFromIndexData(moduleData);
                        if (module != null)
                        {
                            modules.Add(module);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to create module from index data");
                    }
                }
            }
            
            return modules.OrderBy(m => m.Order).ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse index file at {IndexPath}. Using fallback modules.", indexPath);
            return GetFallbackModules();
        }
    }

    public async Task<LessonModule?> LoadModuleAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var indexPath = Path.Combine(_lessonsPath, "index.yml");
        
        if (!File.Exists(indexPath))
        {
            _logger?.LogWarning("Index file not found at {IndexPath}", indexPath);
            return null;
        }

        try
        {
            var indexContent = await File.ReadAllTextAsync(indexPath);
            var indexData = _yamlDeserializer.Deserialize<Dictionary<string, object>>(indexContent);
            
            if (!indexData.ContainsKey("modules"))
            {
                return null;
            }

            if (indexData["modules"] is List<object> modulesList)
            {
                var moduleInfo = modulesList
                    .Cast<Dictionary<object, object>>()
                    .FirstOrDefault(m => m.GetValueOrDefault("name")?.ToString() == name);
                
                if (moduleInfo == null)
                {
                    _logger?.LogWarning("Module '{ModuleName}' not found in index", name);
                    return null;
                }

                var module = CreateModuleFromIndexData(moduleInfo);
                if (module == null)
                {
                    return null;
                }

                // Load the actual lesson content
                var fileName = moduleInfo.GetValueOrDefault("file")?.ToString();
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger?.LogWarning("No file specified for module '{ModuleName}'", name);
                    return module;
                }

                var filePath = Path.Combine(_lessonsPath, fileName);
                if (!File.Exists(filePath))
                {
                    _logger?.LogWarning("Lesson file not found at {FilePath} for module '{ModuleName}'", filePath, name);
                    return module;
                }

                var content = await LoadLessonContentFromFile(filePath);
                if (content == null)
                {
                    _logger?.LogWarning("Failed to load content from {FilePath} for module '{ModuleName}'", filePath, name);
                    return module;
                }

                // Create a single chapter for the entire lesson content
                var chapter = new LessonChapter(
                    ChapterNumber: 1,
                    Title: module.Title,
                    Content: content
                );

                return module with { Chapters = ImmutableList.Create(chapter) };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load module '{ModuleName}'", name);
            return null;
        }
    }

    public async Task<ChapterCompletionResult?> MarkChapterCompleteAsync(string moduleName, int chapterNumber)
    {
        if (string.IsNullOrWhiteSpace(moduleName) || chapterNumber <= 0)
        {
            return null;
        }

        try
        {
            var key = $"{moduleName}:{chapterNumber}";
            var completedAt = DateTime.UtcNow;
            
            _completedChapters[key] = completedAt;
            
            _logger?.LogInformation("Marked chapter {ChapterNumber} of module '{ModuleName}' as completed", chapterNumber, moduleName);
            
            return new ChapterCompletionResult(
                ModuleName: moduleName,
                ChapterNumber: chapterNumber,
                CompletedAt: completedAt,
                QuizScore: 0, // Would be calculated from actual quiz completion
                TimeSpent: TimeSpan.Zero // Would be tracked from session start
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to mark chapter {ChapterNumber} of module '{ModuleName}' as completed", chapterNumber, moduleName);
            return null;
        }
    }

    private LessonModule? CreateModuleFromIndexData(object moduleData)
    {
        try
        {
            if (moduleData is Dictionary<object, object> moduleDict)
            {
                return new LessonModule(
                    Name: moduleDict.GetValueOrDefault("name")?.ToString() ?? string.Empty,
                    Title: moduleDict.GetValueOrDefault("title")?.ToString() ?? string.Empty,
                    Description: moduleDict.GetValueOrDefault("description")?.ToString() ?? string.Empty,
                    Level: moduleDict.GetValueOrDefault("level")?.ToString() ?? string.Empty,
                    Duration: moduleDict.GetValueOrDefault("duration")?.ToString() ?? string.Empty,
                    FileName: moduleDict.GetValueOrDefault("file")?.ToString() ?? string.Empty,
                    Order: int.TryParse(moduleDict.GetValueOrDefault("order")?.ToString(), out int order) ? order : 0,
                    Chapters: ImmutableList<LessonChapter>.Empty
                );
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create module from index data");
            return null;
        }
    }

    private async Task<LessonContent?> LoadLessonContentFromFile(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".md" => await ParseMarkdownLessonAsync(filePath),
                ".json" => await ParseJsonLessonAsync(filePath),
                ".yml" or ".yaml" => await ParseYamlLessonAsync(filePath),
                _ => await HandleUnknownFormatAsync(filePath)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load lesson content from {FilePath}", filePath);
            return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
        }
    }

    private async Task<LessonContent?> ParseMarkdownLessonAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var lines = content.Split('\n');
            
            var theory = new List<string>();
            var examples = new List<LessonExample>();
            var questions = new List<QuizQuestion>();
            
            var currentSection = "";
            var currentExample = new Dictionary<string, object>();
            var currentQuestion = new Dictionary<string, object>();
            var currentOptions = new List<string>();
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Section headers
                if (trimmedLine == "## Theory")
                {
                    currentSection = "theory";
                    continue;
                }
                else if (trimmedLine == "## Examples")
                {
                    currentSection = "examples";
                    continue;
                }
                else if (trimmedLine == "## Quiz")
                {
                    currentSection = "quiz";
                    continue;
                }
                
                // Process content based on current section
                switch (currentSection)
                {
                    case "theory":
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            theory.Add(line);
                        }
                        break;
                        
                    case "examples":
                        ProcessMarkdownExample(line, examples, currentExample);
                        break;
                        
                    case "quiz":
                        ProcessMarkdownQuiz(line, questions, currentQuestion, currentOptions);
                        break;
                }
            }
            
            // Add the last question if exists
            if (currentQuestion.ContainsKey("question"))
            {
                AddQuizQuestion(questions, currentQuestion, currentOptions);
            }
            
            return new LessonContent(
                Theory: string.Join("\n", theory),
                Examples: examples.ToImmutableList(),
                Quiz: new LessonQuiz(questions.ToImmutableList())
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse Markdown lesson from {FilePath}", filePath);
            return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
        }
    }

    private async Task<LessonContent?> ParseJsonLessonAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            
            if (jsonData == null)
            {
                return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
            }
            
            var theory = jsonData.GetValueOrDefault("theory")?.ToString() ?? string.Empty;
            var examples = ParseJsonExamples(jsonData.GetValueOrDefault("examples"));
            var quiz = ParseJsonQuiz(jsonData.GetValueOrDefault("quiz"));
            
            return new LessonContent(
                Theory: theory,
                Examples: examples,
                Quiz: quiz
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse JSON lesson from {FilePath}", filePath);
            return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
        }
    }

    private async Task<LessonContent?> ParseYamlLessonAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var yamlData = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);
            
            if (yamlData == null)
            {
                return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
            }
            
            var theory = yamlData.GetValueOrDefault("theory")?.ToString() ?? string.Empty;
            var examples = ParseYamlExamples(yamlData.GetValueOrDefault("examples"));
            var quiz = ParseYamlQuiz(yamlData.GetValueOrDefault("quiz"));
            
            return new LessonContent(
                Theory: theory,
                Examples: examples,
                Quiz: quiz
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse YAML lesson from {FilePath}", filePath);
            return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
        }
    }

    private async Task<LessonContent?> HandleUnknownFormatAsync(string filePath)
    {
        _logger?.LogWarning("Unknown file format for {FilePath}. Attempting to read as plain text.", filePath);
        
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            return new LessonContent(
                Theory: content,
                Examples: ImmutableList<LessonExample>.Empty,
                Quiz: LessonQuiz.Empty
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to read unknown format file {FilePath}", filePath);
            return CreateFallbackContent(Path.GetFileNameWithoutExtension(filePath));
        }
    }

    private void ProcessMarkdownExample(string line, List<LessonExample> examples, Dictionary<string, object> currentExample)
    {
        var trimmedLine = line.Trim();
        
        if (trimmedLine.StartsWith("### Example"))
        {
            // Save previous example if exists
            if (currentExample.ContainsKey("title"))
            {
                AddExample(examples, currentExample);
                currentExample.Clear();
            }
            
            currentExample["title"] = trimmedLine.Substring(4).Trim();
        }
        else if (trimmedLine.StartsWith("**Description**:"))
        {
            currentExample["description"] = trimmedLine.Substring(16).Trim();
        }
        else if (trimmedLine.StartsWith("**Code**:"))
        {
            currentExample["collecting_code"] = true;
            currentExample["code"] = new List<string>();
        }
        else if (trimmedLine.StartsWith("**Output**:"))
        {
            currentExample["collecting_code"] = false;
            currentExample["collecting_output"] = true;
            currentExample["output"] = new List<string>();
        }
        else if (trimmedLine.StartsWith("**Explanation**:"))
        {
            currentExample["collecting_output"] = false;
            currentExample["explanation"] = trimmedLine.Substring(17).Trim();
        }
        else if (currentExample.ContainsKey("collecting_code") && (bool)currentExample["collecting_code"])
        {
            ((List<string>)currentExample["code"]).Add(line);
        }
        else if (currentExample.ContainsKey("collecting_output") && (bool)currentExample["collecting_output"])
        {
            ((List<string>)currentExample["output"]).Add(line);
        }
    }

    private void AddExample(List<LessonExample> examples, Dictionary<string, object> exampleData)
    {
        var codeLines = exampleData.GetValueOrDefault("code") as List<string> ?? new List<string>();
        var outputLines = exampleData.GetValueOrDefault("output") as List<string> ?? new List<string>();
        
        var example = new LessonExample(
            Title: exampleData.GetValueOrDefault("title")?.ToString() ?? string.Empty,
            Description: exampleData.GetValueOrDefault("description")?.ToString() ?? string.Empty,
            Code: string.Join("\n", codeLines),
            Output: string.Join("\n", outputLines),
            Explanation: exampleData.GetValueOrDefault("explanation")?.ToString() ?? string.Empty,
            Diagram: ImmutableList<string>.Empty
        );
        
        examples.Add(example);
    }

    private void ProcessMarkdownQuiz(string line, List<QuizQuestion> questions, Dictionary<string, object> currentQuestion, List<string> currentOptions)
    {
        var trimmedLine = line.Trim();
        
        if (trimmedLine.StartsWith("### Question"))
        {
            // Save previous question if exists
            if (currentQuestion.ContainsKey("question"))
            {
                AddQuizQuestion(questions, currentQuestion, currentOptions);
                currentQuestion.Clear();
                currentOptions.Clear();
            }
        }
        else if (!trimmedLine.StartsWith("**") && !trimmedLine.StartsWith("A)") && 
                 !trimmedLine.StartsWith("B)") && !trimmedLine.StartsWith("C)") && 
                 !trimmedLine.StartsWith("D)") && !string.IsNullOrEmpty(trimmedLine) &&
                 !currentQuestion.ContainsKey("question"))
        {
            currentQuestion["question"] = trimmedLine;
        }
        else if (trimmedLine.StartsWith("A)") || trimmedLine.StartsWith("B)") || 
                 trimmedLine.StartsWith("C)") || trimmedLine.StartsWith("D)"))
        {
            currentOptions.Add(trimmedLine.Substring(2).Trim());
        }
        else if (trimmedLine.StartsWith("**Correct Answer**:"))
        {
            var answerLetter = trimmedLine.Substring(19).Trim();
            currentQuestion["correctAnswer"] = answerLetter switch
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
            currentQuestion["explanation"] = trimmedLine.Substring(16).Trim();
        }
    }

    private void AddQuizQuestion(List<QuizQuestion> questions, Dictionary<string, object> questionData, List<string> options)
    {
        var question = new QuizQuestion(
            Question: questionData.GetValueOrDefault("question")?.ToString() ?? string.Empty,
            Options: options.ToImmutableList(),
            CorrectAnswer: (int)(questionData.GetValueOrDefault("correctAnswer") ?? 0),
            Explanation: questionData.GetValueOrDefault("explanation")?.ToString() ?? string.Empty
        );
        
        questions.Add(question);
    }

    private ImmutableList<LessonExample> ParseJsonExamples(object? examplesData)
    {
        // Implementation for parsing JSON examples would go here
        // For now, return empty list as graceful fallback
        return ImmutableList<LessonExample>.Empty;
    }

    private LessonQuiz ParseJsonQuiz(object? quizData)
    {
        // Implementation for parsing JSON quiz would go here
        // For now, return empty quiz as graceful fallback
        return LessonQuiz.Empty;
    }

    private ImmutableList<LessonExample> ParseYamlExamples(object? examplesData)
    {
        // Implementation for parsing YAML examples would go here
        // For now, return empty list as graceful fallback
        return ImmutableList<LessonExample>.Empty;
    }

    private LessonQuiz ParseYamlQuiz(object? quizData)
    {
        // Implementation for parsing YAML quiz would go here
        // For now, return empty quiz as graceful fallback
        return LessonQuiz.Empty;
    }

    private LessonContent CreateFallbackContent(string fileName)
    {
        _logger?.LogWarning("Creating fallback content for {FileName}", fileName);
        
        return new LessonContent(
            Theory: $"Content could not be loaded for {fileName}. Please check the lesson file format.",
            Examples: ImmutableList<LessonExample>.Empty,
            Quiz: LessonQuiz.Empty
        );
    }

    private ImmutableList<LessonModule> GetFallbackModules()
    {
        _logger?.LogInformation("Using fallback lesson modules");
        
        return ImmutableList.Create(
            new LessonModule(
                Name: "basics",
                Title: "Git Fundamentals",
                Description: "Learn the core concepts and basic commands of Git version control",
                Level: "Beginner",
                Duration: "2-3 hours",
                FileName: "basic-git.md",
                Order: 1,
                Chapters: ImmutableList<LessonChapter>.Empty
            ),
            new LessonModule(
                Name: "branching",
                Title: "Branch Management",
                Description: "Master Git branching for parallel development and feature isolation",
                Level: "Intermediate",
                Duration: "1-2 hours",
                FileName: "branching.json",
                Order: 2,
                Chapters: ImmutableList<LessonChapter>.Empty
            ),
            new LessonModule(
                Name: "collaboration",
                Title: "Team Collaboration",
                Description: "Work effectively with teams using remotes, pushing, and pull requests",
                Level: "Intermediate",
                Duration: "2-3 hours",
                FileName: "collaboration.yml",
                Order: 3,
                Chapters: ImmutableList<LessonChapter>.Empty
            )
        );
    }
}
