using GitMaster.Models;
using System.Text.Json;

namespace GitMaster.Services;

public class ProgressService
{
    private readonly string _progressPath;
    private readonly string _legacyProgressPath;
    private ProgressData _progressData;

    public ProgressService()
    {
        // Use %LOCALAPPDATA% as specified in requirements
        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var gitMasterPath = Path.Combine(localAppDataPath, "GitMaster");
        
        if (!Directory.Exists(gitMasterPath))
        {
            Directory.CreateDirectory(gitMasterPath);
        }
        
        _progressPath = Path.Combine(gitMasterPath, "progress.json");
        
        // Keep legacy path for migration
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var legacyGitMasterPath = Path.Combine(appDataPath, "GitMaster");
        _legacyProgressPath = Path.Combine(legacyGitMasterPath, "learning-progress.json");
        
        _progressData = LoadProgress();
    }

    private ProgressData LoadProgress()
    {
        try
        {
            if (!File.Exists(_progressPath))
            {
                // Try to migrate from legacy location
                var migrated = MigrateLegacyProgress();
                if (migrated != null)
                {
                    return migrated;
                }
                
                return new ProgressData();
            }

            var jsonContent = File.ReadAllText(_progressPath);
            var progress = JsonSerializer.Deserialize<ProgressData>(jsonContent);
            return progress ?? new ProgressData();
        }
        catch
        {
            return new ProgressData();
        }
    }

    private ProgressData? MigrateLegacyProgress()
    {
        try
        {
            if (!File.Exists(_legacyProgressPath))
            {
                return null;
            }

            var jsonContent = File.ReadAllText(_legacyProgressPath);
            var legacyProgress = JsonSerializer.Deserialize<Dictionary<string, LearningProgress>>(jsonContent);
            
            if (legacyProgress == null)
            {
                return null;
            }

            var newProgress = new ProgressData();
            
            // Migrate learning progress to new format
            foreach (var kvp in legacyProgress)
            {
                var moduleProgress = new ModuleProgress
                {
                    ModuleName = kvp.Value.ModuleName,
                    CurrentLesson = kvp.Value.CurrentLesson,
                    FirstAccessed = kvp.Value.LastAccessed,
                    LastAccessed = kvp.Value.LastAccessed,
                    IsCompleted = kvp.Value.IsCompleted
                };

                // Create lesson attempts for completed lessons
                foreach (var lessonIndex in kvp.Value.CompletedLessons)
                {
                    moduleProgress.LessonAttempts.Add(new LessonAttempt
                    {
                        LessonIndex = lessonIndex,
                        StartTime = kvp.Value.LastAccessed,
                        CompletedTime = kvp.Value.LastAccessed,
                        Completed = true,
                        QuizScore = 100 // Default score for migrated data
                    });
                }

                newProgress.Modules[kvp.Key] = moduleProgress;
            }

            // Initialize stats
            newProgress.Stats.FirstSession = DateTime.Now;
            newProgress.Stats.TotalSessions = 1;
            
            SaveProgress();
            return newProgress;
        }
        catch
        {
            return null;
        }
    }

    private void SaveProgress()
    {
        try
        {
            _progressData.LastUpdated = DateTime.Now;
            var jsonContent = JsonSerializer.Serialize(_progressData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_progressPath, jsonContent);
        }
        catch
        {
            // Silently fail - progress tracking is not critical
        }
    }

    // Legacy methods for backward compatibility
    public LearningProgress GetProgress(string moduleName)
    {
        if (_progressData.Modules.ContainsKey(moduleName))
        {
            var moduleProgress = _progressData.Modules[moduleName];
            return new LearningProgress
            {
                ModuleName = moduleProgress.ModuleName,
                CurrentLesson = moduleProgress.CurrentLesson,
                CompletedLessons = moduleProgress.LessonAttempts
                    .Where(a => a.Completed)
                    .Select(a => a.LessonIndex)
                    .ToList(),
                LastAccessed = moduleProgress.LastAccessed,
                IsCompleted = moduleProgress.IsCompleted
            };
        }

        return new LearningProgress
        {
            ModuleName = moduleName,
            CurrentLesson = 0,
            CompletedLessons = new List<int>(),
            LastAccessed = DateTime.Now,
            IsCompleted = false
        };
    }

