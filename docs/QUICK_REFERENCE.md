# FileChronicle Quick Reference

## Commands At a Glance

| Command | Purpose | Example |
|---------|---------|---------|
| `snapshot` | Create directory snapshot | `FileChronicle snapshot C:\Project output.json` |
| `diff` | Compare two snapshots | `FileChronicle diff old.json new.json` |
| `compare` | Compare two directories | `FileChronicle compare dir1 dir2` |
| `watch` | Monitor directory | `FileChronicle watch C:\Project` |
| `interactive` | Launch interactive mode | `FileChronicle interactive` |
| `macro` | Manage macros | `FileChronicle macro list` |
| `config` | Manage configuration | `FileChronicle config list` |
| `help` | Show help | `FileChronicle help` |

## Common Options

### Snapshot Options
```
--include <pattern>    Include only matching files
--exclude <pattern>    Exclude matching files
--format <type>        Output format (json, csv)
--no-hash             Skip hash computation
--progress            Show progress
```

### Diff Options
```
--output <file>       Export diff to file
--format <type>       Output format (json, csv, html)
--show-unchanged      Include unchanged files
--detailed            Show detailed information
```

### Watch Options
```
--interval <seconds>  Check interval (default: 5)
--include <pattern>   Include patterns
--exclude <pattern>   Exclude patterns
```

## Pattern Syntax

| Pattern | Matches | Example |
|---------|---------|---------|
| `*.ext` | Files with extension | `*.cs`, `*.txt` |
| `**/*.ext` | Files in subdirs | `**/*.log` |
| `dir/**` | Entire directory | `bin/**`, `obj/**` |
| `file?.ext` | Single character | `file1.txt`, `file2.txt` |

## Interactive Mode Commands

```
@macroname        Execute macro
macros            List all macros
context           Show variables
set var value     Set variable
clear / cls       Clear screen
help              Show help
exit / quit       Exit
```

## Built-in Variables

```
$cwd              Current working directory
$timestamp        Current timestamp (yyyyMMdd_HHmmss)
$date             Current date (yyyyMMdd)
```

## Configuration Keys

```
defaultFormat             Output format (json, csv, html)
defaultWatchInterval      Watch interval in seconds
coloredOutput             Enable colored output (true/false)
```

Set with: `FileChronicle config set <key> <value>`

## Macro Commands

```
macro save <name> "<command>"    Create macro
macro list                       List all macros
macro run <name>                 Execute macro
macro delete <name>              Delete macro
```

## Quick Examples

### Basic Snapshot
```bash
FileChronicle snapshot C:\Project output.json
```

### Filtered Snapshot
```bash
FileChronicle snapshot C:\Project output.json \
  --include *.cs --exclude bin\**
```

### Compare Snapshots
```bash
FileChronicle diff old.json new.json --detailed
```

### HTML Report
```bash
FileChronicle diff old.json new.json \
  --format html --output report.html
```

### Watch Directory
```bash
FileChronicle watch C:\Project --interval 10
```

### Create and Use Macro
```bash
# Create
FileChronicle macro save mysnap "snapshot C:\Project output.json"

# Use (in interactive mode)
FileChronicle> @mysnap
```

## Common Patterns

### Development
```bash
--exclude bin\** --exclude obj\** --exclude *.user
```

### Web Projects
```bash
--exclude node_modules\** --exclude dist\** --exclude .git\**
```

### Backups
```bash
--exclude *.tmp --exclude *.log --exclude cache\**
```

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Error or help displayed |

## File Locations

### Windows
```
Config: %APPDATA%\FileChronicle\config.json
Macros: %APPDATA%\FileChronicle\macros.json
```

### Linux/macOS
```
Config: ~/.config/FileChronicle/config.json
Macros: ~/.config/FileChronicle/macros.json
```

## Tips

💡 Use `--progress` for large directories  
💡 Test patterns before saving as defaults  
💡 Save frequent commands as macros  
💡 Use `$timestamp` for automatic file naming  
💡 Generate HTML reports for sharing  
💡 Use `--no-hash` for faster snapshots  

## Getting Help

```bash
FileChronicle help                    # General help
FileChronicle snapshot --help         # Command help (if implemented)
FileChronicle interactive             # Interactive mode with help
```

## Resources

- [Full Documentation](README.md)
- [Examples](docs/EXAMPLES.md)
- [Configuration Guide](docs/CONFIGURATION.md)
- [Contributing](CONTRIBUTING.md)
