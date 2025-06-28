# GitMaster Practice Mode

The Practice Mode engine provides hands-on learning experiences for Git concepts through guided scenarios in isolated sandbox environments.

## Features

- **Automated Sandbox Creation**: Creates temporary Git repositories with pre-configured states
- **Interactive Monitoring**: Real-time evaluation of user commands and repository state
- **Progressive Learning**: Step-by-step objectives with hints and feedback
- **Multiple Validation Types**: Command-based and file content validation
- **Scenario Debugging**: Custom sandbox paths for scenario development

## Usage

### Basic Usage

```bash
# List available scenarios
gitmaster practice

# Run a specific scenario interactively
gitmaster practice --scenario merge_conflict --interactive

# Run in non-interactive mode (setup only)
gitmaster practice --scenario basic_branching

# Use custom sandbox path for debugging
gitmaster practice --scenario merge_conflict --sandbox-path C:\temp\debug
```

### Command Options

- `-s, --scenario <name>`: Specific practice scenario to run
- `-i, --interactive`: Enable interactive mode with real-time monitoring
- `--sandbox-path <path>`: Custom path for practice sandbox (for debugging)
- `-v, --verbose`: Enable verbose output

## Creating New Scenarios

Scenarios are defined in YAML files in the `assets/practice/` directory. Each scenario includes:

### Scenario Structure

```yaml
name: "Scenario Name"
description: "Brief description of what the user will learn"
difficulty: "beginner|intermediate|advanced"
category: "branching|merging|collaboration|etc"
estimated_time: "X-Y minutes"

# Commands to set up the initial repository state
setup:
  - command: "git init"
    description: "Initialize repository"
  - command: "echo 'content' > file.txt"
    description: "Create initial file"

# Learning objectives for the user
objectives:
  - id: "unique_id"
    goal: "What the user should accomplish"
    command: "expected git command"  # Optional
    expected_result: "result_type"   # Optional
    validation_type: "file_content"  # Optional
    target_file: "filename"          # For file_content validation
    expected_content_contains: ["text1", "text2"]  # For file_content validation
    hint: "Helpful hint for the user"

# Overall success criteria
success_criteria:
  - type: "branch_merged"
    description: "Description of success condition"

# Dynamic evaluation hints
evaluation:
  - condition: "merge_in_progress"
    message: "Contextual feedback message"
```

### Validation Types

#### Command-Based Validation
Validates specific expected results from git operations:
- `merge_conflict`: Detects when a merge conflict occurs
- `shows_conflicts`: Verifies git status shows conflicted files
- `file_staged`: Confirms files are staged and conflicts resolved
- `merge_completed`: Ensures merge is successfully completed

#### File Content Validation
Validates file contents against expected patterns:
```yaml
validation_type: "file_content"
target_file: "app.py"
expected_content_contains: ["login", "authentication"]
```

## Available Scenarios

### merge_conflict
- **Difficulty**: Intermediate
- **Focus**: Resolving merge conflicts between branches
- **Skills**: Merging, conflict resolution, file editing

### basic_branching
- **Difficulty**: Beginner  
- **Focus**: Creating and switching between branches
- **Skills**: Branch creation, checkout, basic workflow

## Architecture

### Core Components

1. **PracticeService**: Manages scenario loading and evaluation
2. **GitRepositoryService**: Wraps LibGit2Sharp for git operations
3. **PracticeRunner**: Orchestrates interactive practice sessions
4. **PracticeModels**: Data structures for scenarios and sessions

### Evaluation Engine

The system continuously monitors the sandbox repository state and evaluates:
- Git repository status (branches, conflicts, merge state)
- File contents and modifications
- Command execution results
- User progress through objectives

### Sandbox Management

- Creates isolated temporary directories
- Initializes Git repositories with scenario-specific state
- Configures git user settings for practice sessions
- Supports custom paths for scenario debugging

## Example Session Flow

1. **Scenario Selection**: User selects or specifies a practice scenario
2. **Sandbox Creation**: System creates temporary directory and initializes Git repo
3. **State Setup**: Executes scenario setup commands to create initial state
4. **Interactive Loop**: 
   - Display current objective
   - Monitor repository state
   - Provide real-time feedback
   - Advance to next objective when completed
5. **Session Summary**: Display completion status and learning outcomes

## Debugging Scenarios

Use the `--sandbox-path` option to specify a custom directory for scenario development:

```bash
gitmaster practice --scenario my_scenario --sandbox-path C:\temp\debug --verbose
```

This allows you to:
- Inspect the exact repository state
- Manually test scenario setup commands
- Verify evaluation logic
- Debug objective validation

## Best Practices

### Scenario Design
- Start with simple, clear objectives
- Provide helpful hints that guide without giving away answers
- Test scenarios thoroughly with different user approaches
- Include both positive and negative test cases

### Objective Validation
- Use specific, measurable validation criteria
- Combine multiple validation types for robust checking
- Provide meaningful feedback messages
- Consider edge cases and alternative solutions

### File Management
- Use relative paths for cross-platform compatibility
- Keep file contents simple and focused
- Use descriptive filenames that relate to the learning objective
