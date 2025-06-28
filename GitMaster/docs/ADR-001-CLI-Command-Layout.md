# ADR-001: CLI Command Layout and Interface Design

## Status
Accepted

## Context
We need to design a comprehensive CLI interface for GitMaster that provides intuitive access to learning resources, practice scenarios, progress tracking, and maintenance functions. The CLI should be user-friendly for beginners while offering advanced options for experienced users.

## Decision
We have decided to implement a command-based CLI using Spectre.Console.Cli with the following structure:

### Primary Commands
- `gitmaster practice` - Interactive Git practice sessions
- `gitmaster cheat <topic>` - Git cheat sheets organized by topic
- `gitmaster learn <module>` - Structured learning modules
- `gitmaster progress` - Progress tracking and statistics
- `gitmaster reset-progress` - Progress reset functionality
- `gitmaster update` - Self-update mechanism

### Global Options
All commands support these global flags:
- `--repo-path <path>` - Specify Git repository path (defaults to current directory)
- `--verbose` - Enable detailed output for debugging and transparency
- `--color <mode>` - Control colored output (auto/always/never)

### Command-Specific Options

#### Practice Command
- `-s|--scenario <name>` - Run specific practice scenario
- `-d|--difficulty <level>` - Set difficulty (beginner/intermediate/advanced)
- `-i|--interactive` - Enable guided hints and interactive mode

#### Cheat Command
- `<topic>` - Required argument for cheat sheet topic
- `-f|--format <type>` - Output format (table/list/markdown)
- `-s|--search <term>` - Filter commands by search term

#### Learn Command
- `<module>` - Required argument for learning module
- `-l|--list` - List all available modules
- `-r|--resume` - Resume from last position
- `-c|--chapter <number>` - Start from specific chapter

#### Progress Command
- `-d|--detailed` - Show detailed progress breakdown
- `-m|--module <name>` - Filter by specific module
- `--export <format>` - Export progress (json/csv)

#### Reset Progress Command
- `-m|--module <name>` - Reset only specific module
- `-f|--force` - Skip confirmation prompt
- `--backup` - Create backup before reset

#### Update Command
- `--check-only` - Only check for updates, don't install
- `--pre-release` - Include pre-release versions
- `--force` - Force update even if on latest version

## Rationale

### Framework Choice: Spectre.Console.Cli
- **Rich UI Support**: Provides tables, progress bars, colored output, and interactive prompts
- **Automatic Help Generation**: Built-in help system with examples and descriptions
- **Strong Typing**: Command settings are strongly typed with validation
- **Extensibility**: Easy to add new commands and options
- **Cross-Platform**: Works consistently across Windows, macOS, and Linux

### Command Structure
- **Verb-based Commands**: Clear, action-oriented command names (practice, learn, cheat)
- **Logical Grouping**: Related functionality grouped under single commands with options
- **Discoverability**: Help system guides users to available options and examples

### Global Options Design
- **Consistent Interface**: Same global options work across all commands
- **Repository Awareness**: `--repo-path` allows working with different repositories
- **Debugging Support**: `--verbose` provides transparency for troubleshooting
- **Accessibility**: `--color` option supports different terminal environments

### User Experience Considerations
- **Progressive Disclosure**: Basic commands work without options, advanced features available via flags
- **Sensible Defaults**: Common use cases work with minimal configuration
- **Error Prevention**: Confirmation prompts for destructive actions (reset-progress)
- **Visual Feedback**: Progress indicators, colored output, and clear status messages

## Consequences

### Positive
- **Intuitive Interface**: Users can quickly discover and use functionality
- **Consistent Experience**: Global options and patterns work the same across commands
- **Rich Output**: Tables, progress bars, and colors improve readability
- **Extensible**: Easy to add new commands and options as features grow
- **Self-Documenting**: Built-in help system reduces documentation burden

### Negative
- **Dependency**: Requires Spectre.Console.Cli package
- **Learning Curve**: Developers need to understand Spectre.Console patterns
- **Binary Size**: Additional dependencies increase application size

### Trade-offs
- Chose rich CLI framework over minimal approach for better user experience
- Prioritized discoverability over brevity in command names
- Selected verb-based commands over noun-based for clarity

## Implementation Notes

### Command Settings Inheritance
All command settings inherit from `GlobalSettings` to ensure consistent global option support.

### Help System
Spectre.Console.Cli automatically generates help text from:
- Command descriptions
- Option descriptions
- Usage examples
- Default values

### Error Handling
Commands return appropriate exit codes:
- 0: Success
- 1: General error
- Non-zero: Specific error conditions

### Future Considerations
- Command aliases (e.g., `gm` as shorthand for `gitmaster`)
- Plugin system for custom commands
- Configuration file support for default options
- Shell completion scripts

## Examples

```bash
# Basic usage
gitmaster practice
gitmaster cheat merge
gitmaster learn basics

# With global options
gitmaster --verbose --repo-path /path/to/repo practice
gitmaster --color never progress --detailed

# Advanced scenarios
gitmaster practice --scenario "merge-conflict" --difficulty advanced
gitmaster cheat branching --format markdown --search "cherry-pick"
gitmaster learn collaboration --resume --chapter 3
gitmaster reset-progress --module basics --backup
```

## Related Decisions
- ADR-002: Progress Storage Format (future)
- ADR-003: Practice Scenario Engine (future)
- ADR-004: Learning Content Structure (future)
