using Xunit;
using GitMaster.Services;
using GitMaster.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GitMaster.Tests;

public class CheatSheetServiceTests
{
    [Fact]
    public void DeserializeBasicYaml_ParsesCorrectly()
    {
        // Arrange
        var yaml = @"
topics:
  test:
    title: Test Topic
    description: Test description
    commands:
      - name: test command
        syntax: test syntax
        description: test desc
        examples:
          - command: example cmd
            description: example desc
        tags: [tag1, tag2]
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        // Act
        var result = deserializer.Deserialize<CheatSheetData>(yaml);

        // Assert
        Assert.Single(result.Topics);
        var topic = result.Topics["test"];
        Assert.Equal("Test Topic", topic.Title);
        Assert.Equal("Test description", topic.Description);
        Assert.Single(topic.Commands);
        
        var command = topic.Commands[0];
        Assert.Equal("test command", command.Name);
        Assert.Equal("test syntax", command.Syntax);
        Assert.Single(command.Examples);
        Assert.Equal(2, command.Tags.Count);
    }

    [Fact]
    public void SearchCommands_EmptyTerm_ReturnsEmpty()
    {
        // Arrange
        var service = new CheatSheetService();

        // Act
        var results = service.SearchCommands("");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void SearchCommands_ValidTerm_ReturnsResults()
    {
        // Arrange
        var service = new CheatSheetService();

        // Act - Search for "rebase" which should be in our cheat sheet
        var results = service.SearchCommands("rebase");

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, cmd => cmd.Name.Contains("rebase"));
    }

    [Fact]
    public void GetTopicNames_ReturnsNonEmptyList()
    {
        // Arrange
        var service = new CheatSheetService();

        // Act
        var topicNames = service.GetTopicNames();

        // Assert
        Assert.NotEmpty(topicNames);
    }

    [Fact]
    public void GetTopic_ExistingTopic_ReturnsNotNull()
    {
        // Arrange
        var service = new CheatSheetService();

        // Act - Try to get "basics" topic which should exist
        var topic = service.GetTopic("basics");

        // Assert
        Assert.NotNull(topic);
        Assert.NotEmpty(topic.Title);
        Assert.NotEmpty(topic.Commands);
    }

    [Fact]
    public void GetTopic_NonExistentTopic_ReturnsNull()
    {
        // Arrange
        var service = new CheatSheetService();

        // Act
        var topic = service.GetTopic("nonexistent");

        // Assert
        Assert.Null(topic);
    }
}

public class CheatSheetRendererTests
{
    [Fact]
    public void CheatSheetRenderer_CanBeInstantiated()
    {
        // Arrange & Act
        var renderer = new CheatSheetRenderer();

        // Assert
        Assert.NotNull(renderer);
    }
}
