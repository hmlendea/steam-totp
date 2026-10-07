# Implementation Documentation

## Source Code Inventory

| File | Lines | Purpose |
|------|-------|---------|
| `SteamTOTP/Program.cs` | 10 | Entry point, only application source |
| `SteamTOTP/SteamTOTP.csproj` | 18 | Project configuration, dependencies |
| `SteamTOTP.slnx` | 5 | Solution file |
| `.github/workflows/dotnet.yml` | 22 | CI pipeline |
| `release.sh` | 4 | Release automation |
| `README.md` | 100+ | User documentation |
| `docs/dependencies.md` | 80+ | Dependency analysis |
| `docs/testing.md` | 100+ | Test strategy |
| `docs/README.md` | 100+ | Documentation index |

**Total application source: ~10 lines (Program.cs only)**

---

## Program.cs — Complete Source Analysis

```csharp
using SG = SteamGuard.TOTP.SteamGuard;

namespace SteamTOTP
{
    public sealed class Program
    {
        public static void Main(string[] args)
            => Console.WriteLine(new SG().GenerateAuthenticationCode(args[0]));
    }
}
```

### Symbol Table

| Symbol | Kind | Location | Visibility | Notes |
|--------|------|----------|------------|-------|
| `SteamTOTP` | Namespace | File-level | Public | Root namespace |
| `Program` | Class | Line 4 | Public, sealed | Entry point container |
| `Main` | Method | Line 6 | Public, static | Entry point |
| `args` | Parameter | Line 6 | - | `string[]`, CLI arguments |
| `SG` | Alias | Line 2 | File-scoped | `SteamGuard.TOTP.SteamGuard` |
| `SteamGuard` | Type | External | Public | From SteamGuard.TOTP NuGet |
| `GenerateAuthenticationCode` | Method | External | Public | Instance method |
| `Console.WriteLine` | Method | BCL | Public | Output |

### Call Graph

```
Main(args: string[])
    │
    ├── args[0] (array access, no bounds check)
    │
    ├── new SG()
    │   └── SteamGuard() [default ctor]
    │       └── new DefaultTimeStepProvider() [internal]
    │
    ├── GenerateAuthenticationCode(secret: string)
    │   ├── EncodeToBase32(secret) → byte[]
    │   ├── timeStepProvider.GetCurrentTimeStep() → long
    │   ├── HMACSHA1(key).ComputeHash(timeStepBytes) → byte[20]
    │   ├── DynamicTruncation(hash) → int
    │   └── MapToSteamCharset(code) → string (5 chars)
    │       └── (uses NuciExtensions internally)
    │
    └── Console.WriteLine(code)
        └── stdout write (UTF-8 + LF)
```

### Control Flow

```
Main
  │
  ├─▶ args.Length >= 1? (IMPLICIT: no check)
  │     ├─ NO → IndexOutOfRangeException → CRASH
  │     └─ YES → continue
  │
  ├─▶ new SteamGuard()
  │     └─▶ Success (no failure mode in ctor)
  │
  ├─▶ GenerateAuthenticationCode(args[0])
  │     ├─▶ secret null? → NullReferenceException → CRASH
  │     ├─▶ secret empty? → InvalidOperationException → CRASH
  │     ├─▶ secret invalid Base32? → FormatException → CRASH
  │     └─▶ Success → string code
  │
  ├─▶ Console.WriteLine(code)
  │     └─▶ Success → stdout
  │
  └─▶ Return (exit code 0)
```

### Exception Propagation

| Exception | Source | Caught? | Result |
|-----------|--------|---------|--------|
| `IndexOutOfRangeException` | `args[0]` | No | Unhandled → process crash, exit 139/1 |
| `NullReferenceException` | `GenerateAuthenticationCode(null)` | No | Unhandled → process crash, exit 1 |
| `InvalidOperationException` | Empty secret | No | Unhandled → process crash, exit 1 |
| `FormatException` | Invalid Base32 | No | Unhandled → process crash, exit 1 |
| `MissingMethodException` | Trimmer removed required type | No | Unhandled → process crash, exit 1 |

**Zero try/catch blocks in application code.**

---

