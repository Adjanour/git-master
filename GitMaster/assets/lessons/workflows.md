# Git Workflows - Organized Team Development

## Theory

Git workflows are structured approaches to using Git in team environments. They define how branches are created, how features are developed, how code is reviewed, and how releases are managed. Different workflows suit different team sizes, project types, and organizational structures.

### Popular Git Workflows

**Centralized Workflow**: Simple workflow where everyone works on the main branch. Good for small teams transitioning from centralized VCS.

**Feature Branch Workflow**: Each feature is developed in a dedicated branch. Features are merged back to main through pull requests.

**GitFlow**: A comprehensive workflow with dedicated branches for features, releases, and hotfixes. Provides structure for large projects with scheduled releases.

**GitHub Flow**: Simplified workflow focused on continuous deployment. Features are developed in branches and deployed directly from pull requests.

**GitLab Flow**: Combines feature branches with environment-specific branches for staging and production.

### Key Principles

- Keep the main branch deployable
- Use descriptive branch names
- Make small, focused commits
- Review code before merging
- Automate testing and deployment
- Document your workflow

### Branch Naming Conventions

```
feature/add-user-authentication
bugfix/fix-login-redirect
hotfix/security-vulnerability
release/v2.1.0
```

## Examples

### Example 1: Feature Branch Workflow

**Description**: The most common workflow for small to medium teams using GitHub or GitLab.

**Code**:
```bash
# Start with updated main branch
git checkout main
git pull origin main

# Create feature branch
git checkout -b feature/user-dashboard

# Work on feature with multiple commits
echo "Dashboard component" > dashboard.js
git add dashboard.js
git commit -m "Add dashboard component"

echo "Dashboard styles" > dashboard.css
git add dashboard.css
git commit -m "Add dashboard styling"

# Push feature branch
git push -u origin feature/user-dashboard

# Create pull request (done via GitHub/GitLab UI)
# After review and approval, merge via UI

# Cleanup locally
git checkout main
git pull origin main
git branch -d feature/user-dashboard
```

**Output**:
```
Already on 'main'
Already up to date.

Switched to a new branch 'feature/user-dashboard'

[feature/user-dashboard abc123] Add dashboard component
 1 file changed, 1 insertion(+)
 create mode 100644 dashboard.js

[feature/user-dashboard def456] Add dashboard styling
 1 file changed, 1 insertion(+)
 create mode 100644 dashboard.css

Branch 'feature/user-dashboard' set up to track remote branch.

# PR created and merged via UI

Updating abc123..def456
Fast-forward
 dashboard.js | 1 +
 dashboard.css | 1 +
 2 files changed, 2 insertions(+)

Deleted branch feature/user-dashboard (was def456).
```

**Explanation**: This workflow isolates feature development while maintaining a clean main branch. Pull requests enable code review and discussion before integration.

**Diagram**:
```
Feature Branch Workflow:

main:     A---B---C-------F---G
               \         /
feature:        D---E---/

Where:
A,B,C = existing commits
D,E = feature commits  
F = merge commit
G = continued development
```

### Example 2: GitFlow Workflow

**Description**: Structured workflow for projects with scheduled releases and multiple environments.

**Code**:
```bash
# Initialize GitFlow
git flow init

# Start a new feature
git flow feature start user-profiles

# Work on feature
echo "User profile logic" > profiles.js
git add profiles.js
git commit -m "Implement user profiles"

# Finish feature (merges to develop)
git flow feature finish user-profiles

# Start a release
git flow release start v1.2.0

# Prepare release (version bumps, changelog, etc.)
echo "Version 1.2.0" > VERSION
git add VERSION
git commit -m "Bump version to 1.2.0"

# Finish release (merges to main and develop, tags main)
git flow release finish v1.2.0

# Handle a hotfix
git flow hotfix start v1.2.1

echo "Security fix" > security-patch.js
git add security-patch.js
git commit -m "Fix security vulnerability"

git flow hotfix finish v1.2.1
```

