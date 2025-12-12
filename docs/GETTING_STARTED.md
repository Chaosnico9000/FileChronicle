# Getting Started with FileChronicle

Welcome to FileChronicle! This guide will help you get up and running quickly.

## Prerequisites

- .NET 10 SDK or later
- Windows, Linux, or macOS
- Terminal/Command Prompt

## Installation

### Option 1: Build from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/FileChronicle.git
cd FileChronicle

# Build the project
dotnet build

# Run the application
dotnet run
```

### Option 2: Using Build Script

**Windows (PowerShell):**
```powershell
.\build.ps1 -Configuration Release -Publish -Pack
```

**Linux/macOS:**
```bash
chmod +x build.sh
./build.sh --configuration Release --publish --pack
```

### Option 3: Install as Global Tool (when published to NuGet)

```bash
dotnet tool install --global FileChronicle
```

## Your First Snapshot

Let's create your first directory snapshot:

```bash
# Navigate to a directory you want to snapshot
cd C:\MyProject  # Windows
cd ~/myproject   # Linux/macOS

# Create a snapshot
dotnet run -- snapshot . my-first-snapshot.json
```

You should see output like:
```
📸 Creating snapshot for: C:\MyProject

✅ Snapshot written to: my-first-snapshot.json
   Files tracked : 42
   Files skipped : 0
   Total size    : 1.5 MB
```

## Your First Comparison

Now let's make some changes and compare:

```bash
# Make some changes to your files
# (edit, add, or delete some files)

# Create a second snapshot
dotnet run -- snapshot . my-second-snapshot.json

# Compare the two snapshots
dotnet run -- diff my-first-snapshot.json my-second-snapshot.json
```

You'll see a detailed report showing:
- ➕ Added files
- ➖ Removed files
- ✏️ Changed files

## Interactive Mode

For the best experience, try interactive mode:

```bash
dotnet run -- interactive
```

In interactive mode, you can:

```
FileChronicle> help
# See all available commands

FileChronicle> snapshot C:\MyProject snapshot.json
# Create snapshots interactively

FileChronicle> macros
# See saved macros

FileChronicle> exit
# Exit interactive mode
```

## Creating Your First Macro

Macros save you time by storing frequently used commands:

```bash
FileChronicle> macro save myproject "snapshot C:\MyProject snapshots\project_$timestamp.json --progress"
✅ Macro '@myproject' saved successfully.

FileChronicle> @myproject
→ Executing macro: snapshot C:\MyProject snapshots\project_20241215_143025.json --progress
📸 Creating snapshot for: C:\MyProject
...
```

## Common Workflows

### Daily Backup Snapshots

```bash
# Create a macro for daily backups
macro save daily "snapshot C:\Important backups\backup_$date.json --progress"

# Run it daily
@daily
```

### Code Review Preparation

```bash
# Before starting work
dotnet run -- snapshot . before-work.json --include *.cs

# After completing work
dotnet run -- snapshot . after-work.json --include *.cs

# Generate HTML report
dotnet run -- diff before-work.json after-work.json \
  --format html --output my-changes.html --detailed
```

### Monitoring Production

```bash
# Watch directory for changes
dotnet run -- watch C:\WebApp --interval 60
```

## Filtering Files

Exclude unwanted files from snapshots:

```bash
# Exclude build artifacts
dotnet run -- snapshot . output.json \
  --exclude bin\** \
  --exclude obj\** \
  --exclude *.user

# Include only specific files
dotnet run -- snapshot . output.json \
  --include *.cs \
  --include *.csproj
```

## Configuration

Customize FileChronicle to your needs:

```bash
# View current configuration
dotnet run -- config list

# Set default format to HTML
dotnet run -- config set defaultFormat html

# Set default watch interval
dotnet run -- config set defaultWatchInterval 10
```

## Tips for New Users

1. **Start Simple**: Begin with basic snapshots before using advanced features
2. **Use Progress**: Add `--progress` flag for large directories
3. **Test Patterns**: Test include/exclude patterns on small directories first
4. **Save Macros**: Create macros for commands you use frequently
5. **HTML Reports**: Use HTML format for reports you share with others

## Example: Complete Workflow

Here's a complete workflow from start to finish:

```bash
# 1. Enter interactive mode
dotnet run -- interactive

# 2. Set up variables
FileChronicle> set myproject C:\Projects\MyApp
FileChronicle> set backups C:\Backups

# 3. Create macro for snapshots
FileChronicle> macro save backup "snapshot $myproject $backups\backup_$timestamp.json --exclude bin\** --exclude obj\** --progress"

# 4. Run the backup
FileChronicle> @backup

# 5. Later, run another backup
FileChronicle> @backup

# 6. Compare the two latest backups
FileChronicle> diff C:\Backups\backup_20241215_100000.json C:\Backups\backup_20241215_150000.json --detailed
```

## Troubleshooting

### "Command not found"

Make sure you're using `dotnet run --` before commands when running from source:
```bash
dotnet run -- snapshot . output.json
```

### Pattern Not Matching

Use forward slashes or double backslashes:
```bash
--exclude bin/**
--exclude bin\\**
```

### Access Denied Errors

FileChronicle will show warnings but continue processing:
```
⚠ Access denied: C:\System\protected-file.sys
```

To exclude protected directories:
```bash
--exclude "System Volume Information/**"
```

## Next Steps

- Read the [Full Documentation](../README.md)
- Check out [Examples](EXAMPLES.md) for more use cases
- Learn about [Configuration](CONFIGURATION.md)
- View the [Quick Reference](QUICK_REFERENCE.md)

## Getting Help

- Run `dotnet run -- help` for command help
- Use `help` command in interactive mode
- Check the documentation in the `docs/` folder
- Open an issue on GitHub

## Quick Reference Card

```
Create Snapshot:    snapshot <dir> <output>
Compare Snapshots:  diff <old> <new>
Compare Dirs:       compare <dir1> <dir2>
Watch Directory:    watch <dir>
Interactive Mode:   interactive
Show Help:          help
```

Welcome to FileChronicle! Happy snapshotting! 📸
