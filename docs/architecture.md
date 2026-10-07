# Architecture Documentation

## System Overview

SteamTOTP is a **minimalist, single-purpose .NET console application** that generates Steam Guard TOTP (Time-based One-Time Password) codes from a Base32-encoded shared secret. It serves as a thin CLI wrapper around the `SteamGuard.TOTP` NuGet library.

### Purpose
Enable generation of Steam Guard Mobile Authenticator codes without a mobile device, suitable for scripting, automation, and headless environments.

### Scope
- **Owns**: CLI argument handling, stdout output, process exit codes
- **Does not own**: TOTP algorithm, Base32 decoding, HMAC-SHA1 computation, time-step calculation (delegated to SteamGuard.TOTP)

---

## Architectural Decomposition

```
┌─────────────────────────────────────────────────────────────┐
│                      SteamTOTP Process                       │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────┐    ┌──────────────────────────────────┐  │
│  │  Program.cs  │───▶│  SteamGuard.TOTP (NuGet)         │  │
│  │  (Entry)     │    │  ┌────────────────────────────┐  │  │
│  └──────────────┘    │  │ SteamGuard.GenerateAuthCode  │  │  │
│         │            │  │  - Base32 decode             │  │  │
│         ▼            │  │  - HMAC-SHA1                 │  │  │
│  ┌──────────────┐    │  │  - Time-step (UTC/30s)       │  │  │
│  │  Console     │    │  │  - Steam charset mapping     │  │  │
│  │  .WriteLine  │    │  └────────────────────────────┘  │  │
│  └──────────────┘    │              │                   │  │
│                      │              ▼                   │  │
│                      │  ┌────────────────────────────┐  │  │
│                      │  │ NuciExtensions 5.0.0       │  │  │
│                      │  │ (utility library)          │  │  │
│                      │  └────────────────────────────┘  │  │
│                      └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Location | Responsibility |
|-----------|----------|----------------|
| `Program.Main` | `SteamTOTP/Program.cs:7` | Entry point, argument access, output |
| `SteamGuard.GenerateAuthenticationCode` | `SteamGuard.TOTP` (external) | TOTP computation per RFC 6238 + Steam charset |
| `ITimeStepProvider` | `SteamGuard.TOTP` (external) | Time abstraction (default: `DateTimeOffset.UtcNow`) |

---

## Dependency Direction

```
SteamTOTP (exe)
    │
    ▼ (compile-time + runtime)
SteamGuard.TOTP 1.1.0 (NuGet)
    │
    ▼ (transitive)
NuciExtensions 5.0.0 (NuGet)
    │
    ▼ (transitive)
.NET 10.0 Runtime (System.Security.Cryptography, etc.)
```

- **No circular dependencies**
- **No internal abstractions** — direct dependency on concrete `SteamGuard` class
- **Two external dependencies** — SteamGuard.TOTP (direct), NuciExtensions (transitive)

---

## Runtime Topology

```
User Shell
    │
    ▼ argv[0] = shared_secret
SteamTOTP Process (single-threaded, synchronous)
    │
    ▼ stdout: 5-char code + newline
    ▼ stderr: unhandled exceptions only
    ▼ exit code: 0 (success), non-zero (unhandled exception)
