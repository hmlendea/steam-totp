# Dependencies Documentation

## Direct Dependencies

### Runtime Dependencies

| Dependency | Type | Version | Source | Purpose |
|------------|------|---------|--------|---------|
| .NET Runtime | Framework | 10.0 | Microsoft | Execution platform |
| SteamGuard.TOTP | NuGet Package | 1.1.0 | NuGet.org | TOTP algorithm implementation |
| NuciExtensions | NuGet Package | 5.0.0 | NuGet.org | Utility library (transitive via SteamGuard.TOTP) |

### Build Dependencies

| Dependency | Type | Version | Source | Purpose |
|------------|------|---------|--------|---------|
| .NET SDK | SDK | 10.0.x | Microsoft | Compilation, publish, test |
| NuGet Client | Tool | Built-in | .NET SDK | Package restore |

### CI Dependencies

| Dependency | Type | Version | Source | Purpose |
|------------|------|---------|--------|---------|
| GitHub Actions Runner | Service | ubuntu-latest | GitHub | CI execution environment |
| actions/checkout | Action | v2 | GitHub | Repository checkout |
| actions/setup-dotnet | Action | v1 | GitHub | .NET SDK installation |

### Release Dependencies

| Dependency | Type | Version | Source | Purpose |
|------------|------|---------|--------|---------|
| deployment-scripts | External Script | 10.0.sh | hmlendea/deployment-scripts | Release automation |

---

## Dependency Graph

```
SteamTOTP Application
│
├── Compile-time
│   └── SteamGuard.TOTP v1.1.0 (NuGet)
│       └── NuciExtensions v5.0.0 (NuGet)
│
├── Runtime
│   └── .NET 10.0 Runtime
│       └── (system libraries)
│
├── Build
│   └── .NET 10.0 SDK
│       ├── Roslyn Compiler
│       ├── MSBuild
│       ├── NuGet Client
│       └── ILLink Trimmer (for PublishTrimmed)
│
├── CI
│   ├── GitHub Actions (ubuntu-latest)
│   ├── actions/checkout@v2
│   └── actions/setup-dotnet@v1
│
└── Release
    └── deployment-scripts/dotnet/10.0.sh (external)
        └── (unknown transitive dependencies)
```

---

## SteamGuard.TOTP Analysis

### Package Metadata
- **Id**: `SteamGuard.TOTP`
- **Version**: `1.1.0`
- **Authors**: Not specified in csproj
- **License**: Not specified in csproj
- **Project URL**: Not specified in csproj
- **Repository**: Not specified in csproj

### Target Frameworks (from NuGet)
Likely supports: `netstandard2.0`, `net6.0`, `net8.0`, `net10.0`

### Public API Surface
```csharp
namespace SteamGuard.TOTP
{
    public class SteamGuard
    {
        public SteamGuard();                          // Default constructor
        public string GenerateAuthenticationCode(string sharedSecret);
    }
}
```

### Transitive Dependencies
| Dependency | Version | Source | Purpose |
|------------|---------|--------|---------|
| NuciExtensions | 5.0.0 | NuGet.org | Utility library (required by SteamGuard.TOTP) |

SteamGuard.TOTP depends on NuciExtensions 5.0.0 (targeting net9.0). This is a utility library by the same author (hmlendea).

### Version Compatibility
| SteamTOTP Target | SteamGuard.TOTP Version | Status |
|------------------|-------------------------|--------|
| net10.0 | 1.1.0 | Compatible (assumed) |

### Update Considerations
- Check for breaking changes in `GenerateAuthenticationCode` signature
- Verify algorithm parameters unchanged (5 digits, SHA1, 30s)
- Test with known shared secrets

---

## .NET Version Alignment

| Component | Version | Constraint |
|-----------|---------|------------|
| TargetFramework | net10.0 | `SteamTOTP.csproj` |
| CI Setup | 10.0.x | `.github/workflows/dotnet.yml` |
| Release Script | 10.0 | `release.sh` |
| Runtime Required | 10.0 | For published binary |

**All aligned to .NET 10.0.**

---

## Vulnerability Surface

### Direct Dependencies
- `SteamGuard.TOTP 1.1.0` — No known CVEs (as of analysis date)
- .NET 10.0 — Supported by Microsoft, regular security updates

### Transitive Dependencies
- None from SteamGuard.TOTP
- .NET Runtime includes system libraries (monitor via `dotnet list package --vulnerable`)

### Supply Chain Risks
| Risk | Location | Mitigation |
|------|----------|------------|
| Unpinned GitHub Actions | `actions/checkout@v2`, `actions/setup-dotnet@v1` | Pin to SHA |
| Remote release script | `release.sh` → `deployment-scripts` | Audit, pin commit, verify checksum |
| NuGet package | `SteamGuard.TOTP` | Lock version, monitor advisories |

---

## Dependency Management Commands

```bash
# List direct dependencies
dotnet list SteamTOTP/SteamTOTP.csproj package

# List transitive dependencies
dotnet list SteamTOTP/SteamTOTP.csproj package --include-transitive

# Check for vulnerabilities
dotnet list SteamTOTP/SteamTOTP.csproj package --vulnerable

# Check for updates
dotnet list SteamTOTP/SteamTOTP.csproj package --outdated

# Update SteamGuard.TOTP
dotnet add SteamTOTP/SteamTOTP.csproj package SteamGuard.TOTP --version <new_version>
```

---

## License Compatibility

| Dependency | License | Compatibility with GPLv3 |
|------------|---------|-------------------------|
| SteamGuard.TOTP | Unknown (not in csproj) | **Verify before distribution** |
| NuciExtensions | Unknown (not in csproj) | **Verify before distribution** |
| .NET Runtime | MIT | Compatible |
| .NET SDK | MIT | Compatible (build tool only) |
| GitHub Actions | Various | Compatible (CI only) |
| deployment-scripts | Unknown | **Verify before use** |

**Action Required**: Confirm `SteamGuard.TOTP` and `NuciExtensions` licenses are GPLv3-compatible (MIT, Apache-2.0, BSD, or GPLv3).

---

## Lock File

`SteamTOTP/obj/project.assets.json` — Generated by `dotnet restore`, contains resolved dependency graph with hashes.

Key sections:
- `targets` — Per-framework resolved packages
- `libraries` — Package metadata and content hashes
- `projectFileDependencyGroups` — Top-level references

Do not commit `obj/` (in `.gitignore`). Regenerate with `dotnet restore`.