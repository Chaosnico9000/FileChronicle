# FileChronicle 📸

<div align="center">

**Advanced Directory Snapshot & Diff Tool**

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 14.0](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

</div>

## 🎯 Overview

FileChronicle is a powerful command-line tool for creating directory snapshots, comparing file changes, and monitoring directories in real-time. It provides detailed insights into file modifications with support for multiple export formats and an interactive mode with macro support.

## ✨ Features

### Core Functionality
- 📸 **Directory Snapshots** - Create detailed snapshots of directory structures with file metadata
- 🔍 **Smart Diffing** - Compare snapshots to identify added, removed, and modified files
- 👁️ **Real-time Monitoring** - Watch directories for changes with configurable intervals
- ⚡ **Fast Comparison** - Direct directory comparison without creating snapshots

### Advanced Features
- 🎨 **Multiple Export Formats** - JSON, CSV, and HTML reports
- 🔐 **SHA-256 Hashing** - Cryptographic file integrity verification
- 🎭 **Pattern Matching** - Include/exclude files with wildcards (`*.cs`, `bin/**`)
- 🎮 **Interactive Mode** - REPL-style interface with command macros
- 💾 **Macro System** - Save and reuse complex commands
- 🔧 **Context Variables** - Dynamic values for timestamps and paths
- ⚙️ **Configurable** - Persistent configuration with sensible defaults

## 🚀 Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/FileChronicle.git

# Navigate to the project
cd FileChronicle

# Build the project
dotnet build

# Run the tool
dotnet run
```

### Basic Usage

```bash
# Create a snapshot
FileChronicle snapshot C:\Projects snapshot.json

# Compare two snapshots
FileChronicle diff old.json new.json --detailed

# Compare directories directly
FileChronicle compare C:\Projects\v1 C:\Projects\v2

# Watch directory for changes
FileChronicle watch C:\Projects --interval 10

# Launch interactive mode
FileChronicle interactive
```

## 📖 Documentation

### Commands

#### Snapshot
Create a snapshot of a directory structure.

```bash
FileChronicle snapshot <directory> <output.json> [options]

Options:
  --include <pattern>    Include only matching files (e.g., *.cs)
  --exclude <pattern>    Exclude matching files (e.g., *.tmp)
  --format <json|csv>    Output format (default: json)
  --no-hash             Skip hash computation (faster)
  --progress            Show progress during operation
```

**Examples:**
```bash
# Basic snapshot
FileChronicle snapshot C:\Projects output.json

# Exclude build artifacts
FileChronicle snapshot C:\Projects output.json --exclude bin\** --exclude obj\**

# Include only C# files
FileChronicle snapshot C:\Projects output.json --include *.cs

# CSV format without hashing
FileChronicle snapshot C:\Projects output.csv --format csv --no-hash
```

#### Diff
Compare two snapshots to identify changes.

```bash
FileChronicle diff <oldSnapshot.json> <newSnapshot.json> [options]

Options:
  --output <file>              Export diff to file
  --format <json|csv|html>     Output format
  --show-unchanged             Include unchanged files
  --detailed                   Show detailed file information
```

**Examples:**
```bash
# Basic diff
FileChronicle diff snapshot1.json snapshot2.json

# Detailed HTML report
FileChronicle diff old.json new.json --detailed --format html --output report.html

# Show all files including unchanged
FileChronicle diff old.json new.json --show-unchanged
```

#### Compare
Directly compare two directories without creating snapshots.

```bash
FileChronicle compare <dir1> <dir2> [options]

Options:
  --include <pattern>    Include only matching files
  --exclude <pattern>    Exclude matching files
  --detailed            Show detailed file information
  --progress            Show progress during operation
```

**Examples:**
```bash
# Compare two directories
FileChronicle compare C:\Projects\v1 C:\Projects\v2

# Detailed comparison with exclusions
FileChronicle compare C:\Source\old C:\Source\new --detailed --exclude *.log
```

#### Watch
Monitor a directory for changes in real-time.

```bash
FileChronicle watch <directory> [options]

Options:
  --include <pattern>      Include only matching files
  --exclude <pattern>      Exclude matching files
  --interval <seconds>     Check interval (default: 5)
```

**Examples:**
```bash
# Watch directory every 5 seconds
FileChronicle watch C:\Projects

# Watch with custom interval
FileChronicle watch C:\Projects --interval 10

# Watch only specific files
FileChronicle watch C:\Projects --include *.cs --include *.csproj
```

### Interactive Mode

Launch an interactive session with command history and macros.

```bash
FileChronicle interactive
# or
FileChronicle i
```

**Interactive Commands:**
- `@macroname` - Execute a saved macro
- `macros` - List all saved macros
- `context` - Show context variables
- `set <var> <value>` - Set a context variable
- `clear` / `cls` - Clear the screen
- `help` - Show help
- `exit` / `quit` - Exit interactive mode

**Example Session:**
```
FileChronicle> set projdir C:\MyProject
✅ Set $projdir = C:\MyProject

FileChronicle> macro save daily "snapshot $projdir snapshots\daily_$timestamp.json --progress"
✅ Macro '@daily' saved successfully.

FileChronicle> @daily
→ Executing macro: snapshot C:\MyProject snapshots\daily_20241215_143025.json --progress
📸 Creating snapshot for: C:\MyProject
...
```

### Macro System

Save frequently used commands as shortcuts.

```bash
# Save a macro
FileChronicle macro save mysnap "snapshot C:\Projects output.json"

# List all macros
FileChronicle macro list

# Execute a macro (in interactive mode)
@mysnap

# Delete a macro
FileChronicle macro delete mysnap
```

### Configuration

Manage persistent configuration settings.

```bash
# Show current configuration
FileChronicle config list

# Set configuration values
FileChronicle config set defaultFormat html
FileChronicle config set defaultWatchInterval 10
FileChronicle config set coloredOutput true

# Reset to defaults
FileChronicle config reset
```

**Configuration Options:**
- `defaultFormat` - Default output format (json, csv, html)
- `defaultWatchInterval` - Default watch interval in seconds
- `coloredOutput` - Enable/disable colored console output
- `defaultExcludePatterns` - Default file patterns to exclude

**Configuration Location:**
- Windows: `%APPDATA%\FileChronicle\config.json`
- Linux/Mac: `~/.config/FileChronicle/config.json`

### Context Variables

Built-in variables for dynamic values:

| Variable | Description | Example |
|----------|-------------|---------|
| `$cwd` | Current working directory | `C:\Projects` |
| `$timestamp` | Current timestamp | `20241215_143025` |
| `$date` | Current date | `20241215` |

**Custom Variables:**
```bash
FileChronicle> set myvar value
FileChronicle> snapshot $myvar\data output.json
```

## 🎨 Output Formats

### JSON
Structured data format, ideal for programmatic processing.

```json
{
  "CreatedAtUtc": "2024-12-15T14:30:25Z",
  "RootDirectory": "C:\\Projects",
  "Files": [
    {
      "RelativePath": "src/Program.cs",
      "Length": 12345,
      "LastWriteUtc": "2024-12-15T14:25:00Z",
      "Sha256": "ABC123..."
    }
  ]
}
```

### CSV
Tabular format for spreadsheet analysis.

```csv
RelativePath,Length,LastWriteUtc,Sha256
"src/Program.cs",12345,2024-12-15 14:25:00,ABC123...
```

### HTML
Beautiful, interactive reports with styling.

- Summary statistics
- Color-coded changes (added/removed/modified)
- Sortable tables
- Responsive design

## 🔧 Advanced Usage

### Pattern Matching

FileChronicle supports powerful wildcard patterns:

- `*.cs` - All C# files
- `**/*.txt` - All text files in any subdirectory
- `bin/**` - Everything in bin directory and subdirectories
- `temp?.log` - Files like temp1.log, temp2.log

**Combining Patterns:**
```bash
FileChronicle snapshot C:\Project output.json \
  --include *.cs \
  --include *.csproj \
  --exclude bin\** \
  --exclude obj\** \
  --exclude *.user
```

### Performance Tips

1. **Skip Hashing for Speed:**
   ```bash
   FileChronicle snapshot C:\Large --no-hash
   ```

2. **Exclude Large Directories:**
   ```bash
   FileChronicle snapshot C:\Project output.json \
     --exclude node_modules\** \
     --exclude .git\**
   ```

3. **Use Progress Reporting:**
   ```bash
   FileChronicle snapshot C:\Project output.json --progress
   ```

### Automation Examples

**Backup Script (PowerShell):**
```powershell
$date = Get-Date -Format "yyyyMMdd"
FileChronicle snapshot "C:\Projects" "backups\snapshot_$date.json"
```

**Compare with Previous Backup:**
```powershell
$yesterday = (Get-Date).AddDays(-1).ToString("yyyyMMdd")
$today = Get-Date -Format "yyyyMMdd"
FileChronicle diff "backups\snapshot_$yesterday.json" `
                   "backups\snapshot_$today.json" `
                   --format html --output "reports\changes_$today.html"
```

**CI/CD Integration:**
```bash
# Before deployment
FileChronicle snapshot ./app before-deploy.json

# After deployment
FileChronicle snapshot ./app after-deploy.json

# Generate change report
FileChronicle diff before-deploy.json after-deploy.json \
  --format html --output deployment-report.html
```

## 🏗️ Architecture

FileChronicle is built with:

- **.NET 10** - Latest .NET runtime
- **C# 14.0** - Modern language features
- **Async/Await** - Fully asynchronous I/O operations
- **SHA-256** - Cryptographic file integrity
- **JSON Serialization** - System.Text.Json

### Key Components

- **Snapshot Engine** - Efficient directory traversal and metadata collection
- **Diff Algorithm** - Smart file comparison with hash-based change detection
- **Pattern Matcher** - Regex-based wildcard pattern matching
- **Macro Manager** - Persistent command storage
- **Interactive Shell** - REPL with command history

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/FileChronicle.git

# Build in debug mode
dotnet build

# Run tests
dotnet test

# Run the application
dotnet run -- snapshot C:\Test output.json
```

### Code Style

- Follow C# coding conventions
- Use async/await consistently
- Add XML documentation for public APIs
- Write descriptive commit messages

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by git diff and directory comparison tools
- Built with ❤️ using .NET 10

## 📧 Contact

- Create an issue for bug reports or feature requests
- Pull requests are always welcome

---

<div align="center">

**[⬆ back to top](#filechronicle-)**

Made with ☕ and C#

</div>
