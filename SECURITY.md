# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of FileChronicle seriously. If you believe you have found a security vulnerability, please report it to us as described below.

### Please Do Not

- **Do not** open a public GitHub issue for security vulnerabilities
- **Do not** disclose the vulnerability publicly until it has been addressed

### Please Do

1. **Email** the vulnerability details to [SECURITY_EMAIL_HERE]
2. **Include** as much information as possible:
   - Type of vulnerability
   - Full paths of source file(s) related to the vulnerability
   - Location of the affected source code (tag/branch/commit or direct URL)
   - Any special configuration required to reproduce the issue
   - Step-by-step instructions to reproduce the issue
   - Proof-of-concept or exploit code (if possible)
   - Impact of the issue, including how an attacker might exploit it

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your vulnerability report within 3 business days
- **Updates**: We will keep you informed about the progress of addressing the vulnerability
- **Fix Timeline**: We aim to address critical vulnerabilities within 30 days
- **Credit**: If you wish, we will credit you in the security advisory when the fix is released

## Security Best Practices for Users

When using FileChronicle:

1. **Be Cautious with Paths**: Verify directory paths before creating snapshots
2. **Review Configurations**: Check default exclude patterns in config
3. **Limit Permissions**: Run with least privilege necessary
4. **Verify Downloads**: Always download from official sources
5. **Keep Updated**: Use the latest version with security patches

## Known Security Considerations

### File Access
- FileChronicle requires read access to files and directories being snapshotted
- The tool will skip files it cannot access due to permission restrictions
- Error messages may reveal file paths in the console output

### Hash Computation
- SHA-256 hashes are computed for file integrity verification
- Hash computation can be skipped with `--no-hash` for faster operation
- Hashes are stored in plain text in snapshot files

### Snapshot Files
- Snapshot files contain file paths, sizes, and hashes
- Store snapshot files securely if they contain sensitive directory structures
- Be careful when sharing snapshot files as they reveal directory structure

### Configuration Files
- Configuration is stored in `%APPDATA%\FileChronicle\` on Windows
- Configuration files are stored in plain text
- Macro commands are stored unencrypted

### HTML Reports
- HTML reports are generated from snapshot data
- File paths in reports are HTML-encoded to prevent XSS
- Reports should be treated as sensitive if they contain internal directory structures

## Vulnerability Disclosure Policy

When we receive a security vulnerability report, we will:

1. Confirm the vulnerability and determine affected versions
2. Audit code to find any similar vulnerabilities
3. Prepare fixes for all supported versions
4. Release patches and publish a security advisory

## Security Updates

Security updates will be announced:

- In the GitHub Security Advisories
- In the CHANGELOG.md file
- In release notes on GitHub Releases

## Scope

This security policy applies to:

- The main FileChronicle application
- All official releases and pre-releases
- The official GitHub repository

This policy does not apply to:

- Third-party forks
- Modified versions
- Unofficial distributions

## Comments on This Policy

If you have suggestions on how this policy could be improved, please submit a pull request or open an issue.

---

Thank you for helping keep FileChronicle and its users safe!
