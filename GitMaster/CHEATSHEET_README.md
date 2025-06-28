# GitMaster Cheat Sheet Module

## Overview

The Cheat Sheet module provides a comprehensive Git command reference with syntax highlighting, examples, and fuzzy search capabilities. It stores command data in YAML format and renders it with colored output using Spectre.Console.

## Features Implemented

### ✅ YAML Data Storage
- **File**: `assets/cheatsheet.yml`
- **Structure**: Organized by topics (basics, branching, rebase, remote, stash, reset, hooks)
- **Content**: Each command includes:
  - Name and syntax
  - Description
  - Examples with explanations
  - Tags for categorization

### ✅ CLI Renderer with Colored Highlights
- **Service**: `Services/CheatSheetRenderer.cs`
- **Features**:
  - Colored syntax highlighting for Git commands
  - Tabular format for topic overviews
  - Individual command panels with examples
  - Multiple format options (table, list, markdown)
  - Error handling with suggestions

### ✅ Fuzzy Search Implementation
- **Service**: `Services/CheatSheetService.cs`
- **Package**: FuzzySharp for fuzzy string matching
- **Capabilities**:
  - Global search across all commands: `gitmaster cheat search "rebase"`
  - Topic-specific search: `gitmaster cheat basics --search "add"`
  - Similar topic suggestions for typos
  - Configurable similarity thresholds

### ✅ Unit Tests
- **File**: `Tests/CheatSheetTests.cs`
- **Framework**: xUnit
- **Coverage**:
  - YAML parsing and deserialization
  - Search functionality validation
  - Topic retrieval testing
  - Renderer instantiation tests

## Usage Examples

### View All Topics
```bash
gitmaster cheat
```

### View Specific Topic
```bash
gitmaster cheat rebase
gitmaster cheat basics --format list
```

### Search Commands
```bash
# Global search
gitmaster cheat search "rebase"
gitmaster cheat search "commit"

# Topic-specific search
gitmaster cheat basics --search "add"
```

### Error Handling
```bash
gitmaster cheat invalid    # Shows error with suggestions
gitmaster cheat branch     # Suggests "branching"
```

## Architecture

### Data Models (`Models/CheatSheetModels.cs`)
- `CheatSheetData`: Root container for all topics
- `Topic`: Contains title, description, and commands
- `Command`: Individual Git command with syntax, examples, and tags
- `Example`: Command example with description

### Services
1. **CheatSheetService**: Data loading, searching, and topic management
2. **CheatSheetRenderer**: Formatted output with colors and styling

### Dependencies
- **YamlDotNet**: YAML parsing and deserialization
- **FuzzySharp**: Fuzzy string matching for search
- **Spectre.Console**: Rich terminal output with colors and tables
- **xUnit**: Unit testing framework

## Data Structure Example

```yaml
topics:
  rebase:
    title: "Rebasing and History Modification"
    description: "Commands for rebasing and modifying commit history"
    commands:
      - name: "git rebase"
        syntax: "git rebase [options] <upstream> [branch]"
        description: "Reapply commits on top of another base tip"
        examples:
          - command: "git rebase main"
            description: "Rebase current branch onto main"
        tags: ["rebase", "history", "interactive"]
```

## Testing

The module includes comprehensive unit tests covering:
- YAML parsing functionality
- Search algorithm accuracy
- Topic retrieval and error handling
- Renderer initialization

Run tests with:
```bash
dotnet test
```

## Build and Run

```bash
# Build the project
dotnet build

# Run with various commands
dotnet run -- cheat
dotnet run -- cheat rebase
dotnet run -- cheat search "commit"
```

## Technical Implementation Notes

1. **Markup Escaping**: Uses `EscapeMarkup()` to prevent conflicts with Spectre.Console's markup syntax
2. **Fuzzy Matching**: Implements configurable similarity thresholds (default 70% for commands, 60% for topics)
3. **Asset Management**: YAML file is copied to output directory during build
4. **Error Handling**: Graceful error handling with helpful suggestions
5. **Performance**: Loads YAML once at service initialization for fast lookups

## Future Enhancements

Potential improvements for the cheat sheet module:
- Custom syntax highlighting themes
- Command history and favorites
- Integration with Git help system
- Offline documentation caching
- Interactive command builder