    public void UpdateProgress(string moduleName, int lessonIndex, bool completed = true)
    {
        RecordActivity();
        
        if (!_progressData.Modules.ContainsKey(moduleName))
        {
            _progressData.Modules[moduleName] = new ModuleProgress
            {
                ModuleName = moduleName,
                FirstAccessed = DateTime.Now
            };
        }

        var moduleProgress = _progressData.Modules[moduleName];
        moduleProgress.LastAccessed = DateTime.Now;
        moduleProgress.CurrentLesson = lessonIndex + 1;

        if (completed)
        {
            var existingAttempt = moduleProgress.LessonAttempts
                .FirstOrDefault(a => a.LessonIndex == lessonIndex);

            if (existingAttempt == null)
            {
                moduleProgress.LessonAttempts.Add(new LessonAttempt
                {
                    LessonIndex = lessonIndex,
                    StartTime = DateTime.Now.AddMinutes(-5), // Estimate start time
                    CompletedTime = DateTime.Now,
                    Completed = true,
                    QuizScore = 100, // Default score
                    TimeSpent = TimeSpan.FromMinutes(5) // Default time
                });
            }
            else if (!existingAttempt.Completed)
            {
                existingAttempt.Completed = true;
                existingAttempt.CompletedTime = DateTime.Now;
            }
        }

        SaveProgress();
    }

    public void MarkModuleCompleted(string moduleName)
    {
        RecordActivity();
        
        if (_progressData.Modules.ContainsKey(moduleName))
        {
            _progressData.Modules[moduleName].IsCompleted = true;
            _progressData.Modules[moduleName].LastAccessed = DateTime.Now;
            _progressData.Stats.ModulesCompleted++;
        }

        SaveProgress();
    }

    // Enhanced progress tracking methods
    public void StartPracticeSession(string scenarioName)
    {
        RecordActivity();
        
        if (!_progressData.Practice.ContainsKey(scenarioName))
        {
            _progressData.Practice[scenarioName] = new PracticeProgress
            {
                ScenarioName = scenarioName,
                FirstAttempt = DateTime.Now
            };
        }

        var practiceProgress = _progressData.Practice[scenarioName];
        practiceProgress.LastAttempt = DateTime.Now;
        practiceProgress.Attempts.Add(new PracticeAttempt
        {
            StartTime = DateTime.Now
        });

        SaveProgress();
    }

    public void CompletePracticeSession(string scenarioName, int objectivesCompleted, int totalObjectives, bool completed, List<string> hintsUsed)
    {
        if (!_progressData.Practice.ContainsKey(scenarioName))
        {
            return;
        }

        var practiceProgress = _progressData.Practice[scenarioName];
        var currentAttempt = practiceProgress.Attempts.LastOrDefault();
        
        if (currentAttempt != null)
        {
            currentAttempt.CompletedTime = DateTime.Now;
            currentAttempt.ObjectivesCompleted = objectivesCompleted;
            currentAttempt.TotalObjectives = totalObjectives;
            currentAttempt.Completed = completed;
            currentAttempt.HintsUsed = hintsUsed;
            currentAttempt.Duration = currentAttempt.CompletedTime.Value - currentAttempt.StartTime;
            
            // Calculate score (0-100 based on completion and hints used)
            var score = 0;
            if (totalObjectives > 0)
            {
                score = (int)(((double)objectivesCompleted / totalObjectives) * 100);
                if (completed)
                {
                    score = Math.Max(score, 70); // Minimum 70 for completion
                    score -= Math.Min(hintsUsed.Count * 5, 30); // Deduct for hints
                    score = Math.Max(score, 50); // Minimum 50 for completion
                }
            }
            
            currentAttempt.Score = score;
            
            if (completed)
            {
                practiceProgress.Completed = true;
                _progressData.Stats.PracticeSessionsCompleted++;
                
                if (score > practiceProgress.BestScore)
                {
                    practiceProgress.BestScore = score;
                }
                
                if (practiceProgress.BestTime == TimeSpan.Zero || currentAttempt.Duration < practiceProgress.BestTime)
                {
                    practiceProgress.BestTime = currentAttempt.Duration;
                }
            }
        }

        SaveProgress();
    }

    public void RecordCommandUsage(string commandName, string topic, bool successful, TimeSpan executionTime)
    {
        if (!_progressData.Commands.ContainsKey(commandName))
        {
            _progressData.Commands[commandName] = new CommandStats
            {
                CommandName = commandName,
                FirstUsed = DateTime.Now
            };
        }

        var commandStats = _progressData.Commands[commandName];
        commandStats.LastUsed = DateTime.Now;
        commandStats.UsageCount++;
        
        if (successful)
        {
            commandStats.SuccessfulUses++;
        }
        else
        {
            commandStats.FailedUses++;
        }

        if (!commandStats.Topics.Contains(topic))
        {
            commandStats.Topics.Add(topic);
        }

        // Update average execution time
        var totalTime = commandStats.AverageExecutionTime.TotalMilliseconds * (commandStats.UsageCount - 1);
        totalTime += executionTime.TotalMilliseconds;
        commandStats.AverageExecutionTime = TimeSpan.FromMilliseconds(totalTime / commandStats.UsageCount);

        SaveProgress();
    }

