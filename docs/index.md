# GitMaster Documentation

Welcome to the GitMaster documentation! GitMaster is a comprehensive Git learning and practice application built with .NET.

## Table of Contents

- [Getting Started](#getting-started)
- [Installation](#installation)
- [Commands](#commands)
- [Features](#features)
- [Contributing](#contributing)

## Getting Started

GitMaster helps you learn and master Git through interactive practice, structured lessons, and quick reference cheat sheets.

### Quick Start

```bash
# Clone the repository
git clone https://github.com/Adjanour/git-master.git
cd git-master/GitMaster

# Build the project
dotnet build

# Run GitMaster
dotnet run
```

## Installation

### From Release Binaries

1. Download the latest release for your platform from the [Releases](https://github.com/Adjanour/git-master/releases) page
2. Extract the archive
3. Add the executable to your PATH
4. Run `gitmaster --help`

### From Source

```bash
# Clone and build
git clone https://github.com/Adjanour/git-master.git
cd git-master/GitMaster
dotnet build --configuration Release

# Run
dotnet run
```

## Commands

GitMaster provides several commands to help you learn and practice Git:

### Practice Command

Interactive Git practice sessions with real scenarios.

```bash
gitmaster practice
gitmaster practice --scenario "merge-conflict"
gitmaster practice --difficulty advanced
```

**Options:**
- `-s, --scenario <name>` - Run a specific practice scenario
- `-d, --difficulty <level>` - Set difficulty (beginner/intermediate/advanced)
- `-i, --interactive` - Enable guided hints and interactive mode

### Cheat Command

Quick reference for Git commands organized by topic.

```bash
gitmaster cheat
gitmaster cheat merge
gitmaster cheat branching --format markdown
```

**Options:**
- `<topic>` - Cheat sheet topic (e.g., merge, branch, rebase)
- `-f, --format <type>` - Output format (table/list/markdown)
- `-s, --search <term>` - Filter commands by search term

### Learn Command

Structured learning modules to build Git knowledge from the ground up.

```bash
gitmaster learn
gitmaster learn basics
gitmaster learn collaboration --resume
```

**Options:**
- `<module>` - Learning module name
- `-l, --list` - List all available modules
- `-r, --resume` - Resume from last position
- `-c, --chapter <number>` - Start from specific chapter

### Progress Command

Track your learning progress and achievements.

```bash
gitmaster progress
gitmaster progress --detailed
gitmaster progress --export json
```

**Options:**
- `-d, --detailed` - Show detailed progress breakdown
- `-m, --module <name>` - Filter by specific module
- `--export <format>` - Export progress (json/csv)

### Reset Progress Command

Reset your progress for a fresh start.

```bash
gitmaster reset-progress
gitmaster reset-progress --module basics
gitmaster reset-progress --force --backup
```

**Options:**
- `-m, --module <name>` - Reset only specific module
- `-f, --force` - Skip confirmation prompt
- `--backup` - Create backup before reset

### Update Command

Keep GitMaster up to date.

```bash
gitmaster update
gitmaster update --check-only
```

**Options:**
- `--check-only` - Only check for updates, don't install
- `--pre-release` - Include pre-release versions
- `--force` - Force update even if on latest version

## Features

### Interactive Practice

Practice real Git scenarios in a safe environment:
- Merge conflicts
- Branch management
- Rebasing
- Cherry-picking
- And more!

### Structured Learning

Follow comprehensive learning modules:
- **Basics**: Git fundamentals and core concepts
- **Branching**: Branch management and workflows
- **Collaboration**: Working with remote repositories
- **Advanced**: Rebasing, cherry-picking, and advanced techniques

### Quick Reference

Access Git command cheat sheets instantly:
- Organized by topic
- Searchable
- Multiple output formats

### Progress Tracking

Monitor your learning journey:
- Track completed lessons and scenarios
- View statistics and achievements
- Export progress data

## Global Options

All commands support these global flags:

- `--repo-path <path>` - Specify Git repository path (defaults to current directory)
- `--verbose` - Enable detailed output for debugging
- `--color <mode>` - Control colored output (auto/always/never)

## Examples

### Basic Usage

```bash
# Start a practice session
gitmaster practice

# View Git merge cheat sheet
gitmaster cheat merge

# Learn Git basics
gitmaster learn basics

# Check your progress
gitmaster progress
```

### Advanced Usage

```bash
# Practice with specific scenario
gitmaster practice --scenario "merge-conflict" --difficulty advanced

# Search cheat sheet
gitmaster cheat branching --format markdown --search "cherry-pick"

# Resume learning from where you left off
gitmaster learn collaboration --resume --chapter 3

# Reset progress with backup
gitmaster reset-progress --module basics --backup
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](../README.md#contributing) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/Adjanour/git-master.git
cd git-master/GitMaster

# Build
dotnet build

# Run
dotnet run
```

## License

This project is licensed under the terms specified in [LICENSE.txt](../LICENSE.txt).

## Support

- [Issues](https://github.com/Adjanour/git-master/issues)
- [Discussions](https://github.com/Adjanour/git-master/discussions)

## Additional Resources

- [Release Process Guide](RELEASE.md) - How to create releases
- [GitHub Pages Setup](GITHUB_PAGES.md) - Documentation deployment setup
- [Contributing Guide](../CONTRIBUTING.md) - How to contribute to the project

---

**Built with ❤️ using .NET and Spectre.Console**
