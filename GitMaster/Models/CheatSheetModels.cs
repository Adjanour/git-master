using YamlDotNet.Serialization;

namespace GitMaster.Models;

public class CheatSheetData
{
    [YamlMember(Alias = "topics")]
    public Dictionary<string, Topic> Topics { get; set; } = new();
}

public class Topic
{
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    [YamlMember(Alias = "commands")]
    public List<Command> Commands { get; set; } = new();
}

public class Command
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "syntax")]
    public string Syntax { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    [YamlMember(Alias = "examples")]
    public List<Example> Examples { get; set; } = new();

    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = new();
}

public class Example
{
    [YamlMember(Alias = "command")]
    public string CommandText { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
}

// Learning Module Models
public class LearningModule
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public List<Lesson> Lessons { get; set; } = new();
    public int Order { get; set; }
}

public class Lesson
{
    public string Title { get; set; } = string.Empty;
    public string Theory { get; set; } = string.Empty;
    public List<LessonExample> Examples { get; set; } = new();
    public Quiz Quiz { get; set; } = new();
    public int Order { get; set; }
}

public class LessonExample
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> Diagram { get; set; } = new();
}

public class Quiz
{
    public List<QuizQuestion> Questions { get; set; } = new();
}

public class QuizQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class LearningProgress
{
    public string ModuleName { get; set; } = string.Empty;
    public int CurrentLesson { get; set; }
    public List<int> CompletedLessons { get; set; } = new();
    public DateTime LastAccessed { get; set; }
    public bool IsCompleted { get; set; }
}

// Enhanced Progress Tracking Models
public class ProgressData
{
    public Dictionary<string, ModuleProgress> Modules { get; set; } = new();
    public Dictionary<string, PracticeProgress> Practice { get; set; } = new();
    public Dictionary<string, CommandStats> Commands { get; set; } = new();
    public StreakData Streaks { get; set; } = new();
    public UserStats Stats { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class ModuleProgress
{
    public string ModuleName { get; set; } = string.Empty;
    public int CurrentLesson { get; set; }
    public List<LessonAttempt> LessonAttempts { get; set; } = new();
    public DateTime FirstAccessed { get; set; }
    public DateTime LastAccessed { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public int Score { get; set; }
}

public class LessonAttempt
{
    public int LessonIndex { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public int QuizScore { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public bool Completed { get; set; }
}

public class PracticeProgress
{
    public string ScenarioName { get; set; } = string.Empty;
    public List<PracticeAttempt> Attempts { get; set; } = new();
    public DateTime FirstAttempt { get; set; }
    public DateTime LastAttempt { get; set; }
    public bool Completed { get; set; }
    public int BestScore { get; set; }
    public TimeSpan BestTime { get; set; }
}

public class PracticeAttempt
{
    public DateTime StartTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public int ObjectivesCompleted { get; set; }
    public int TotalObjectives { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Completed { get; set; }
    public int Score { get; set; }
    public List<string> HintsUsed { get; set; } = new();
}

public class CommandStats
{
    public string CommandName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public DateTime FirstUsed { get; set; }
    public DateTime LastUsed { get; set; }
    public int SuccessfulUses { get; set; }
    public int FailedUses { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public List<string> Topics { get; set; } = new();
}

public class StreakData
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime LastActivityDate { get; set; }
    public DateTime StreakStartDate { get; set; }
    public List<DateTime> ActivityDates { get; set; } = new();
}

public class UserStats
{
    public DateTime FirstSession { get; set; }
    public int TotalSessions { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public int ModulesCompleted { get; set; }
    public int PracticeSessionsCompleted { get; set; }
    public int TotalScore { get; set; }
    public int Level { get; set; } = 1;
    public int ExperiencePoints { get; set; }
    public List<Achievement> Achievements { get; set; } = new();
}

public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EarnedDate { get; set; }
    public string IconEmoji { get; set; } = string.Empty;
}
