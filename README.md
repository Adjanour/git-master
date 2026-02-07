# GitMaster

[![CI](https://github.com/Adjanour/git-master/actions/workflows/ci.yml/badge.svg)](https://github.com/Adjanour/git-master/actions/workflows/ci.yml)
[![Documentation](https://github.com/Adjanour/git-master/actions/workflows/docs.yml/badge.svg)](https://github.com/Adjanour/git-master/actions/workflows/docs.yml)
[![License](https://img.shields.io/github/license/Adjanour/git-master)](LICENSE.txt)

A comprehensive Git learning and practice application built with .NET.

📖 **[View Documentation](https://adjanour.github.io/git-master/)** | 🚀 **[Latest Release](https://github.com/Adjanour/git-master/releases/latest)**

## Project Structure

```
git-master/
├── GitMaster/              # Main .NET console application
│   ├── Program.cs          # Entry point
│   └── GitMaster.csproj    # Project file
├── cmd/                    # Main entry points
├── internal/               # Internal packages
│   ├── cheatsheet/         # Git cheatsheet functionality
│   ├── practice/           # Practice exercises
│   ├── lessons/            # Learning lessons
│   └── progress/           # Progress tracking
├── pkg/                    # Public packages
│   └── ui/                 # User interface components
├── assets/                 # Static assets (lesson JSON/Markdown)
├── tests/                  # Test files
├── docs/                   # Documentation
└── README.md               # This file
```

## Installation

### From Release Binaries (Recommended)

1. Download the latest release for your platform from the [Releases](https://github.com/Adjanour/git-master/releases) page
2. Extract the archive
3. Add the executable to your PATH
4. Run `gitmaster --help` to get started

### From Source

1. Clone the repository
2. Navigate to the project directory
3. Build and run the application:
   ```bash
   cd GitMaster
   dotnet build
   dotnet run
   ```

## Quick Start

```bash
# View available commands
gitmaster --help

# Start learning Git basics
gitmaster learn basics

# Practice Git scenarios
gitmaster practice

# Quick reference for Git commands
gitmaster cheat merge

# Check your progress
gitmaster progress
```

## Features

- **Interactive Git cheatsheet** - Quick reference for Git commands organized by topic
- **Hands-on practice exercises** - Real scenarios in a safe environment
- **Structured learning lessons** - Comprehensive modules to build Git knowledge
- **Progress tracking** - Monitor your learning journey
- **User-friendly interface** - Built with Spectre.Console for rich terminal UI
- **Cross-platform** - Works on Windows, macOS, and Linux

## Documentation

For detailed documentation, visit [https://adjanour.github.io/git-master/](https://adjanour.github.io/git-master/)

- [Commands Reference](https://adjanour.github.io/git-master/#commands)
- [Installation Guide](https://adjanour.github.io/git-master/#installation)
- [Examples](https://adjanour.github.io/git-master/#examples)

## Development

This project follows standard .NET development practices. The main application is in the `GitMaster/` directory, with additional modules organized in the `internal/` and `pkg/` directories.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the terms specified in [LICENSE.txt](LICENSE.txt).

## Support

- 📖 [Documentation](https://adjanour.github.io/git-master/)
- 🐛 [Report Issues](https://github.com/Adjanour/git-master/issues)
- 💬 [Discussions](https://github.com/Adjanour/git-master/discussions)
- 📥 [Download Latest Release](https://github.com/Adjanour/git-master/releases/latest)
