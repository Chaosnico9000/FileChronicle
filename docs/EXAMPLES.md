# FileChronicle Examples

This document provides practical examples for using FileChronicle in various scenarios.

## Table of Contents

- [Basic Examples](#basic-examples)
- [Development Workflows](#development-workflows)
- [Backup & Archival](#backup--archival)
- [CI/CD Integration](#cicd-integration)
- [Advanced Scenarios](#advanced-scenarios)

## Basic Examples

### 1. Simple Directory Snapshot

```bash
# Create a snapshot of your project
FileChronicle snapshot C:\MyProject project-snapshot.json
```

### 2. Compare Two Directories

```bash
# Compare two versions of a project
FileChronicle compare C:\Project\v1.0 C:\Project\v2.0 --detailed
```

### 3. Monitor Directory for Changes

```bash
# Watch a directory every 10 seconds
FileChronicle watch C:\MyProject --interval 10
```

### 4. Generate HTML Report

```bash
# Create two snapshots and compare them
FileChronicle snapshot C:\Project before.json
# ... make changes ...
FileChronicle snapshot C:\Project after.json
FileChronicle diff before.json after.json --format html --output changes.html
```

## Development Workflows

### Code Review Preparation

Track changes made during development:

```bash
# Before starting work
FileChronicle snapshot C:\Source before-changes.json --include *.cs --include *.csproj

# After completing work
FileChronicle snapshot C:\Source after-changes.json --include *.cs --include *.csproj

# Generate detailed report
FileChronicle diff before-changes.json after-changes.json \
  --detailed --format html --output code-changes.html
```

### Tracking Build Outputs

Monitor what gets generated during builds:

```bash
# Interactive mode with macro
FileChronicle interactive

FileChronicle> set builddir C:\Project\bin\Release
FileChronicle> macro save prebuild "snapshot $builddir prebuild_$timestamp.json"
FileChronicle> macro save postbuild "snapshot $builddir postbuild_$timestamp.json"

FileChronicle> @prebuild
# Run your build
FileChronicle> @postbuild
```

### Dependency Tracking

Track changes in dependencies:

```bash
FileChronicle snapshot C:\Project packages-before.json \
  --include packages\** --include *.csproj --include packages.config

# Update packages

FileChronicle snapshot C:\Project packages-after.json \
  --include packages\** --include *.csproj --include packages.config

FileChronicle diff packages-before.json packages-after.json --detailed
```

## Backup & Archival

### Daily Automated Backups

**PowerShell Script (backup.ps1):**
```powershell
$date = Get-Date -Format "yyyyMMdd"
$backupDir = "D:\Backups"
$sourceDir = "C:\ImportantData"

# Ensure backup directory exists
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

# Create snapshot
& FileChronicle snapshot $sourceDir "$backupDir\snapshot_$date.json" --progress

# Compare with yesterday if exists
$yesterday = (Get-Date).AddDays(-1).ToString("yyyyMMdd")
$yesterdaySnapshot = "$backupDir\snapshot_$yesterday.json"

if (Test-Path $yesterdaySnapshot) {
    & FileChronicle diff $yesterdaySnapshot "$backupDir\snapshot_$date.json" `
        --format html --output "$backupDir\changes_$date.html"
}
```

**Schedule with Task Scheduler:**
```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-File C:\Scripts\backup.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -Action $action -Trigger $trigger `
    -TaskName "FileChronicle Daily Backup" -Description "Daily backup snapshot"
```

### Weekly Comparison Reports

**Bash Script (weekly-report.sh):**
```bash
#!/bin/bash

# Configuration
BACKUP_DIR="/backups/project"
REPORT_DIR="/reports"
TODAY=$(date +%Y%m%d)
LAST_WEEK=$(date -d '7 days ago' +%Y%m%d)

# Generate report
FileChronicle diff \
    "$BACKUP_DIR/snapshot_$LAST_WEEK.json" \
    "$BACKUP_DIR/snapshot_$TODAY.json" \
    --format html \
    --output "$REPORT_DIR/weekly_changes_$TODAY.html" \
    --detailed

echo "Weekly report generated: $REPORT_DIR/weekly_changes_$TODAY.html"
```

### Incremental Backup Validation

```bash
# Create baseline
FileChronicle snapshot /data/important baseline.json

# Daily validation
FileChronicle snapshot /data/important daily_check.json
FileChronicle diff baseline.json daily_check.json --detailed

# Update baseline monthly
mv daily_check.json baseline.json
```

## CI/CD Integration

### Pre-Deployment Verification

**Azure DevOps Pipeline (azure-pipelines.yml):**
```yaml
steps:
- task: DotNetCoreCLI@2
  displayName: 'Install FileChronicle'
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'install --global FileChronicle'

- task: PowerShell@2
  displayName: 'Snapshot Before Deployment'
  inputs:
    targetType: 'inline'
    script: |
      FileChronicle snapshot $(Build.ArtifactStagingDirectory) before-deploy.json

- task: PowerShell@2
  displayName: 'Deploy Application'
  inputs:
    filePath: 'deploy.ps1'

- task: PowerShell@2
  displayName: 'Snapshot After Deployment'
  inputs:
    targetType: 'inline'
    script: |
      FileChronicle snapshot $(Build.ArtifactStagingDirectory) after-deploy.json
      FileChronicle diff before-deploy.json after-deploy.json \
        --format html --output deployment-report.html

- task: PublishBuildArtifacts@1
  displayName: 'Publish Deployment Report'
  inputs:
    PathtoPublish: 'deployment-report.html'
    ArtifactName: 'deployment-reports'
```

### GitHub Actions Workflow

**.github/workflows/snapshot-check.yml:**
```yaml
name: Directory Snapshot Check

on:
  pull_request:
    branches: [ main ]

jobs:
  snapshot-check:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
      with:
        fetch-depth: 0
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Install FileChronicle
      run: dotnet tool install --global FileChronicle
    
    - name: Checkout base branch
      run: git checkout ${{ github.base_ref }}
    
    - name: Snapshot base
      run: FileChronicle snapshot ./src base-snapshot.json --exclude "**/*.user"
    
    - name: Checkout PR branch
      run: git checkout ${{ github.head_ref }}
    
    - name: Snapshot PR
      run: FileChronicle snapshot ./src pr-snapshot.json --exclude "**/*.user"
    
    - name: Generate diff report
      run: |
        FileChronicle diff base-snapshot.json pr-snapshot.json \
          --format html --output pr-changes.html --detailed
    
    - name: Upload report
      uses: actions/upload-artifact@v3
      with:
        name: pr-changes-report
        path: pr-changes.html
```

### Docker Build Verification

```bash
# Before building image
FileChronicle snapshot ./app before-build.json \
  --exclude node_modules/** --exclude .git/**

# Build Docker image
docker build -t myapp:latest .

# Extract from image
docker create --name temp myapp:latest
docker cp temp:/app ./app-from-image
docker rm temp

# Snapshot extracted files
FileChronicle snapshot ./app-from-image after-build.json

# Compare
FileChronicle diff before-build.json after-build.json \
  --format html --output docker-changes.html --detailed
```

## Advanced Scenarios

### Multi-Environment Comparison

```bash
# Interactive mode with environment variables
FileChronicle interactive

FileChronicle> set dev C:\Environments\Dev
FileChronicle> set staging C:\Environments\Staging
FileChronicle> set prod C:\Environments\Production

FileChronicle> snapshot $dev dev-snapshot.json --exclude *.log --exclude *.tmp
FileChronicle> snapshot $staging staging-snapshot.json --exclude *.log --exclude *.tmp
FileChronicle> snapshot $prod prod-snapshot.json --exclude *.log --exclude *.tmp

FileChronicle> diff dev-snapshot.json staging-snapshot.json \
    --format html --output dev-vs-staging.html --detailed
FileChronicle> diff staging-snapshot.json prod-snapshot.json \
    --format html --output staging-vs-prod.html --detailed
```

### Large Directory Optimization

```bash
# Skip hashing for very large directories
FileChronicle snapshot /large/directory snapshot.json \
  --no-hash --progress

# Focus on specific file types
FileChronicle snapshot /large/directory snapshot.json \
  --include *.dll --include *.exe --progress

# Exclude common large directories
FileChronicle snapshot C:\Project snapshot.json \
  --exclude node_modules/** \
  --exclude .git/** \
  --exclude packages/** \
  --exclude obj/** \
  --exclude bin/** \
  --progress
```

### Security Audit Trail

```bash
# Create baseline security snapshot
FileChronicle snapshot C:\SecureApp security-baseline.json \
  --include *.dll --include *.exe --include *.config

# Schedule daily audits
# audit.sh
#!/bin/bash
DATE=$(date +%Y%m%d)
FileChronicle snapshot /opt/secureapp "audits/audit_$DATE.json" \
  --include "*.dll" --include "*.exe" --include "*.config"

# Compare with baseline
FileChronicle diff security-baseline.json "audits/audit_$DATE.json" \
  --format html --output "audits/security-audit_$DATE.html" --detailed

# Alert on changes
if [ $(grep -c "Changed Files" "audits/security-audit_$DATE.html") -gt 0 ]; then
    echo "Security audit detected changes!" | mail -s "Security Alert" admin@example.com
fi
```

### Configuration Management

Track configuration changes across servers:

```bash
# Snapshot configurations from multiple servers
# config-monitor.ps1
$servers = @("server1", "server2", "server3")
$configPath = "C:\ProgramData\MyApp\config"
$backupBase = "\\fileserver\config-backups"

foreach ($server in $servers) {
    $remotePath = "\\$server\$($configPath -replace ':','')"
    $outputFile = "$backupBase\$server-config_$(Get-Date -f yyyyMMdd).json"
    
    FileChronicle snapshot $remotePath $outputFile
    
    # Compare with yesterday
    $yesterday = (Get-Date).AddDays(-1).ToString("yyyyMMdd")
    $yesterdayFile = "$backupBase\$server-config_$yesterday.json"
    
    if (Test-Path $yesterdayFile) {
        $reportFile = "$backupBase\$server-changes_$(Get-Date -f yyyyMMdd).html"
        FileChronicle diff $yesterdayFile $outputFile `
            --format html --output $reportFile --detailed
    }
}
```

### Release Verification

```bash
# Macro setup for releases
FileChronicle macro save release-prep "snapshot C:\Release\Staging staging-$timestamp.json --progress"
FileChronicle macro save release-prod "snapshot C:\Release\Production prod-$timestamp.json --progress"
FileChronicle macro save release-verify "diff staging-$timestamp.json prod-$timestamp.json --format html --output release-verification.html --detailed"

# Usage in release process
FileChronicle> @release-prep
# Deploy to staging
FileChronicle> @release-prod
# Deploy to production
FileChronicle> @release-verify
# Review release-verification.html
```

## Tips and Best Practices

1. **Use Progress Flag**: For large directories, always use `--progress` to see status
2. **Exclude Build Artifacts**: Always exclude `bin/**`, `obj/**`, `node_modules/**`
3. **Consistent Naming**: Use timestamp in filenames: `snapshot_$timestamp.json`
4. **Regular Baselines**: Update baseline snapshots monthly or after major changes
5. **Automate Reports**: Generate HTML reports for easy sharing
6. **Macro Everything**: Save frequent operations as macros
7. **Version Control**: Store important snapshots in version control
8. **Test Patterns**: Test include/exclude patterns before committing

## Troubleshooting Examples

### Fixing Pattern Issues

```bash
# Problem: Pattern not matching
FileChronicle snapshot C:\Project test.json --include *.cs --progress

# Solution: Check path separators
FileChronicle snapshot C:\Project test.json --include **/*.cs --progress
```

### Handling Permissions

```bash
# Skip permission errors gracefully
FileChronicle snapshot C:\Protected output.json --progress
# Will show warnings but continue

# Exclude protected directories explicitly
FileChronicle snapshot C:\System output.json \
  --exclude "System Volume Information/**" \
  --exclude "$RECYCLE.BIN/**"
```

## More Resources

- [Configuration Guide](CONFIGURATION.md)
- [Main README](../README.md)
- [Contributing Guide](../CONTRIBUTING.md)
