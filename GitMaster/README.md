# GitMaster

A comprehensive Git learning and practice application built with .NET.

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

## Getting Started

1. Clone the repository
2. Navigate to the project directory
3. Build and run the application:
   ```bash
   cd GitMaster
   dotnet build
   dotnet run
   ```

## Features

- Interactive Git cheatsheet
- Hands-on practice exercises
- Structured learning lessons
- Progress tracking
- User-friendly interface

## Development

This project follows standard .NET development practices. The main application is in the `GitMaster/` directory, with additional modules organized in the `internal/` and `pkg/` directories.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

[Add your license information here]