**Output**:
```
Initialized empty Git repository
Branch 'develop' set up to track remote branch 'develop' from 'origin'.

Switched to a new branch 'feature/user-profiles'

[feature/user-profiles abc123] Implement user profiles
 1 file changed, 1 insertion(+)

Switched to branch 'develop'
Merging feature/user-profiles into develop
Deleted branch feature/user-profiles

Switched to a new branch 'release/v1.2.0'

[release/v1.2.0 def456] Bump version to 1.2.0
 1 file changed, 1 insertion(+)

Merging release/v1.2.0 into main
Tagging v1.2.0
Merging release/v1.2.0 into develop
```

**Explanation**: GitFlow provides clear separation between feature development, release preparation, and hotfixes. It's ideal for teams with planned releases.

**Diagram**:
```
GitFlow Branches:

main:      A---C-------F---H
            \   \     /   /
develop:     B---D---E---G---I
                  \     /
feature:           J---K
```

### Example 3: GitHub Flow with Continuous Deployment

**Description**: Simplified workflow for teams practicing continuous deployment.

**Code**:
```bash
# Always start from main
git checkout main
git pull origin main

# Create feature branch
git checkout -b improve-performance

# Make small, focused changes
echo "Performance optimization" > optimize.js
git add optimize.js
git commit -m "Optimize database queries"

# Push and create PR immediately
git push -u origin improve-performance

# Open PR via GitHub CLI or web interface
gh pr create --title "Improve database performance" \
  --body "Optimizes slow queries in user dashboard"

# After tests pass and review, deploy PR to staging
# (Automated via GitHub Actions)

# After staging validation, merge to main
gh pr merge --squash

# Automatic deployment to production
# (Triggered by merge to main)

# Cleanup
git checkout main
git pull origin main
git branch -d improve-performance
```

**Output**:
```
Already on 'main'
Already up to date.

Switched to a new branch 'improve-performance'

[improve-performance abc123] Optimize database queries
 1 file changed, 1 insertion(+)

Branch 'improve-performance' set up to track remote branch.

Creating pull request for improve-performance into main in user/repo

#42 Improve database performance

✓ Checks passed
✓ Review approved
✓ Deployed to staging

Merged pull request #42
✓ Deployed to production

Switched to branch 'main'
Already up to date.
Deleted branch improve-performance (was abc123).
```

**Explanation**: GitHub Flow emphasizes small changes, automated testing, and rapid deployment. Each feature can be deployed independently.

**Diagram**:
```
GitHub Flow with CD:

main:     A---B---D---F
           \     /   /
feature1:   C---/   /
feature2:       E---/

Each merge triggers:
main → staging → production
```

## Quiz

### Question 1
Which workflow is best for teams practicing continuous deployment?

A) GitFlow
B) Centralized Workflow
C) GitHub Flow
D) Feature Branch Workflow

**Correct Answer**: C
**Explanation**: GitHub Flow is designed for continuous deployment with its simplified branching model and emphasis on deploying features directly from pull requests.

### Question 2
In GitFlow, which branch is used for ongoing development?

A) main/master
B) develop
C) feature
D) release

**Correct Answer**: B
**Explanation**: In GitFlow, the develop branch serves as the integration branch for ongoing development, while main/master contains only production-ready code.

### Question 3
What is the main advantage of Feature Branch Workflow over Centralized Workflow?

A) Faster development
B) Fewer branches to manage
C) Code review and isolated feature development
D) Better for solo developers

**Correct Answer**: C
**Explanation**: Feature Branch Workflow enables code review through pull requests and isolates feature development, reducing conflicts and improving code quality.

### Question 4
When should you use a hotfix branch?

A) For all bug fixes
B) For critical issues in production that can't wait for the next release
C) When developing new features
D) For code refactoring

**Correct Answer**: B
**Explanation**: Hotfix branches are specifically for critical issues in production that need immediate attention and can't wait for the normal release cycle.
