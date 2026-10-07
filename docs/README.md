# Documentation Index

This directory contains comprehensive technical documentation for the SteamTOTP repository, generated through deep repository analysis.

## Documents

| Document | Purpose | Audience |
|----------|---------|----------|
| [architecture.md](architecture.md) | System architecture, components, invariants, failure modes | Architects, maintainers |
| [capabilities.md](capabilities.md) | Detailed capability specification with execution flows | Developers, integrators |
| [implementation.md](implementation.md) | Source code details, call graphs, build artifacts, CI/CD | Developers, contributors |
| [dependencies.md](dependencies.md) | Dependency graph, versions, licenses, vulnerabilities | Maintainers, security auditors |
| [testing.md](testing.md) | Test strategy, recommended cases, CI integration, gaps | Developers, QA |

---

## Repository Summary

### What Is SteamTOTP?
A **single-file, trimmed, self-contained .NET 10 console utility** that generates Steam Guard TOTP codes from a shared secret.

### Key Characteristics
- **Lines of code**: ~10 (Program.cs only)
- **Dependencies**: 1 direct NuGet package (SteamGuard.TOTP 1.1.0), 1 transitive (NuciExtensions 5.0.0)
- **Target Framework**: net10.0
- **License**: GPLv3
- **Tests**: None
- **CI**: GitHub Actions (build + vacuous test)

### Architecture at a Glance
```
CLI Argument → Program.Main → SteamGuard.TOTP.GenerateAuthenticationCode → stdout
```

### Critical Implementation Details
1. **No argument validation** — crashes with `IndexOutOfRangeException` if no secret provided
2. **No error handling** — all exceptions propagate to runtime
3. **Stateless, deterministic** — pure function of (secret, UTC time)
4. **Single-file publish** — trimmed, ~3-5 MB native binary
5. **External release script** — downloads and executes remote bash script

---

## Quick Reference

### Build & Run
```bash
dotnet build                    # Debug build
dotnet run -- <secret>          # Run with secret
dotnet publish -c Release       # Release binary (single-file, trimmed)
```

### Key Files
| File | Purpose |
|------|---------|
| `SteamTOTP/Program.cs` | Entry point, only source file |
| `SteamTOTP/SteamTOTP.csproj` | Project config, dependencies |
| `SteamTOTP.slnx` | Solution file |
| `.github/workflows/dotnet.yml` | CI pipeline |
| `release.sh` | Release automation (external script) |

### Common Tasks
| Task | Command |
|------|---------|
| Add argument validation | Edit `Program.cs` |
| Update .NET version | Edit `csproj`, `dotnet.yml`, `release.sh` |
| Upgrade SteamGuard.TOTP | `dotnet add package SteamGuard.TOTP --version <v>` |
| Add tests | Create `SteamTOTP.Tests` project (see testing.md) |
| Run CI locally | `act` (GitHub Actions local runner) |

---

## Known Issues & Technical Debt

| Issue | Severity | Location | Recommendation |
|-------|----------|----------|----------------|
| No argument validation | High | `Program.cs:7` | Add bounds check, friendly error |
| No tests | High | (missing) | Implement test strategy from testing.md |
| Unpinned GitHub Actions | Medium | `dotnet.yml` | Pin to SHA (e.g., `actions/checkout@11bd719...`) |
| Remote release script | Medium | `release.sh` | Audit, pin commit, verify checksum |
| Unknown SteamGuard.TOTP license | Medium | `csproj` | Verify GPLv3 compatibility |
| Deprecated CI actions | Low | `dotnet.yml` | Upgrade to v4 actions |
| No time abstraction | Low | (in SteamGuard.TOTP) | Adapter pattern for testability |

---

## For Future Agents

### To Understand This Repository Quickly
1. Read `architecture.md` for system overview
2. Read `capabilities.md` for the single capability detail
3. Read `implementation.md` for code-level details
4. Check `dependencies.md` for supply chain risks
5. Review `testing.md` for test gaps and strategy

### To Make Changes Safely
1. **Always** add tests first (see testing.md)
2. **Validate** SteamGuard.TOTP license before dependency updates
3. **Pin** GitHub Actions to SHAs
4. **Audit** release.sh external script before running
5. **Test** with known secrets at known times

### To Extend Functionality
- Add argument parsing library (e.g., `System.CommandLine`)
- Add time abstraction for testability
- Add structured logging (optional)
- Consider subcommands for future features

---

## Documentation Maintenance

### When to Update
- After any behavioral change
- After dependency updates
- After CI/CD changes
- After architectural decisions
- Quarterly review for accuracy

### Update Process
1. Modify relevant `.md` file(s)
2. Verify against implementation
3. Update this index if structure changes
4. Commit with descriptive message

---

## Generation Metadata

- **Generated**: 2026-10-07
- **Method**: Deep repository analysis with causal reasoning
- **Scope**: Complete repository (all source, config, CI, scripts)
- **Verification**: Cross-referenced implementation, config, CI, and external dependencies