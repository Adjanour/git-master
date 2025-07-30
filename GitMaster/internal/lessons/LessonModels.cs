using System.Collections.Immutable;

namespace GitMaster.Internal.Lessons;

/// <summary>
/// Represents a complete learning module containing multiple chapters
/// </summary>
public record LessonModule(
    string Name,
    string Title,
    string Description,
    string Level,
    string Duration,
    string FileName,
    int Order,
    ImmutableList<LessonChapter> Chapters
)
{
    public static LessonModule Empty => new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        ImmutableList<LessonChapter>.Empty
    );
}

/// <summary>
/// Represents a single chapter within a lesson module
/// </summary>
public record LessonChapter(
    int ChapterNumber,
    string Title,
    LessonContent Content,
    bool IsCompleted = false
)
{
    public static LessonChapter Empty => new(
        0,
        string.Empty,
        LessonContent.Empty
    );
}

/// <summary>
/// Represents the content of a lesson chapter including theory, examples, and quiz
/// </summary>
public record LessonContent(
    string Theory,
    ImmutableList<LessonExample> Examples,
    LessonQuiz Quiz
)
{
    public static LessonContent Empty => new(
        string.Empty,
        ImmutableList<LessonExample>.Empty,
        LessonQuiz.Empty
    );
}

/// <summary>
/// Represents a practical example within a lesson
/// </summary>
public record LessonExample(
    string Title,
    string Description,
    string Code,
    string Output,
    string Explanation,
    ImmutableList<string> Diagram
)
{
    public static LessonExample Empty => new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        ImmutableList<string>.Empty
    );
}

/// <summary>
/// Represents a quiz section with multiple questions
/// </summary>
public record LessonQuiz(
    ImmutableList<QuizQuestion> Questions
)
{
    public static LessonQuiz Empty => new(
        ImmutableList<QuizQuestion>.Empty
    );
}

/// <summary>
/// Represents a single quiz question with multiple choice answers
/// </summary>
public record QuizQuestion(
    string Question,
    ImmutableList<string> Options,
    int CorrectAnswer,
    string Explanation
)
{
    public static QuizQuestion Empty => new(
        string.Empty,
        ImmutableList<string>.Empty,
        0,
        string.Empty
    );
}

/// <summary>
/// Represents the result of completing a lesson chapter
/// </summary>
public record ChapterCompletionResult(
    string ModuleName,
    int ChapterNumber,
    DateTime CompletedAt,
    int QuizScore,
    TimeSpan TimeSpent
);
