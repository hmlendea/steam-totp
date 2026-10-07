# Testing Documentation

## Current Test State

**No tests exist** in this repository.

### Evidence
- No `*Tests.cs` files found
- No test projects (`*.Tests.csproj`)
- CI runs `dotnet test` but produces: "No test matches the given test filter"
- No test frameworks referenced (xUnit, NUnit, MSTest, TUnit)

---

## Test Coverage Analysis

### Capabilities Requiring Testing

| Capability | Current Coverage | Required Coverage |
|------------|------------------|-------------------|
| Valid secret → 5-digit code | 0% | Unit + Integration |
| Invalid secret → exception | 0% | Unit |
| Missing argument → exception | 0% | Unit |
| Time-window correctness | 0% | Unit (time abstraction) |
| Known test vectors | 0% | Unit (deterministic) |
| CLI argument parsing | 0% | Integration |
| Output format (stdout, newline) | 0% | Integration |
| Exit codes | 0% | Integration |

---

## Recommended Test Strategy

### Test Project Structure

```
SteamTOTP.Tests/
├── SteamTOTP.Tests.csproj
├── Unit/
│   ├── ProgramTests.cs
│   └── TimeProviderTests.cs (if time abstracted)
├── Integration/
│   └── CliTests.cs
└── TestVectors/
    └── KnownSecrets.cs
```

### Test Framework Selection

| Framework | Pros | Cons | Recommendation |
|-----------|------|------|----------------|
| xUnit | Modern, parallel, .NET native | Slight learning curve | **Recommended** |
| NUnit | Mature, rich assertions | Older patterns | Acceptable |
| MSTest | Built-in, VS integration | Less flexible | Acceptable |
| TUnit | Modern, source generators | Newer, less ecosystem | Future consideration |

**Recommendation**: xUnit with `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`

### Test Dependencies

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
  <PackageReference Include="xunit" Version="2.9.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" Version="9.0.0" />
</ItemGroup>
```

---

## Test Cases

### Unit Tests: Program.Main

```csharp
// SteamTOTP.Tests/Unit/ProgramTests.cs

public class ProgramTests
{
    [Fact]
    public void Main_ValidSecret_WritesCodeToStdout()
    {
        // Arrange
        var secret = "AABBCCDDEE112233==";
        var expectedCode = "12345"; // Time-dependent, needs time control
        
        // Act & Assert
        // Requires: TimeProvider abstraction or known time window
    }

    [Fact]
    public void Main_NoArguments_ThrowsIndexOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => Program.Main(Array.Empty<string>()));
    }

    [Fact]
    public void Main_NullSecret_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Program.Main(new[] { null! }));
    }

    [Fact]
    public void Main_InvalidBase32_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Program.Main(new[] { "INVALID!!!" }));
    }
}
```

### Integration Tests: CLI

```csharp
// SteamTOTP.Tests/Integration/CliTests.cs

public class CliTests
{
    [Fact]
    public async Task Run_ValidSecret_OutputsFiveDigitsAndNewline()
    {
        // Arrange
        var secret = "AABBCCDDEE112233==";
        var exePath = GetPublishedExePath();
        
        // Act
        var result = await ProcessRunner.RunAsync(exePath, secret);
        
        // Assert
        result.ExitCode.Should().Be(0);
        result.Stdout.Should().MatchRegex("^\\d{5}\\n$");
        result.Stderr.Should().BeEmpty();
    }

    [Fact]
    public async Task Run_NoArguments_ReturnsNonZero()
    {
        // Act
        var result = await ProcessRunner.RunAsync(GetPublishedExePath());
        
        // Assert
        result.ExitCode.Should().NotBe(0);
    }
}
```

### Known Test Vectors

SteamGuard.TOTP may provide test vectors. If not, generate using reference implementation:

| Secret (base32) | Unix Time | Time Step | Expected Code |
|-----------------|-----------|-----------|---------------|
| `AABBCCDDEE112233==` | 1699999999 | 56666666 | (compute) |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 0 | 0 | (compute) |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 59 | 1 | (compute) |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 1111111109 | 37037036 | (compute) |

*Reference: RFC 6238 Appendix B test vectors adapted for 5-digit/30s/SHA1*

---

## Time Abstraction for Testability

Current implementation uses `DateTimeOffset.UtcNow` directly inside `SteamGuard.TOTP`. To test deterministically:

### Option 1: Wrap SteamGuard (Adapter Pattern)
```csharp
public interface ITotpGenerator
{
    string GenerateCode(string secret);
}

public class SteamGuardAdapter : ITotpGenerator
{
    public string GenerateCode(string secret) 
        => new SteamGuard.TOTP.SteamGuard().GenerateAuthenticationCode(secret);
}
```

### Option 2: Use Microsoft.Extensions.TimeProvider.Testing (if SteamGuard.TOTP supports it)
Requires SteamGuard.TOTP to accept `TimeProvider` — unlikely without fork.

### Option 3: Integration Tests Only
Test against real time with known secrets at known times (flaky, not recommended).

### Option 4: Fork/Contribute to SteamGuard.TOTP
Add `TimeProvider` support upstream.

**Recommendation**: Option 1 (Adapter) for immediate testability; Option 4 for long-term.

---

## CI Integration

### Updated `.github/workflows/dotnet.yml`

```yaml
jobs:
  test:
    name: Test
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 10.0.x
    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    - name: Upload coverage
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.cobertura.xml
```

### Coverage Targets

| Metric | Target |
|--------|--------|
| Line Coverage | ≥ 90% |
| Branch Coverage | ≥ 80% |
| Critical Paths | 100% |

---

## Test Execution Commands

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ProgramTests"

# Run with detailed output
dotnet test --verbosity normal

# Generate coverage report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

---

## Test Maintenance

### When to Add Tests
- Before any behavioral change
- When adding argument validation
- When changing output format
- When upgrading SteamGuard.TOTP
- When adding new CLI options

### Test Naming Convention
```
<MethodName>_<Scenario>_<ExpectedResult>
Examples:
Main_ValidSecret_WritesCodeToStdout
Main_NoArguments_ThrowsIndexOutOfRangeException
Run_ValidSecret_OutputsFiveDigitsAndNewline
```

### Test Data Management
- Store test secrets in `TestVectors/KnownSecrets.cs` as constants
- Use `Theory`/`InlineData` for parameterized tests
- Never commit real Steam secrets

---

## Gap Analysis

| Area | Current | Target | Effort |
|------|---------|--------|--------|
| Unit Tests | 0% | 100% | Medium (requires time abstraction) |
| Integration Tests | 0% | 100% | Low |
| CI Test Stage | Runs (passes vacuously) | Meaningful | Low |
| Coverage Reporting | None | Codecov/coveralls | Low |
| Mutation Testing | None | Stryker.NET | Medium |

---

## Quick Start: Add First Test

1. Create test project:
```bash
dotnet new xunit -n SteamTOTP.Tests -o SteamTOTP.Tests
dotnet add SteamTOTP.Tests/SteamTOTP.Tests.csproj reference SteamTOTP/SteamTOTP.csproj
dotnet add SteamTOTP.Tests/SteamTOTP.Tests.csproj package FluentAssertions
dotnet add SteamTOTP.Tests/SteamTOTP.Tests.csproj package Microsoft.Extensions.TimeProvider.Testing
dotnet sln add SteamTOTP.Tests/SteamTOTP.Tests.csproj
```

2. Add adapter for testability (in main project or test project)

3. Write first test for `Main_NoArguments_ThrowsIndexOutOfRangeException`

4. Run: `dotnet test`