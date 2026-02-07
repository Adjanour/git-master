# Contributing to GitMaster

Thank you for your interest in contributing to GitMaster! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Coding Standards](#coding-standards)

## Code of Conduct

Please be respectful and considerate of others. We aim to create a welcoming environment for all contributors.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/git-master.git
   cd git-master
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/Adjanour/git-master.git
   ```

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, Visual Studio Code, or Rider recommended)

### Building the Project

```bash
cd GitMaster
dotnet restore
dotnet build
```

### Running the Application

```bash
cd GitMaster
dotnet run
```

Or with arguments:

```bash
cd GitMaster
dotnet run -- practice
dotnet run -- cheat merge
dotnet run -- learn basics
```

## Making Changes

1. Create a new branch for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes following our [coding standards](#coding-standards)

3. Test your changes thoroughly

4. Commit your changes with a clear commit message:
   ```bash
   git commit -m "Add feature: description of your changes"
   ```

## Testing

Before submitting your changes:

1. Build the project:
   ```bash
   dotnet build
   ```

2. Run tests (if available):
   ```bash
   dotnet test
   ```

3. Test the application manually:
   ```bash
   dotnet run -- <command>
   ```

4. Verify your changes work across different scenarios

## Submitting Changes

1. Push your changes to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. Open a Pull Request on GitHub:
   - Go to the [original repository](https://github.com/Adjanour/git-master)
   - Click "New Pull Request"
   - Select your fork and branch
   - Provide a clear description of your changes
   - Reference any related issues

3. Wait for review:
   - Address any feedback from reviewers
   - Make additional commits if needed
   - Be patient and responsive

## Coding Standards

### C# Guidelines

- Follow [C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for asynchronous operations

### Project Structure

```
GitMaster/
├── Commands/         # Command implementations
├── Models/          # Data models
├── Services/        # Business logic services
├── assets/          # Static resources
└── docs/            # Documentation
```

### File Organization

- One class per file
- File name should match the class name
- Group related functionality in namespaces

### Code Style

- Use 4 spaces for indentation (not tabs)
- Place opening braces on a new line
- Use nullable reference types (`#nullable enable`)
- Prefer explicit types over `var` when clarity matters

### Comments

- Write self-documenting code with clear names
- Add comments for complex logic
- Use XML documentation for public APIs
- Keep comments up to date with code changes

### Example

```csharp
namespace GitMaster.Services
{
    /// <summary>
    /// Provides functionality for managing user progress.
    /// </summary>
    public class ProgressService
    {
        /// <summary>
        /// Saves the user's progress to storage.
        /// </summary>
        /// <param name="progress">The progress data to save.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool SaveProgress(ProgressData progress)
        {
            // Implementation
        }
    }
}
```

## Types of Contributions

We welcome various types of contributions:

### Bug Reports

- Use the issue tracker
- Include steps to reproduce
- Provide system information
- Include error messages/screenshots

### Feature Requests

- Use the issue tracker
- Describe the problem you're solving
- Explain your proposed solution
- Consider alternatives

### Documentation

- Fix typos and grammar
- Improve clarity
- Add examples
- Keep documentation up to date

### Code Contributions

- Bug fixes
- New features
- Performance improvements
- Refactoring
- Test coverage

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Spectre.Console Documentation](https://spectreconsole.net/)
- [Git Documentation](https://git-scm.com/doc)

## Questions?

If you have questions, feel free to:
- Open an issue
- Start a discussion
- Reach out to maintainers

Thank you for contributing to GitMaster!
