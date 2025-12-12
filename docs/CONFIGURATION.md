# Example Configuration Files

This directory contains example configuration files for FileChronicle.

## config.json

Default configuration file location:
- Windows: `%APPDATA%\FileChronicle\config.json`
- Linux/macOS: `~/.config/FileChronicle/config.json`

```json
{
  "DefaultFormat": "json",
  "DefaultExcludePatterns": [
    "*.tmp",
    "*.log",
    ".git/**",
    "bin/**",
    "obj/**",
    "node_modules/**",
    ".vs/**",
    "*.user",
    "*.suo"
  ],
  "DefaultWatchInterval": 5,
  "ColoredOutput": true
}
```

### Configuration Options

#### DefaultFormat
The default output format for snapshots and diffs.
- **Values**: `json`, `csv`, `html`
- **Default**: `json`

```bash
# Set with CLI
FileChronicle config set defaultFormat html
```

#### DefaultExcludePatterns
Array of file patterns to exclude by default from snapshots.
- **Type**: Array of strings
- **Pattern syntax**: Supports `*` (any characters), `**` (any directories), `?` (single character)

**Common patterns:**
- `*.tmp` - All temporary files
- `*.log` - All log files
- `.git/**` - Entire .git directory
- `bin/**` - Entire bin directory and subdirectories
- `node_modules/**` - Node.js dependencies
- `*.user` - User-specific files
- `*.cache` - Cache files

#### DefaultWatchInterval
Default interval in seconds for watch mode.
- **Type**: Integer
- **Default**: `5`
- **Range**: 1-3600 (1 second to 1 hour)

```bash
# Set with CLI
FileChronicle config set defaultWatchInterval 10
```

#### ColoredOutput
Enable or disable colored console output.
- **Type**: Boolean
- **Default**: `true`

```bash
# Disable colored output
FileChronicle config set coloredOutput false
```

## macros.json

Macro storage location:
- Windows: `%APPDATA%\FileChronicle\macros.json`
- Linux/macOS: `~/.config/FileChronicle/macros.json`

```json
{
  "Macros": {
    "daily": "snapshot C:\\Projects snapshots\\daily_$timestamp.json --progress",
    "compare-dev": "compare C:\\Dev\\old C:\\Dev\\new --detailed",
    "watch-src": "watch C:\\Projects\\src --interval 5 --include *.cs",
    "quick-snap": "snapshot $cwd output_$date.json --no-hash"
  }
}
```

### Creating Macros

**Via CLI:**
```bash
FileChronicle macro save daily "snapshot C:\Projects snapshots\daily_$timestamp.json"
```

**Via Interactive Mode:**
```bash
FileChronicle> macro save daily "snapshot C:\Projects snapshots\daily_$timestamp.json"
```

### Using Macros

**In Interactive Mode:**
```bash
FileChronicle> @daily
```

**List All Macros:**
```bash
FileChronicle macro list
# or in interactive mode
FileChronicle> macros
```

### Macro Variables

Macros can use context variables:

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `$cwd` | Current working directory | `C:\Projects` |
| `$timestamp` | Current timestamp | `20241215_143025` |
| `$date` | Current date | `20241215` |
| Custom | User-defined variables | Any value |

## Example Macro Use Cases

### 1. Daily Backups
```json
{
  "daily-backup": "snapshot C:\\ImportantData backups\\backup_$timestamp.json --progress"
}
```

Usage: `@daily-backup`

### 2. Project Comparison
```json
{
  "compare-releases": "compare C:\\Project\\v1.0 C:\\Project\\v2.0 --detailed --exclude *.log"
}
```

Usage: `@compare-releases`

### 3. Source Code Monitoring
```json
{
  "watch-code": "watch C:\\Source --include *.cs --include *.csproj --interval 3"
}
```

Usage: `@watch-code`

### 4. Quick Snapshots
```json
{
  "quick": "snapshot $cwd snapshot_$timestamp.json --no-hash",
  "quick-report": "diff snapshot1.json snapshot2.json --format html --output report_$date.html"
}
```

Usage: `@quick` then `@quick-report`

### 5. Filtered Snapshots
```json
{
  "code-only": "snapshot C:\\Project output.json --include *.cs --include *.csproj --exclude bin/** --exclude obj/**"
}
```

Usage: `@code-only`

## Environment-Specific Configurations

### Development Environment
```json
{
  "DefaultFormat": "json",
  "DefaultExcludePatterns": [
    "bin/**",
    "obj/**",
    ".vs/**",
    "*.user",
    "*.cache",
    "TestResults/**"
  ],
  "DefaultWatchInterval": 3,
  "ColoredOutput": true
}
```

### Production Monitoring
```json
{
  "DefaultFormat": "html",
  "DefaultExcludePatterns": [
    "*.tmp",
    "*.log",
    "cache/**"
  ],
  "DefaultWatchInterval": 300,
  "ColoredOutput": false
}
```

### CI/CD Pipeline
```json
{
  "DefaultFormat": "json",
  "DefaultExcludePatterns": [
    ".git/**",
    "node_modules/**",
    "coverage/**"
  ],
  "DefaultWatchInterval": 5,
  "ColoredOutput": false
}
```

## Tips

1. **Start Simple**: Begin with default configuration and adjust as needed
2. **Test Patterns**: Use `--include` and `--exclude` flags to test patterns before making them default
3. **Version Control**: Consider storing your configuration in version control for team consistency
4. **Backup Macros**: Export your macros.json file if you have many custom macros
5. **Use Variables**: Leverage `$timestamp` and `$date` in macros for automatic naming

## Troubleshooting

### Configuration Not Loading
1. Check file exists in correct location
2. Verify JSON syntax is valid
3. Check file permissions
4. Reset to defaults: `FileChronicle config reset`

### Macros Not Working
1. Verify macro syntax is correct
2. Check quotes in command strings
3. Test command manually before saving as macro
4. List macros to verify it's saved: `FileChronicle macro list`

### Patterns Not Matching
1. Use forward slashes or double backslashes
2. Test pattern matching manually
3. Check pattern is in correct list (include vs exclude)
4. Remember `**` matches any subdirectories

## Additional Resources

- [Main README](../README.md)
- [Contributing Guide](../CONTRIBUTING.md)
- [Changelog](../CHANGELOG.md)