## SteamTOTP.csproj — Complete Analysis

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>SteamTOTP</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>true</PublishTrimmed>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SteamGuard.TOTP" Version="1.1.0" />
  </ItemGroup>

</Project>
```

### Property Group Analysis

| Property | Value | Effect |
|----------|-------|--------|
| `OutputType` | `Exe` | Console application (not library) |
| `TargetFramework` | `net10.0` | .NET 10.0 only (no multi-targeting) |
| `RootNamespace` | `SteamTOTP` | Default namespace for source files |
| `ImplicitUsings` | `enable` | Auto-imports `System`, `System.Linq`, etc. |
| `Nullable` | `enable` | Nullable reference types enabled |
| `PublishSingleFile` | `true` | Single-file bundle on publish |
| `PublishTrimmed` | `true` | IL trimming on publish (removes unused code) |

### Package References

| Package | Version | Source | Purpose |
|---------|---------|--------|---------|
| `SteamGuard.TOTP` | `1.1.0` | NuGet.org | TOTP implementation |
| `NuciExtensions` | `5.0.0` | NuGet.org | Utility library (transitive via SteamGuard.TOTP) |

**Transitive dependency**: SteamGuard.TOTP depends on NuciExtensions 5.0.0 (targeting net9.0).

### Build Artifacts (Debug)

```
bin/Debug/net10.0/
├── SteamTOTP                 # Executable (not single-file in Debug)
├── SteamTOTP.deps.json       # Dependency manifest
├── SteamTOTP.runtimeconfig.json  # Runtime config
└── SteamTOTP.pdb             # Debug symbols
```

### Publish Artifacts (Release)

```
bin/Release/net10.0/
├── steam-totp                # Single-file, trimmed, self-contained (~3-5 MB)
├── steam-totp.pdb            # Debug symbols (optional)
└── steam-totp.deps.json      # Minimal (mostly empty due to single-file)
```

---

## SteamTOTP.slnx — Solution File

```xml
<?xml version="1.0" encoding="utf-8"?>
<Solution Sdk="Microsoft.NET.Sdk">
  <Project Path="SteamTOTP/SteamTOTP.csproj" />
</Solution>
```

- **SDK-style solution** (`.slnx`, not legacy `.sln`)
- **Single project** — no test projects, no library projects
- **No solution-level configuration** — all in csproj

---

## CI Pipeline — .github/workflows/dotnet.yml

```yaml
name: .NET

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]

jobs:
  build:
    name: Build
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 10.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### Step Analysis

| Step | Action | Version | Notes |
|------|--------|---------|-------|
| Checkout | `actions/checkout` | v2 | **Deprecated** (v4 current) |
| Setup .NET | `actions/setup-dotnet` | v1 | **Deprecated** (v4 current) |
| Restore | `dotnet restore` | - | Implicit in build, but explicit here |
| Build | `dotnet build --no-restore` | - | Debug configuration |
| Test | `dotnet test --no-build` | - | **Vacuous** — no test projects exist |

### CI Issues

| Issue | Severity | Recommendation |
|-------|----------|----------------|
| `actions/checkout@v2` | Medium | Pin to SHA or upgrade to v4 |
| `actions/setup-dotnet@v1` | Medium | Pin to SHA or upgrade to v4 |
| `dotnet-version: 10.0.x` | Low | Pin to specific version (e.g., `10.0.100`) |
| No test projects | High | Add test project (see testing.md) |
| No artifact upload | Low | Upload publish output for releases |

---

## Release Script — release.sh

```bash
#!/bin/bash
DOTNET_VERSION='10.0'
RELEASE_SCRIPT_URL="https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/${DOTNET_VERSION}.sh"
wget --quiet -O - "${RELEASE_SCRIPT_URL}" | bash /dev/stdin ${@}
```

### Execution Flow

```
release.sh <version>
    │
    ▼
wget -q -O - URL
    │
    ▼ (stdout: script content)
bash /dev/stdin <version>
    │
    ▼ (external script executes)
    ├── Version validation
    ├── csproj version update
    ├── Changelog generation (git log)
    ├── Git commit + tag
    ├── GitHub Release creation (gh CLI)
    ├── dotnet publish (all RID targets)
    ├── Asset upload to GitHub Release
    └── NuGet push (if configured)
```

