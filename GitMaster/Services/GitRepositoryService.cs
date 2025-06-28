using LibGit2Sharp;
using System.Diagnostics;

namespace GitMaster.Services;

public interface IGitRepositoryService
{
    bool IsRepository(string path);
    void InitializeRepository(string path);
    Repository OpenRepository(string path);
    bool HasMergeConflicts(string repoPath);
    bool IsMergeInProgress(string repoPath);
    string? GetCurrentBranch(string repoPath);
    List<string> GetBranches(string repoPath);
    List<string> GetConflictedFiles(string repoPath);
    bool IsBranchMerged(string repoPath, string branchName, string targetBranch = "main");
    string ExecuteGitCommand(string repoPath, string command);
}

public class GitRepositoryService : IGitRepositoryService
{
    public bool IsRepository(string path)
    {
        try
        {
            return Repository.IsValid(path);
        }
        catch
        {
            return false;
        }
    }

    public void InitializeRepository(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        Repository.Init(path);
    }

    public Repository OpenRepository(string path)
    {
        return new Repository(path);
    }

    public bool HasMergeConflicts(string repoPath)
    {
        try
        {
            using var repo = new Repository(repoPath);
            return repo.Index.Conflicts.Any();
        }
        catch
        {
            return false;
        }
    }

    public bool IsMergeInProgress(string repoPath)
    {
        try
        {
            using var repo = new Repository(repoPath);
            return repo.Info.CurrentOperation == CurrentOperation.Merge;
        }
        catch
        {
            return false;
        }
    }

    public string? GetCurrentBranch(string repoPath)
    {
        try
        {
            using var repo = new Repository(repoPath);
            return repo.Head.FriendlyName;
        }
        catch
        {
            return null;
        }
    }

    public List<string> GetBranches(string repoPath)
    {
        try
        {
            using var repo = new Repository(repoPath);
            return repo.Branches.Select(b => b.FriendlyName).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public List<string> GetConflictedFiles(string repoPath)
    {
        try
        {
            using var repo = new Repository(repoPath);
            return repo.Index.Conflicts.Select(c => c.Ancestor.Path).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public bool IsBranchMerged(string repoPath, string branchName, string targetBranch = "main")
    {
        try
        {
            using var repo = new Repository(repoPath);
            var branch = repo.Branches[branchName];
            var target = repo.Branches[targetBranch];
            
            if (branch == null || target == null)
                return false;

            // Check if branch commit is reachable from target branch
            var filter = new CommitFilter
            {
                IncludeReachableFrom = target,
                ExcludeReachableFrom = branch
            };

            return !repo.Commits.QueryBy(filter).Any();
        }
        catch
        {
            return false;
        }
    }

    public string ExecuteGitCommand(string repoPath, string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo("git")
            {
                Arguments = command,
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
                return string.Empty;

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            return string.IsNullOrEmpty(error) ? output : $"{output}\n{error}";
        }
        catch (Exception ex)
        {
            return $"Error executing git command: {ex.Message}";
        }
    }
}