    private void RecordActivity()
    {
        var today = DateTime.Today;
        
        if (_progressData.Stats.FirstSession == DateTime.MinValue)
        {
            _progressData.Stats.FirstSession = DateTime.Now;
        }

        // Update streaks
        if (_progressData.Streaks.LastActivityDate.Date != today)
        {
            if (_progressData.Streaks.LastActivityDate.Date == today.AddDays(-1))
            {
                // Consecutive day
                _progressData.Streaks.CurrentStreak++;
            }
            else if (_progressData.Streaks.LastActivityDate.Date < today.AddDays(-1))
            {
                // Streak broken
                _progressData.Streaks.CurrentStreak = 1;
                _progressData.Streaks.StreakStartDate = today;
            }
            else
            {
                // First day
                _progressData.Streaks.CurrentStreak = 1;
                _progressData.Streaks.StreakStartDate = today;
            }

            _progressData.Streaks.LastActivityDate = today;
            
            if (_progressData.Streaks.CurrentStreak > _progressData.Streaks.LongestStreak)
            {
                _progressData.Streaks.LongestStreak = _progressData.Streaks.CurrentStreak;
            }

            if (!_progressData.Streaks.ActivityDates.Contains(today))
            {
                _progressData.Streaks.ActivityDates.Add(today);
            }

            _progressData.Stats.TotalSessions++;
        }
    }

    // Data access methods for progress command
    public ProgressData GetProgressData()
    {
        return _progressData;
    }

    public void ResetProgress(string moduleName)
    {
        if (_progressData.Modules.ContainsKey(moduleName))
        {
            _progressData.Modules.Remove(moduleName);
            SaveProgress();
        }
    }

    public void ResetAllProgress()
    {
        _progressData = new ProgressData();
        SaveProgress();
    }

    public List<LearningProgress> GetAllProgress()
    {
        return _progressData.Modules.Values.Select(m => new LearningProgress
        {
            ModuleName = m.ModuleName,
            CurrentLesson = m.CurrentLesson,
            CompletedLessons = m.LessonAttempts.Where(a => a.Completed).Select(a => a.LessonIndex).ToList(),
            LastAccessed = m.LastAccessed,
            IsCompleted = m.IsCompleted
        }).ToList();
    }

    public string GetNextModule(List<LearningModule> availableModules)
    {
        var sortedModules = availableModules.OrderBy(m => m.Order).ToList();
        
        foreach (var module in sortedModules)
        {
            var moduleName = GetModuleNameFromTitle(module.Title);
            if (!_progressData.Modules.ContainsKey(moduleName) || !_progressData.Modules[moduleName].IsCompleted)
            {
                return moduleName;
            }
        }

        return GetModuleNameFromTitle(sortedModules.FirstOrDefault()?.Title ?? "Git Fundamentals");
    }

    public int GetModuleProgressPercentage(string moduleName, int totalLessons)
    {
        if (!_progressData.Modules.ContainsKey(moduleName) || totalLessons == 0)
        {
            return 0;
        }
        
        var moduleProgress = _progressData.Modules[moduleName];
        var completedLessons = moduleProgress.LessonAttempts.Count(a => a.Completed);
        return (int)Math.Round((double)completedLessons / totalLessons * 100);
    }

    public int GetOverallProgressPercentage(List<LearningModule> availableModules)
    {
        if (!availableModules.Any()) return 0;

        var completedModules = availableModules.Count(module =>
        {
            var moduleName = GetModuleNameFromTitle(module.Title);
            return _progressData.Modules.ContainsKey(moduleName) && _progressData.Modules[moduleName].IsCompleted;
        });

        return (int)Math.Round((double)completedModules / availableModules.Count * 100);
    }
    
    private string GetModuleNameFromTitle(string title)
    {
        return title.ToLowerInvariant() switch
        {
            "git fundamentals" => "basics",
            "branch management" => "branching", 
            "team collaboration" => "collaboration",
            "git workflows" => "workflows",
            "advanced techniques" => "advanced",
            "common problems" => "troubleshooting",
            _ => title.ToLowerInvariant().Replace(" ", "-")
        };
    }
}