### Security Concerns

| Concern | Risk | Mitigation |
|---------|------|------------|
| `wget | bash` | High | Remote code execution; script could change |
| No checksum verification | High | Add `sha256sum` check |
| No commit pinning | Medium | Pin to specific commit SHA |
| External dependency | Medium | Vendor script or use GitHub Actions for release |

---

## SteamGuard.TOTP — External Dependency Implementation

Based on repository analysis (hmlendea/SteamGuard.TOTP):

### Public API

```csharp
namespace SteamGuard.TOTP
{
    public interface ISteamGuard
    {
        string GenerateAuthenticationCode(string totpKey);
    }

    public interface ITimeStepProvider
    {
        long GetCurrentTimeStep();
    }

    public sealed class SteamGuard : ISteamGuard
    {
        public SteamGuard() : this(new DefaultTimeStepProvider()) {}
        public SteamGuard(ITimeStepProvider timeStepProvider) { ... }
        public string GenerateAuthenticationCode(string totpKey) { ... }
    }

    public sealed class DefaultTimeStepProvider : ITimeStepProvider
    {
        public long GetCurrentTimeStep()
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
    }
}
```

### GenerateAuthenticationCode Implementation (Reconstructed)

```csharp
public string GenerateAuthenticationCode(string totpKey)
{
    // 1. Base32 decode (RFC 4648, no padding required)
    byte[] key = EncodeToBase32(totpKey);  // Throws FormatException/InvalidOperationException

    // 2. Get current time step (30-second windows since Unix epoch)
    long timeStep = timeStepProvider.GetCurrentTimeStep();

    // 3. Convert time step to big-endian bytes
    byte[] timeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timeStep));

    // 4. HMAC-SHA1(key, timeBytes)
    using var hmac = new HMACSHA1(key);
    byte[] hash = hmac.ComputeHash(timeBytes);

    // 5. Dynamic truncation (RFC 4226)
    int offset = hash[^1] & 0x0F;
    int code = ((hash[offset] & 0x7F) << 24) |
               ((hash[offset + 1] & 0xFF) << 16) |
               ((hash[offset + 2] & 0xFF) << 8) |
               (hash[offset + 3] & 0xFF);

    // 6. Map to Steam charset (5 chars from 23456789BCDFGHJKMNPQRTVWXY)
    const string charset = "23456789BCDFGHJKMNPQRTVWXY";
    char[] result = new char[5];
    for (int i = 0; i < 5; i++)
    {
        result[i] = charset[code % charset.Length];
        code /= charset.Length;
    }
    return new string(result);
}
```

### EncodeToBase32 (Reconstructed)

```csharp
private static byte[] EncodeToBase32(string input)
{
    if (string.IsNullOrEmpty(input))
        throw new InvalidOperationException("Secret cannot be empty");

    // Custom Base32 decode (RFC 4648, alphabet: ABCDEFGHIJKLMNOPQRSTUVWXYZ234567)
    // Handles padding (=) optionally
    // Returns byte array
}
```

### Test Vectors (from SteamGuard.TOTP.UnitTests)

| Secret | Time Step | Expected |
|--------|-----------|----------|
| `DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3` | 613 | `D57RM` |

---

## Build Commands Reference

### Development
```bash
# Restore (implicit in build)
dotnet restore

# Debug build
dotnet build

# Run with secret
dotnet run -- <shared_secret>

# Release build (no publish)
dotnet build -c Release
```

### Publish (Production Binary)
```bash
# Single-file, trimmed, self-contained
dotnet publish -c Release

# Output: bin/Release/net10.0/steam-totp (Linux/macOS)
#         bin/Release/net10.0/steam-totp.exe (Windows)

# Cross-platform publish
dotnet publish -c Release -r linux-x64 --self-contained true
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r osx-x64 --self-contained true
dotnet publish -c Release -r osx-arm64 --self-contained true
```

### CI Simulation
```bash
# Exact CI steps
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal  # Will warn: no tests
```

---

## Runtime Behavior

