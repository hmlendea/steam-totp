# Security Policy

This document describes the security maintenance scope, vulnerability reporting process, and disclosure expectations for the SteamTOTP repository. SteamTOTP is a minimalist, single-file .NET console utility that generates Steam Guard TOTP codes from a shared secret.

## 📑 Table of Contents

- [Supported Versions](#-supported-versions)
- [Reporting a Vulnerability](#-reporting-a-vulnerability)
- [Scope](#-scope)
- [Disclosure Policy](#-disclosure-policy)
- [Safe Harbour](#-safe-harbour)
- [Recognition](#-recognition)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Latest version | Source (main branch) | ✅ |
| Preceding versions | Any distribution channel | ❌ |
| Unofficial third-party distribution channels | Any | ❌ |

Only the latest released version and the `main` branch receive security fixes. There is no long-term support (LTS) policy for preceding versions. Users should always use the latest release.

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/steam-totp/security/advisories)
- Contact the maintainers directly via GitHub issues (for non-sensitive reports) or the Security Advisories tab (for sensitive reports)

Include the following information when possible:
- Description of the vulnerability and its impact
- Steps to reproduce or proof-of-concept
- Affected version(s) or commit(s)
- Suggested mitigation or fix (if known)

## 📌 Scope

The subsequent report categories are in scope for this repository:
- **Code execution vulnerabilities** in `Program.cs` or the build/publish pipeline
- **Supply chain vulnerabilities** in `SteamGuard.TOTP`, `NuciExtensions`, or transitive dependencies
- **Dependency vulnerabilities** reported by `dotnet list package --vulnerable` or GitHub Dependabot
- **Trimmer-related vulnerabilities** where `PublishTrimmed=true` removes security-critical types
- **Release pipeline vulnerabilities** in `release.sh` or the external `deployment-scripts`
- **CI/CD vulnerabilities** in `.github/workflows/dotnet.yml` (unpinned actions, runner compromise)

The subsequent categories are out of scope unless explicitly stated to the contrary:
- **Steam protocol or Steam Guard algorithm vulnerabilities** — these are upstream Steam/Valve concerns
- **User secret management practices** — how users store, inject, or protect their shared secret (CLI argument visibility in `ps aux`, shell history, etc.)
- **OS-level process visibility** — the shared secret appears in the process command line; this is an OS behaviour, not an application vulnerability
- **Physical or local access attacks** — an attacker with local machine access can read process memory, shell history, or swap
- **Denial of service via resource exhaustion** — the application is single-invocation, stateless, and short-lived
- **Social engineering or phishing** — not an application vulnerability

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🧾 Safe Harbour

If your research is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.

## 🙏 Recognition

We appreciate responsible disclosure. Reporters who desire public attribution may be acknowledged in release notes, advisories, or a dedicated acknowledgements section.