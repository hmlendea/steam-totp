# Privacy and Personal Data

SteamTOTP is a single-purpose, stateless .NET console utility that generates Steam Guard TOTP codes from a user-provided shared secret. It performs no data collection, no network communication, no persistent storage, and no telemetry. The only personal data it processes is the shared secret passed as a command-line argument, which exists transiently in process memory during a single invocation.

**Information reviewed:** 2026-10-07

## 📑 Table of Contents

- [What This Document Covers](#-what-this-document-covers)
- [Self-Hosted Deployments](#-self-hosted-deployments)
- [Data We Handle](#-data-we-handle)
- [Processing and Use](#-processing-and-use)
- [Storage, Retention, and Deletion](#-storage-retention-and-deletion)
- [External Processing and Integrations](#-external-processing-and-integrations)
- [Data Protection and Security](#-data-protection-and-security)
- [Document Changes](#-document-changes)
- [Contact](#-contact)

## 🔎 What This Document Covers

This document describes how SteamTOTP at https://github.com/hmlendea/steam-totp handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

SteamTOTP is distributed as a self-contained, single-file executable that users download and run locally. There is no server component, no central service, and no project-maintained infrastructure that receives data from user deployments.

- **Instance operator responsibilities:** The operator (the person running the binary) controls all local execution context, including shell history, process table visibility, log aggregation, backup policies, and access controls on the machine where the binary runs.
- **Data sent to project maintainers:** None. The application makes no network requests, performs no update checks, sends no telemetry, and transmits no crash reports to the project maintainers or any external service.
- **Optional flows:** There are no optional data flows to disable.

## 📥 Data We Handle

### Data Provided to the Application

- **Steam Guard shared secret (Base32-encoded string):** Provided by the user as the first command-line argument (`argv[1]`). This secret is used to generate the TOTP code. The application does not request, prompt for, or store this secret beyond the duration of the single invocation.

### Data Generated or Collected by the Application

- **No personal data is generated or collected automatically.** The application produces a 5-character TOTP code on stdout, which is derived from the shared secret and the current UTC time. This output is not personal data; it is a one-time authentication code.

### Data Received from Integrations

- **No personal data is received from integrations or third parties.** The application has no integrations, no network clients, and no configuration that would cause it to receive data from external sources.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- **TOTP code generation** — Steam Guard shared secret (Base32 string) and current UTC time step

## 🗄️ Storage, Retention, and Deletion

- **Process memory only:** The shared secret exists transiently as a `string` (CLI argument) and a decoded `byte[]` (HMAC key) in the process heap during the single invocation. The TOTP code exists as a `string` briefly before being written to stdout.
- **No persistent storage:** The application writes no files, creates no databases, uses no caches, and maintains no state between invocations.
- **No backups:** The application has no backup mechanism.
- **Deletion:** All in-memory data is released when the process exits (typically within <100ms).
- **Operator control:** For self-hosted deployments, the instance operator controls the execution environment, including shell history (which may retain the CLI argument), process accounting logs, and any external logging or monitoring infrastructure they have configured.

## 🔗 External Processing and Integrations

The application has no built-in external data transfer. It makes no network requests, contacts no APIs, and integrates with no external services at runtime.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| (none) | — | — | — |

**Build-time and release-time dependencies** (not runtime):
- NuGet.org (package restore for `SteamGuard.TOTP` and `NuciExtensions`)
- GitHub Actions (CI execution on `ubuntu-latest` runners)
- GitHub Releases (artifact hosting for published binaries)
- External release script (`deployment-scripts` from `hmlendea/deployment-scripts`)

These are development and release infrastructure, not runtime data flows.

## 🛡️ Data Protection and Security

- **No secrets in logs:** The application performs no logging. The shared secret is never written to stdout, stderr, or any log file by the application code.
- **Memory handling:** The shared secret is decoded to a `byte[]` for HMAC-SHA1 computation. This memory is managed by the .NET runtime and released on process exit. No explicit zeroing is performed.
- **Process visibility:** The shared secret is visible in the process command line (`ps aux`, `/proc/<pid>/cmdline`) and shell history during invocation. Users should be aware of this OS-level exposure.
- **Trimmed single-file binary:** The published binary is trimmed and self-contained (~14 MB on linux-arm64), reducing the attack surface by excluding unused framework code.
- **No cryptographic agility:** The algorithm (HMAC-SHA1, 5-digit, 30-second step, Steam charset) is fixed by the `SteamGuard.TOTP` library.
- **Operator responsibilities (self-hosted):** Keep the binary and execution environment updated, restrict shell history access if secrets are passed via CLI, use secure secret injection methods (e.g., environment variables with argument substitution, secret managers) instead of raw CLI arguments where possible.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at `PRIVACY.md` in the repository root.

## 📬 Contact

For questions about application data handling, contact the project maintainers via GitHub issues at https://github.com/hmlendea/steam-totp/issues. For a self-hosted instance, contact the instance operator (the person running the binary on their machine). Do not send passwords, access tokens, shared secrets, or other secrets in any communication.