```

### Process Characteristics
- **Single-threaded** — no concurrency
- **Stateless** — no persistent state between invocations
- **Deterministic** — pure function of (secret, UTC time)
- **Short-lived** — executes in <100ms typically
- **No configuration files** — all input via CLI argument

---

## Major State

| State | Location | Mutability | Persistence |
|-------|----------|------------|-------------|
| CLI argument (`args[0]`) | Stack (Main) | Immutable per invocation | None |
| Current UTC time | `DateTimeOffset.UtcNow` (via SteamGuard.TOTP) | Read-only | None |
| TOTP secret (decoded) | Heap (SteamGuard.TOTP) | Transient | None |

**No application-managed state** — all state is either input arguments or system time.

---

## Major Integrations

| Integration | Type | Direction | Protocol |
|-------------|------|-----------|----------|
| SteamGuard.TOTP | NuGet library | Outbound (library call) | In-process .NET call |
| Console stdout | OS stream | Outbound | Text (UTF-8) |
| Console stderr | OS stream | Outbound (errors only) | Text (UTF-8) |
| Process exit code | OS | Outbound | Integer |

---

## System-Wide Invariants

| Invariant | Enforcement | Violation Consequence |
|-----------|-------------|----------------------|
| Exactly one CLI argument required | None (crashes on missing) | `IndexOutOfRangeException`, exit code 139/1 |
| Secret must be valid Base32 | SteamGuard.TOTP (throws) | `FormatException`/`InvalidOperationException` |
| Output is exactly 5 chars + newline | SteamGuard.TOTP contract | N/A (library guarantee) |
| Code uses Steam charset `23456789BCDFGHJKMNPQRTVWXY` | SteamGuard.TOTP implementation | N/A (library guarantee) |
| Time step = 30 seconds (RFC 6238) | SteamGuard.TOTP implementation | N/A (library guarantee) |
| Single-file, trimmed publish | csproj: `PublishSingleFile`, `PublishTrimmed` | Larger binary, slower startup if disabled |

---

## Failure Modes

| Trigger | Exception Type | Exit Code | User Visibility |
|---------|----------------|-----------|-----------------|
| No arguments | `IndexOutOfRangeException` | 139 (SIGSEGV) / 1 | Unhandled exception stack trace |
| Empty string secret | `InvalidOperationException` | 1 | Unhandled exception stack trace |
| Invalid Base32 secret | `FormatException` | 1 | Unhandled exception stack trace |
| Null secret | `NullReferenceException` | 1 | Unhandled exception stack trace |
| Trimmed publish removes required type | `MissingMethodException` | 1 | Unhandled exception stack trace |

**No graceful error handling exists** — all failures surface as unhandled exceptions.

---

## Build & Deployment Architecture

### Build Pipeline
```
dotnet restore
    │
    ▼
dotnet build (Debug, net10.0)
    │
    ▼
dotnet publish -c Release (single-file, trimmed, self-contained)
    │
    ▼
bin/Release/net10.0/steam-totp (or steam-totp.exe)
```

### CI Pipeline (`.github/workflows/dotnet.yml`)
```
on: push/PR to master
    │
    ▼
ubuntu-latest runner
    │
    ├── actions/checkout@v2
    ├── actions/setup-dotnet@v1 (10.0.x)
    ├── dotnet restore
    ├── dotnet build --no-restore
    └── dotnet test --no-build --verbosity normal  (vacuous: no tests)
```

### Release Pipeline (`release.sh`)
```
bash release.sh <version>
    │
    ▼
wget https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh
    │
    ▼
bash /dev/stdin <version>
    │
    ▼ (external script)
    ├── Version bump
    ├── Changelog generation
    ├── Git tag
    ├── GitHub Release creation
    ├── Asset upload (published binaries)
    └── NuGet push (if applicable)
```

---

## Security Architecture

### Trust Boundaries
```
User Input (secret) ──▶ Process Memory ──▶ SteamGuard.TOTP ──▶ stdout
      │                    │                    │
      ▼                    ▼                    ▼
  Untrusted          Transient            Trusted library
  (CLI arg)          (decoded bytes)      (GPLv3, same author)
```

### Secrets Handling
- **Secret enters via CLI argument** — visible in process table (`ps aux`), shell history
- **Secret decoded in memory** — Base32 → byte[] → HMAC key
- **No secret logging** — library doesn't log; app doesn't log
- **No secret persistence** — purely transient
- **Trimmed binary** — reduces attack surface but doesn't eliminate memory exposure

### Supply Chain
- **SteamGuard.TOTP** — same author (hmlendea), GPLv3, on NuGet.org
- **deployment-scripts** — same author, external, executed via `wget | bash`
- **.NET SDK/Runtime** — Microsoft, official channels

---

## Extensibility Points

| Extension Point | Current State | Effort to Extend |
|-----------------|---------------|------------------|
| Additional CLI options | None (single positional arg) | Low (add argument parsing) |
| Multiple output formats | None (stdout only) | Low |
| Time abstraction for testing | In SteamGuard.TOTP (`ITimeStepProvider`) | Medium (adapter in SteamTOTP) |
| Structured logging | None | Low (add `Microsoft.Extensions.Logging`) |
| Configuration file | None | Medium |

---

## Modification Impact Analysis

| Change | Affected Components | Risk Level |
|--------|---------------------|------------|
| Add argument validation | `Program.cs` only | Low |
| Change output format | `Program.cs` only | Low |
| Upgrade .NET version | `csproj`, `dotnet.yml`, `release.sh` | Medium |
| Upgrade SteamGuard.TOTP | `csproj`, verify API compatibility | Low-Medium |
| Add tests | New `SteamTOTP.Tests` project | Medium |
| Add subcommands | `Program.cs`, possibly new files | Medium |
| Change publish mode | `csproj` only | Low |
| Pin GitHub Actions | `dotnet.yml` only | Low |
| Audit/replace release.sh | `release.sh` only | Medium |