# Git Basics - Version Control Fundamentals

## Theory

Git is a distributed version control system that tracks changes in your code over time. Think of it as a sophisticated save system that remembers every version of your files and allows you to:

- Track changes over time
- Collaborate with others
- Revert to previous versions
- Create parallel development branches
- Merge different versions together

### Key Concepts

**Repository (Repo)**: A folder that contains your project files and Git's tracking information.

**Commit**: A snapshot of your project at a specific point in time. Each commit has a unique identifier (hash).

**Working Directory**: The folder on your computer where you edit files.

**Staging Area**: A temporary holding area where you prepare changes before committing them.

**Branch**: An independent line of development. The main branch is usually called `main` or `master`.

### The Git Workflow

```
Working Directory → Staging Area → Repository
     (edit)           (add)        (commit)
```

## Examples

### Example 1: Setting Up Your First Repository

**Description**: Learn how to initialize a new Git repository and configure your identity.

**Code**:
```bash
# Configure Git with your information (one-time setup)
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# Initialize a new Git repository
mkdir my-project
cd my-project
git init

# Check repository status
git status
```

**Output**:
```
Initialized empty Git repository in /path/to/my-project/.git/
On branch main

No commits yet

nothing to commit (create/copy files and use "git add" to track)
```

**Explanation**: The `git init` command creates a hidden `.git` folder that stores all version control information. Git is now tracking this directory.

**Diagram**:
```
┌─────────────────┐
│  Working Dir    │
│  ┌───────────┐  │    git init
│  │ my-project│  │ ──────────→
│  └───────────┘  │
└─────────────────┘

┌─────────────────┐
│  Working Dir    │
│  ┌───────────┐  │
│  │ my-project│  │
│  │  ├─.git/  │  │ ← Git tracking
│  └───────────┘  │
└─────────────────┘
```

### Example 2: Making Your First Commit

**Description**: Create a file, stage it, and make your first commit.

**Code**:
```bash
# Create a new file
echo "# My Project" > README.md

# Check what Git sees
git status

# Stage the file (prepare it for commit)
git add README.md

# Check status again
git status

# Commit the changes
git commit -m "Initial commit: Add README"
```

**Output**:
```
On branch main
Untracked files:
  README.md

On branch main
Changes to be committed:
  new file:   README.md

[main (root-commit) a1b2c3d] Initial commit: Add README
 1 file changed, 1 insertion(+)
 create mode 100644 README.md
```

**Explanation**: Files go through three stages: untracked → staged → committed. The staging area lets you choose exactly what changes to include in each commit.

**Diagram**:
```
Working Directory    Staging Area       Repository
┌───────────────┐   ┌─────────────┐   ┌─────────────┐
│ README.md     │   │             │   │             │
│ (modified)    │   │             │   │             │
└───────────────┘   └─────────────┘   └─────────────┘
        │                    
        │ git add README.md
        ▼                    
┌───────────────┐   ┌─────────────┐   ┌─────────────┐
│ README.md     │   │ README.md   │   │             │
│               │   │ (staged)    │   │             │
└───────────────┘   └─────────────┘   └─────────────┘
                             │
                             │ git commit
                             ▼
┌───────────────┐   ┌─────────────┐   ┌─────────────┐
│ README.md     │   │             │   │ README.md   │
│               │   │             │   │ (committed) │
└───────────────┘   └─────────────┘   └─────────────┘
```

### Example 3: Viewing History and Changes

**Description**: Learn how to view your commit history and see what changed between versions.

**Code**:
```bash
# View commit history
git log

# View compact history
git log --oneline

# See what changed in the last commit
git show

# Compare working directory with last commit
echo "This is my awesome project!" >> README.md
git diff
```

**Output**:
```
commit a1b2c3d4e5f6789... (HEAD -> main)
Author: Your Name <your.email@example.com>
Date:   Mon Jan 1 12:00:00 2024 +0000

    Initial commit: Add README

a1b2c3d Initial commit: Add README

diff --git a/README.md b/README.md
index 1234567..abcdefg 100644
--- a/README.md
+++ b/README.md
@@ -1 +1,2 @@
 # My Project
+This is my awesome project!
```

**Explanation**: Git maintains a complete history of all changes. You can always see what changed, when, and who made the changes.

## Quiz

### Question 1
What is the correct order of the Git workflow?

A) Working Directory → Repository → Staging Area
B) Staging Area → Working Directory → Repository  
C) Working Directory → Staging Area → Repository
D) Repository → Staging Area → Working Directory

**Correct Answer**: C
**Explanation**: You edit files in the Working Directory, stage them in the Staging Area with `git add`, then commit them to the Repository with `git commit`.

### Question 2
Which command is used to initialize a new Git repository?

A) `git start`
B) `git create`
C) `git init` 
D) `git new`

**Correct Answer**: C
**Explanation**: `git init` creates a new Git repository in the current directory by adding a `.git` folder that contains all the version control metadata.

### Question 3
What does the staging area allow you to do?

A) Edit files before committing
B) Choose which changes to include in a commit
C) Store files permanently
D) Share files with other developers

**Correct Answer**: B
**Explanation**: The staging area is like a preparation area where you can selectively choose which changes to include in your next commit, giving you fine-grained control over your version history.

### Question 4
Which command shows the differences between your working directory and the last commit?

A) `git status`
B) `git log`
C) `git diff`
D) `git show`

**Correct Answer**: C
**Explanation**: `git diff` shows the line-by-line differences between your current working directory and the last committed version of your files.