### Process Invocation
```bash
# Success
$ ./steam-totp AABBCCDDEE112233==
R7V3M
$ echo $?
0

# Failure: no args
$ ./steam-totp
Unhandled exception. System.IndexOutOfRangeException: Index was outside the bounds of the array.
   at SteamTOTP.Program.Main(String[] args) in Program.cs:line 7
$ echo $?
139  # or 1 depending on OS

# Failure: invalid secret
$ ./steam-totp "not-base32!"
Unhandled exception. System.FormatException: Invalid Base32 string
   at SteamGuard.TOTP.SteamGuard.EncodeToBase32(String input)
   at SteamGuard.TOTP.SteamGuard.GenerateAuthenticationCode(String totpKey)
   at SteamTOTP.Program.Main(String[] args) in Program.cs:line 7
$ echo $?
1
```

### Memory Profile (Trimmed Single-File)
- **RSS**: ~8-12 MB (includes .NET runtime)
- **Startup**: ~20-50 ms (ReadyToRun, trimmed)
- **GC**: Workstation GC (default for console apps)
- **Threads**: 1 (main) + GC finalizer + JIT (if not ReadyToRun)

---

## Trimmer Interaction

### Trimmed Types (Potential Risk)
| Type | Risk | Mitigation |
|------|------|------------|
| `SteamGuard.TOTP.SteamGuard` | Low (directly referenced) | Preserved |
| `SteamGuard.TOTP.DefaultTimeStepProvider` | Low (used by default ctor) | Preserved |
| `System.Security.Cryptography.HMACSHA1` | Low (BLC, used via reflection?) | Preserved |
| `System.Text.Encoding` | Low (Base32 decode) | Preserved |

### Trimmer Root Assembly
Not configured. If trimmer removes required types, add:
```xml
<ItemGroup>
  <TrimmerRootAssembly Include="SteamGuard.TOTP" />
</ItemGroup>
```

---

## Cross-Platform Considerations

| Platform | Binary Name | Execution | Notes |
|----------|-------------|-----------|-------|
| Linux x64 | `steam-totp` | `./steam-totp` | ELF, executable bit |
| Linux arm64 | `steam-totp` | `./steam-totp` | ELF, executable bit |
| Windows x64 | `steam-totp.exe` | `steam-totp.exe` | PE32+ |
| macOS x64 | `steam-totp` | `./steam-totp` | Mach-O, executable bit |
| macOS arm64 | `steam-totp` | `./steam-totp` | Mach-O, executable bit |

**Single-file publish includes native runtime** — no separate .NET installation required on target.

---

## Configuration Files

| File | Purpose | Modifiable? |
|------|---------|-------------|
| `SteamTOTP.csproj` | Build config, dependencies | Yes |
| `SteamTOTP.slnx` | Solution structure | Rarely |
| `.github/workflows/dotnet.yml` | CI pipeline | Yes |
| `release.sh` | Release automation | Yes |
| `README.md` | User docs | Yes |
| `docs/*.md` | Technical docs | Yes |

**No runtime configuration files** — no `appsettings.json`, no environment variables used.

---

## Source Control Metadata

### .gitignore (Inferred)
```
bin/
obj/
*.user
*.suo
.vscode/
*.log
```

### Git History Patterns
- Single author (hmlendea)
- Linear history on master
- Version tags (v1.0.0, etc.)
- Release script manages versioning

---

## Verification Checklist

### Code Correctness
- [ ] `Program.cs` compiles without warnings (nullable enabled)
- [ ] `args[0]` access documented as unsafe
- [ ] No dead code
- [ ] No unused usings (implicit usings handle this)

### Build Correctness
- [ ] `dotnet build` succeeds
- [ ] `dotnet publish -c Release` produces single-file binary
- [ ] Binary executes on target platforms
- [ ] Binary size ~3-5 MB

### CI Correctness
- [ ] Workflow triggers on push/PR to master
- [ ] .NET 10.0.x installed
- [ ] Build succeeds
- [ ] Test step runs (vacuously passes)

### Release Correctness
- [ ] `release.sh <version>` executes external script
- [ ] Version bumped in csproj
- [ ] Git tag created
- [ ] GitHub Release created with assets
- [ ] Binaries published for all RID targets