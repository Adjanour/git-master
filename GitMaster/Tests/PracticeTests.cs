using GitMaster.Models;
using GitMaster.Services;
using Xunit;

namespace GitMaster.Tests;

public class PracticeServiceTests
{
    [Fact]
    public async Task LoadScenarioAsync_WithValidScenario_ReturnsScenario()
    {
        // Arrange
        var gitService = new GitRepositoryService();
        var practiceService = new PracticeService(gitService);

        // Act
        var scenario = await practiceService.LoadScenarioAsync("merge_conflict");

        // Assert
        Assert.NotNull(scenario);
        Assert.Equal("Merge Conflict Resolution", scenario.Name);
        Assert.Equal("intermediate", scenario.Difficulty);
        Assert.True(scenario.Setup.Count > 0);
        Assert.True(scenario.Objectives.Count > 0);
    }

    [Fact]
    public async Task LoadScenarioAsync_WithInvalidScenario_ReturnsNull()
    {
        // Arrange
        var gitService = new GitRepositoryService();
        var practiceService = new PracticeService(gitService);

        // Act
        var scenario = await practiceService.LoadScenarioAsync("nonexistent_scenario");

        // Assert
        Assert.Null(scenario);
    }

    [Fact]
    public async Task GetAvailableScenariosAsync_ReturnsScenarios()
    {
        // Arrange
        var gitService = new GitRepositoryService();
        var practiceService = new PracticeService(gitService);

        // Act
        var scenarios = await practiceService.GetAvailableScenariosAsync();

        // Assert
        Assert.NotEmpty(scenarios);
        Assert.Contains("merge_conflict", scenarios);
        Assert.Contains("basic_branching", scenarios);
    }

    [Fact]
    public async Task StartScenarioAsync_CreatesValidSession()
    {
        // Arrange
        var gitService = new GitRepositoryService();
        var practiceService = new PracticeService(gitService);

        // Act
        var session = await practiceService.StartScenarioAsync("merge_conflict");

        // Assert
        Assert.NotNull(session);
        Assert.Equal("merge_conflict", session.ScenarioName);
        Assert.NotEmpty(session.SandboxPath);
        Assert.False(session.IsCompleted);
        Assert.Equal(0, session.CurrentObjectiveIndex);
        Assert.Empty(session.CompletedObjectives);
    }
}

public class GitRepositoryServiceTests
{
    [Fact]
    public void IsRepository_WithNonExistentPath_ReturnsFalse()
    {
        // Arrange
        var gitService = new GitRepositoryService();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = gitService.IsRepository(nonExistentPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InitializeRepository_CreatesValidRepo()
    {
        // Arrange
        var gitService = new GitRepositoryService();
        var tempPath = Path.Combine(Path.GetTempPath(), "GitMasterTest_" + Guid.NewGuid().ToString());

        try
        {
            // Act
            gitService.InitializeRepository(tempPath);

            // Assert
            Assert.True(Directory.Exists(tempPath));
            Assert.True(gitService.IsRepository(tempPath));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }
}

public class PracticeModelsTests
{
    [Fact]
    public void PracticeSession_InitializesWithDefaults()
    {
        // Act
        var session = new PracticeSession();

        // Assert
        Assert.Empty(session.ScenarioName);
        Assert.Empty(session.SandboxPath);
        Assert.NotNull(session.Scenario);
        Assert.Equal(0, session.CurrentObjectiveIndex);
        Assert.Empty(session.CompletedObjectives);
        Assert.False(session.IsCompleted);
    }

    [Fact]
    public void ObjectiveResult_CanBeCreated()
    {
        // Act
        var result = new ObjectiveResult
        {
            Status = ObjectiveStatus.Completed,
            Message = "Test message",
            Hint = "Test hint",
            ShouldAdvance = true
        };

        // Assert
        Assert.Equal(ObjectiveStatus.Completed, result.Status);
        Assert.Equal("Test message", result.Message);
        Assert.Equal("Test hint", result.Hint);
        Assert.True(result.ShouldAdvance);
    }
}
