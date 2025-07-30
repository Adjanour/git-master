using System.Collections.Immutable;

namespace GitMaster.Internal.Lessons;

/// <summary>
/// Service interface for managing lesson modules and tracking progress
/// </summary>
public interface ILessonService
{
    /// <summary>
    /// Gets all available lesson modules
    /// </summary>
    /// <returns>An immutable list of available lesson modules</returns>
    Task<ImmutableList<LessonModule>> GetAvailableModulesAsync();

    /// <summary>
    /// Loads a specific lesson module by name
    /// </summary>
    /// <param name="name">The name identifier of the module to load</param>
    /// <returns>The lesson module if found, null otherwise</returns>
    Task<LessonModule?> LoadModuleAsync(string name);

    /// <summary>
    /// Marks a specific chapter as completed for a module
    /// </summary>
    /// <param name="moduleName">The name of the module</param>
    /// <param name="chapterNumber">The chapter number to mark as completed</param>
    /// <returns>The completion result if successful</returns>
    Task<ChapterCompletionResult?> MarkChapterCompleteAsync(string moduleName, int chapterNumber);
}
