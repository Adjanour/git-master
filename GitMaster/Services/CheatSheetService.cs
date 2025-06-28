using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using GitMaster.Models;
using FuzzySharp;
using System.Reflection;

namespace GitMaster.Services;

public class CheatSheetService
{
    private readonly CheatSheetData _cheatSheetData;
    private const string CheatSheetFileName = "cheatsheet.yml";

    public CheatSheetService()
    {
        _cheatSheetData = LoadCheatSheetData();
    }

    private CheatSheetData LoadCheatSheetData()
    {
        try
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
            var assetPath = Path.Combine(assemblyDirectory, "assets", CheatSheetFileName);

            // If not found in assembly directory, try current directory
            if (!File.Exists(assetPath))
            {
                assetPath = Path.Combine(Environment.CurrentDirectory, "assets", CheatSheetFileName);
            }

            if (!File.Exists(assetPath))
            {
                throw new FileNotFoundException($"Cheat sheet file not found at: {assetPath}");
            }

            var yamlContent = File.ReadAllText(assetPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<CheatSheetData>(yamlContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load cheat sheet data: {ex.Message}", ex);
        }
    }

    public Dictionary<string, Topic> GetAllTopics()
    {
        return _cheatSheetData.Topics;
    }

    public Topic? GetTopic(string topicName)
    {
        return _cheatSheetData.Topics.TryGetValue(topicName.ToLowerInvariant(), out var topic) ? topic : null;
    }

    public List<string> GetTopicNames()
    {
        return _cheatSheetData.Topics.Keys.ToList();
    }

    public List<Command> SearchCommands(string searchTerm, int threshold = 70)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Command>();

        var allCommands = _cheatSheetData.Topics
            .SelectMany(t => t.Value.Commands)
            .ToList();

        var results = new List<(Command command, int score)>();

        foreach (var command in allCommands)
        {
            // Search in command name
            var nameScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), command.Name.ToLowerInvariant());
            
            // Search in description
            var descScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), command.Description.ToLowerInvariant());
            
            // Search in tags
            var tagScore = command.Tags.Max(tag => 
                Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), tag.ToLowerInvariant()));
            
            // Search in syntax
            var syntaxScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), command.Syntax.ToLowerInvariant());

            var maxScore = Math.Max(Math.Max(nameScore, descScore), Math.Max(tagScore, syntaxScore));
            
            if (maxScore >= threshold)
            {
                results.Add((command, maxScore));
            }
        }

        return results
            .OrderByDescending(r => r.score)
            .Select(r => r.command)
            .ToList();
    }

    public List<Command> SearchCommandsInTopic(string topicName, string searchTerm, int threshold = 70)
    {
        var topic = GetTopic(topicName);
        if (topic == null)
            return new List<Command>();

        if (string.IsNullOrWhiteSpace(searchTerm))
            return topic.Commands;

        var results = new List<(Command command, int score)>();

        foreach (var command in topic.Commands)
        {
            // Search in command name
            var nameScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), command.Name.ToLowerInvariant());
            
            // Search in description
            var descScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), command.Description.ToLowerInvariant());
            
            // Search in tags
            var tagScore = command.Tags.Any() ? command.Tags.Max(tag => 
                Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), tag.ToLowerInvariant())) : 0;
            
            // Search in syntax
            var syntaxScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), command.Syntax.ToLowerInvariant());

            var maxScore = Math.Max(Math.Max(nameScore, descScore), Math.Max(tagScore, syntaxScore));
            
            if (maxScore >= threshold)
            {
                results.Add((command, maxScore));
            }
        }

        return results
            .OrderByDescending(r => r.score)
            .Select(r => r.command)
            .ToList();
    }

    public List<string> FindSimilarTopics(string searchTerm, int threshold = 60)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<string>();

        var results = new List<(string topic, int score)>();

        foreach (var (topicKey, topicValue) in _cheatSheetData.Topics)
        {
            var keyScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), topicKey.ToLowerInvariant());
            var titleScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), topicValue.Title.ToLowerInvariant());
            var descScore = Fuzz.PartialRatio(searchTerm.ToLowerInvariant(), topicValue.Description.ToLowerInvariant());

            var maxScore = Math.Max(Math.Max(keyScore, titleScore), descScore);

            if (maxScore >= threshold)
            {
                results.Add((topicKey, maxScore));
            }
        }

        return results
            .OrderByDescending(r => r.score)
            .Select(r => r.topic)
            .ToList();
    }
}
