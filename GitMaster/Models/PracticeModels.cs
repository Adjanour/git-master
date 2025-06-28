using YamlDotNet.Serialization;

namespace GitMaster.Models;

public class PracticeScenario
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;
    
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
    
    [YamlMember(Alias = "difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    
    [YamlMember(Alias = "category")]
    public string Category { get; set; } = string.Empty;
    
    [YamlMember(Alias = "estimated_time")]
    public string EstimatedTime { get; set; } = string.Empty;
    
    [YamlMember(Alias = "setup")]
    public List<SetupStep> Setup { get; set; } = new();
    
    [YamlMember(Alias = "objectives")]
    public List<PracticeObjective> Objectives { get; set; } = new();
    
    [YamlMember(Alias = "success_criteria")]
    public List<SuccessCriterion> SuccessCriteria { get; set; } = new();
    
    [YamlMember(Alias = "evaluation")]
    public List<EvaluationHint> Evaluation { get; set; } = new();
}

public class SetupStep
{
    [YamlMember(Alias = "command")]
    public string Command { get; set; } = string.Empty;
    
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
}

public class PracticeObjective
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;
    
    [YamlMember(Alias = "goal")]
    public string Goal { get; set; } = string.Empty;
    
    [YamlMember(Alias = "command")]
    public string? Command { get; set; }
    
    [YamlMember(Alias = "expected_result")]
    public string? ExpectedResult { get; set; }
    
    [YamlMember(Alias = "validation_type")]
    public string? ValidationType { get; set; }
    
    [YamlMember(Alias = "target_file")]
    public string? TargetFile { get; set; }
    
    [YamlMember(Alias = "expected_content_contains")]
    public List<string>? ExpectedContentContains { get; set; }
    
    [YamlMember(Alias = "hint")]
    public string Hint { get; set; } = string.Empty;
}

public class SuccessCriterion
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;
    
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
    
    [YamlMember(Alias = "file")]
    public string? File { get; set; }
    
    [YamlMember(Alias = "content")]
    public List<string>? Content { get; set; }
}

public class EvaluationHint
{
    [YamlMember(Alias = "condition")]
    public string Condition { get; set; } = string.Empty;
    
    [YamlMember(Alias = "message")]
    public string Message { get; set; } = string.Empty;
}

public class PracticeSession
{
    public string ScenarioName { get; set; } = string.Empty;
    public string SandboxPath { get; set; } = string.Empty;
    public PracticeScenario Scenario { get; set; } = new();
    public int CurrentObjectiveIndex { get; set; } = 0;
    public List<string> CompletedObjectives { get; set; } = new();
    public DateTime StartTime { get; set; } = DateTime.Now;
    public bool IsCompleted { get; set; } = false;
}

public enum ObjectiveStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public class ObjectiveResult
{
    public ObjectiveStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public bool ShouldAdvance { get; set; }
}
