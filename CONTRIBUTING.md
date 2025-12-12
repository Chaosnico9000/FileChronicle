# Contributing to FileChronicle

First off, thank you for considering contributing to FileChronicle! It's people like you that make FileChronicle such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

* **Use a clear and descriptive title** for the issue
* **Describe the exact steps which reproduce the problem**
* **Provide specific examples** to demonstrate the steps
* **Describe the behavior you observed** and what behavior you expected
* **Include screenshots** if relevant
* **Include your environment details** (OS, .NET version, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

* **Use a clear and descriptive title**
* **Provide a detailed description** of the suggested enhancement
* **Explain why this enhancement would be useful** to most FileChronicle users
* **List some other tools where this enhancement exists**, if applicable

### Pull Requests

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. Ensure the test suite passes
4. Make sure your code follows the existing code style
5. Write a clear commit message

## Development Setup

### Prerequisites

- .NET 10 SDK or later
- A code editor (Visual Studio, VS Code, Rider, etc.)
- Git

### Building from Source

```bash
# Clone your fork
git clone https://github.com/your-username/FileChronicle.git
cd FileChronicle

# Build the project
dotnet build

# Run the application
dotnet run -- snapshot C:\Test output.json

# Run tests (when available)
dotnet test
```

### Project Structure

```
FileChronicle/
??? Program.cs              # Main application entry and command implementations
??? FileChronicle.csproj    # Project file
??? README.md               # Documentation
??? LICENSE                 # MIT License
??? CHANGELOG.md            # Version history
```

## Coding Guidelines

### C# Style Guide

We follow standard C# coding conventions:

#### Naming Conventions

```csharp
// Classes and methods: PascalCase
public class SnapshotManager { }
public void CreateSnapshot() { }

// Local variables and parameters: camelCase
var fileName = "test.json";
void ProcessFile(string filePath) { }

// Private fields: _camelCase
private Configuration? _config;

// Constants: PascalCase
private const int BufferSize = 81920;
```

#### Code Structure

- Use async/await for I/O operations
- Prefer `var` for local variables when type is obvious
- Use expression-bodied members when appropriate
- Keep methods focused and small
- Add XML documentation comments for public APIs

#### Example

```csharp
/// <summary>
/// Creates a snapshot of the specified directory.
/// </summary>
/// <param name="directory">The directory to snapshot.</param>
/// <param name="options">Snapshot options.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A snapshot containing file information.</returns>
private static async Task<Snapshot> CreateSnapshotAsync(
    string directory,
    SnapshotOptions options,
    CancellationToken cancellationToken = default)
{
    var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
    var entries = new List<FileEntry>();

    foreach (var file in files)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // Process file...
    }

    return new Snapshot
    {
        CreatedAtUtc = DateTime.UtcNow,
        Files = entries
    };
}
```

### Error Handling

- Use try-catch blocks for expected exceptions
- Provide meaningful error messages
- Log errors appropriately
- Clean up resources properly

```csharp
try
{
    await ProcessFileAsync(file, cancellationToken);
}
catch (UnauthorizedAccessException)
{
    WriteColorLine($"? Access denied: {file}", ConsoleColor.Yellow);
    skipped++;
}
catch (Exception ex)
{
    WriteColorLine($"? Failed to process {file}: {ex.Message}", ConsoleColor.Yellow);
    skipped++;
}
```

### Testing

When adding new features, please include tests:

```csharp
[Fact]
public void MatchesPattern_WithWildcard_ReturnsTrue()
{
    // Arrange
    var path = "src/Program.cs";
    var pattern = "*.cs";

    // Act
    var result = MatchesPattern(path, pattern);

    // Assert
    Assert.True(result);
}
```

## Commit Messages

We follow conventional commits format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

```
feat(snapshot): add support for archive files

Add support for creating snapshots from ZIP and TAR archives.
Includes new command line options --archive and --archive-format.

Closes #123
```

```
fix(diff): correct size change calculation

Fix incorrect percentage calculation when old size is zero.
Now shows N/A instead of dividing by zero.

Fixes #456
```

## Feature Requests

### What Makes a Good Feature Request?

1. **Clear Use Case**: Describe the problem you're trying to solve
2. **Expected Behavior**: Explain how the feature should work
3. **Alternative Solutions**: Mention any workarounds you've considered
4. **Additional Context**: Add any other relevant information

### Feature Development Process

1. **Discussion**: Create an issue to discuss the feature
2. **Design**: If it's a major feature, create a design document
3. **Implementation**: Develop the feature in a branch
4. **Review**: Submit a pull request for review
5. **Testing**: Ensure tests pass and code is documented
6. **Merge**: Maintainers will merge after approval

## Areas for Contribution

Here are some areas where contributions are especially welcome:

### Easy Pickings (Good First Issues)

- Improve error messages
- Add more examples to documentation
- Fix typos and improve clarity in README
- Add unit tests for existing functionality
- Improve console output formatting

### Medium Complexity

- Add new export formats (XML, Markdown)
- Implement additional pattern matching features
- Add more configuration options
- Improve performance for large directories
- Add command-line autocompletion

### Advanced Features

- Archive file support (ZIP, TAR)
- Database storage for snapshots
- Plugin system architecture
- GUI wrapper
- Network directory support

## Questions?

Don't hesitate to ask questions! You can:

1. Open an issue with the question label
2. Start a discussion in GitHub Discussions
3. Check existing documentation and issues

## Recognition

Contributors will be recognized in:

- The project README
- Release notes
- A CONTRIBUTORS file

Thank you for contributing to FileChronicle! ??
