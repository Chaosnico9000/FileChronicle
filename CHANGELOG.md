# FileChronicle Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-15

### Added
- Initial release of FileChronicle
- Directory snapshot creation with SHA-256 hashing
- Snapshot comparison and diff generation
- Direct directory comparison
- Real-time directory monitoring (watch mode)
- Interactive mode with REPL interface
- Macro system for command shortcuts
- Context variables for dynamic values
- Multiple export formats (JSON, CSV, HTML)
- Pattern matching with wildcards for include/exclude
- Configurable settings with persistent storage
- Progress reporting for long operations
- Colored console output
- Cancellation support with Ctrl+C
- Async/await throughout for performance
- Cross-platform path normalization
- Detailed and summary view modes
- Default exclude patterns for common files
- Built-in variables: $cwd, $timestamp, $date

### Features

#### Snapshot Command
- Create detailed directory snapshots
- Optional SHA-256 file hashing
- Include/exclude pattern matching
- Progress reporting
- JSON and CSV output formats

#### Diff Command
- Compare two snapshots
- Identify added, removed, and modified files
- Export to JSON, CSV, or HTML
- Detailed file information display
- Size change calculations
- Optional unchanged files display

#### Compare Command
- Direct directory comparison
- No intermediate snapshot files needed
- Same filtering options as snapshot
- Real-time comparison results

#### Watch Command
- Monitor directories for changes
- Configurable check intervals
- Real-time change notifications
- Pattern-based filtering

#### Interactive Mode
- REPL-style command interface
- Command history
- Macro execution with @name syntax
- Context variable support
- Built-in help system
- Screen clearing

#### Macro System
- Save frequently used commands
- List all saved macros
- Execute macros by name
- Delete unwanted macros
- Persistent storage between sessions

#### Configuration
- Configurable default format
- Configurable watch interval
- Configurable colored output
- Default exclude patterns
- Persistent configuration file
- Easy reset to defaults

### Technical Details
- Built with .NET 10
- C# 14.0 language features
- Fully asynchronous I/O operations
- Memory-efficient file streaming
- 80KB buffer for hash computation
- Case-insensitive path comparison
- UTF-8 console encoding
- Cross-platform compatibility

### Performance
- Async file operations for better performance
- Streaming hash computation for large files
- Efficient pattern matching with regex
- Optional hash skipping for speed
- Progress reporting for user feedback

### Security
- SHA-256 cryptographic hashing
- File access error handling
- Unauthorized access protection
- Safe path handling

## [Unreleased]

### Planned Features
- Archive comparison (ZIP, TAR)
- Database snapshot storage
- Network directory support
- Scheduled snapshots
- Email notifications
- Diff filtering by file type
- Graphical user interface
- Plugin system
- Custom hash algorithms
- Compression for snapshots
- Incremental snapshots
- Snapshot cleanup/pruning
- Backup/restore functionality
- Integration with version control systems

---

## Release Notes Format

### Added
New features and capabilities

### Changed
Changes to existing functionality

### Deprecated
Features that will be removed in future versions

### Removed
Features that have been removed

### Fixed
Bug fixes

### Security
Security-related improvements
