# GitMaster CLI Interface

GitMaster is a comprehensive Git learning and practice tool with an intuitive command-line interface built using Spectre.Console.

## Quick Start

```bash
# Get help
gitmaster --help

# Start practicing Git
gitmaster practice

# View available cheat sheets
gitmaster cheat

# Access learning modules
gitmaster learn

# Check your progress
gitmaster progress
```

## Commands Overview

### `gitmaster practice`
Start interactive Git practice sessions with various scenarios and difficulties.

**Options:**
- `-s|--scenario <name>` - Run specific practice scenario
- `-d|--difficulty <level>` - Set difficulty (beginner/intermediate/advanced)
- `-i|--interactive` - Enable guided hints and interactive mode

**Examples:**
```bash
gitmaster practice
gitmaster practice --scenario "merge-conflict" --difficulty advanced
gitmaster practice --interactive --difficulty beginner
```

### `gitmaster cheat [topic]`
Access Git cheat sheets organized by topic. Run without arguments to see available topics.

**Options:**
- `-f|--format <type>` - Output format (table/list/markdown)
- `-s|--search <term>` - Filter commands by search term

**Examples:**
```bash
gitmaster cheat                          # List all topics
gitmaster cheat merge                    # Show merge cheat sheet
gitmaster cheat branching --format list  # List format
gitmaster cheat rebase --search "cherry" # Search within topic
```

### `gitmaster learn [module]`
Access structured learning modules. Run without arguments to see available modules.

**Options:**
- `-l|--list` - List all available modules
- `-r|--resume` - Resume from last position
- `-c|--chapter <number>` - Start from specific chapter

**Examples:**
```bash
gitmaster learn                    # List all modules
gitmaster learn basics             # Start basics module
gitmaster learn collaboration --resume  # Resume where you left off
gitmaster learn advanced --chapter 3    # Start from chapter 3
```

### `gitmaster progress`
View your learning progress and statistics with detailed breakdowns.

**Options:**
- `-d|--detailed` - Show detailed progress breakdown
- `-m|--module <name>` - Filter by specific module
- `--export <format>` - Export progress (json/csv)

**Examples:**
```bash
gitmaster progress                    # Basic progress view
gitmaster progress --detailed         # Detailed breakdown
gitmaster progress --module basics    # Module-specific progress
gitmaster progress --export json      # Export to JSON
```

### `gitmaster reset-progress`
Reset your learning progress with optional backup and module filtering.

**Options:**
- `-m|--module <name>` - Reset only specific module
- `-f|--force` - Skip confirmation prompt
- `--backup` - Create backup before reset

**Examples:**
```bash
gitmaster reset-progress              # Reset all progress
gitmaster reset-progress --module basics  # Reset specific module
gitmaster reset-progress --backup --force # Force reset with backup
```

### `gitmaster update`
Update GitMaster to the latest version with various update options.

**Options:**
- `--check-only` - Only check for updates, don't install
- `--pre-release` - Include pre-release versions
- `--force` - Force update even if on latest version

**Examples:**
```bash
gitmaster update                # Update to latest stable
gitmaster update --check-only   # Check for updates only
gitmaster update --pre-release  # Include beta versions
```

## Global Options

All commands support these global options:

- `--repo-path <path>` - Specify Git repository path (defaults to current directory)
- `--verbose` - Enable detailed output for debugging and transparency
- `--color <mode>` - Control colored output (auto/always/never)

**Examples:**
```bash
gitmaster --verbose practice
gitmaster --repo-path /path/to/repo cheat merge
gitmaster --color never progress --detailed
```

## Help System

Get help for any command using the `--help` flag:

```bash
gitmaster --help                # Main help
gitmaster practice --help       # Command-specific help
gitmaster cheat --help          # Show all options and examples
```

## Features

### Rich Visual Output
- **Colored Output**: Syntax highlighting and status colors
- **Tables**: Well-formatted data presentation
- **Progress Bars**: Visual progress tracking
- **Interactive Prompts**: Confirmation dialogs and selections

### User-Friendly Design
- **Progressive Disclosure**: Basic functionality works without options
- **Sensible Defaults**: Common use cases require minimal configuration
- **Error Prevention**: Confirmation prompts for destructive actions
- **Comprehensive Help**: Built-in documentation and examples

### Cross-Platform Support
Works consistently across Windows, macOS, and Linux with native terminal support.

## Learning Path

1. **Start with basics**: `gitmaster learn basics`
2. **Practice scenarios**: `gitmaster practice --interactive`
3. **Reference materials**: `gitmaster cheat basics`
4. **Track progress**: `gitmaster progress --detailed`
5. **Advance gradually**: `gitmaster learn branching`

## Tips

- Use `--verbose` to understand what the tool is doing
- Start with interactive practice mode for guided learning
- Check available topics/modules before diving in
- Export your progress periodically as backup
- Use the cheat sheets as quick reference during practice

## Examples Workflow

```bash
# 1. Check what's available
gitmaster learn
gitmaster cheat

# 2. Start learning
gitmaster learn basics --interactive

# 3. Practice what you learned
gitmaster practice --scenario "basic-workflow" --interactive

# 4. Reference materials when stuck
gitmaster cheat basics --search "commit"

# 5. Track your progress
gitmaster progress --detailed

# 6. Continue learning
gitmaster learn branching --resume
```
