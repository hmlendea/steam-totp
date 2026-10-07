# Capabilities Specification

## Capability: Generate Steam Guard TOTP Code

### Identifier
`CAP-TOTP-GENERATE`

### Description
Given a valid Steam Guard shared secret (Base32-encoded), produce the current 5-character TOTP authentication code using the Steam-compatible character set and 30-second time windows.

### Entry Point
```csharp
SteamTOTP.Program.Main(string[] args)
```
- **Trigger**: Process invocation with exactly one argument
- **Input**: `args[0]` = Base32 shared secret (e.g., `"AABBCCDDEE112233=="`)
- **Preconditions**:
  - Process started with `argc >= 2`
  - `args[0]` is valid Base32 string decodable to byte array
  - System clock synchronized to UTC (NTP recommended)

### Execution Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CAP-TOTP-GENERATE                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  User Shell                                                         │
│      │                                                              │
│      ▼ argv[1] = "AABBCCDDEE112233=="                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ Program.Main(args)                                          │   │
│  │   │                                                         │   │
│  │   ▼ args[0]                                                 │   │
│  │ new SteamGuard().GenerateAuthenticationCode(args[0])        │   │
│  │   │                                                         │   │
│  │   ▼ string code (e.g., "R7V3M")                             │   │
│  │ Console.WriteLine(code)                                     │   │
│  │   │                                                         │   │
│  │   ▼ stdout: "R7V3M\n"                                       │   │
│  └─────────────────────────────────────────────────────────────┘   │
│      │                                                              │
│      ▼ exit code 0                                                  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Detailed Steps

| Step | Location | Operation | Data Transformation |
|------|----------|-----------|---------------------|
| 1 | `Program.cs:7` | Access `args[0]` | `string[]` → `string` (secret) |
| 2 | `SteamGuard.cs` (external) | `new SteamGuard()` | Allocates `SteamGuard` with default `TimeStepProvider` |
| 3 | `SteamGuard.cs` | `GenerateAuthenticationCode(secret)` | `string` (Base32) → `string` (5-char code) |
| 3a | `SteamGuard.cs` | `EncodeToBase32(secret)` | Base32 string → `byte[]` key |
| 3b | `SteamGuard.cs` | `HMACSHA1(key).ComputeHash(timeStepBytes)` | `byte[]` key + `byte[]` time → `byte[]` hash |
| 3c | `SteamGuard.cs` | Dynamic truncation (RFC 4226) | `byte[]` hash → `int` offset → `int` code |
| 3d | `SteamGuard.cs` | Steam charset mapping | `int` code → `string` (5 chars from `23456789BCDFGHJKMNPQRTVWXY`) |
| 4 | `Program.cs:7` | `Console.WriteLine(code)` | `string` → stdout bytes (UTF-8 + newline) |

**Note**: SteamGuard.TOTP internally uses NuciExtensions 5.0.0 for utility functions.

### Exit Conditions

| Condition | Exit Code | Stdout | Stderr |
|-----------|-----------|--------|--------|
| Success | 0 | `XXXXX\n` (5 chars + newline) | Empty |
| No arguments | 139/1 | Empty | `IndexOutOfRangeException` stack trace |
| Invalid Base32 | 1 | Empty | `FormatException`/`InvalidOperationException` stack trace |
| Null/empty secret | 1 | Empty | `NullReferenceException`/`InvalidOperationException` stack trace |

### Output Specification

| Property | Value |
|----------|-------|
| Format | Plain text, UTF-8 |
| Length | Exactly 6 bytes (5 code chars + `\n`) |
| Character set | `23456789BCDFGHJKMNPQRTVWXY` (Steam alphabet, excludes `018AILOSU`) |
| Newline | LF (`\n`, not CRLF) |
| Encoding | UTF-8 (no BOM) |

### Time Sensitivity

| Aspect | Detail |
|--------|--------|
| Time source | `DateTimeOffset.UtcNow` (via `DefaultTimeStepProvider`) |
| Time step | 30 seconds (RFC 6238) |
| Time step calculation | `DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30` |
| Code validity window | Current 30-second window ±1 window (Steam accepts adjacent) |
| Clock skew tolerance | ~30 seconds (Steam server allows ±1 window) |
| Deterministic testing | Requires `ITimeStepProvider` injection (not used by SteamTOTP) |

### Business Rules

| Rule | Enforcement | Violation |
|------|-------------|-----------|
| Secret must be Base32 | SteamGuard.TOTP `EncodeToBase32` | Throws `FormatException` |
| Secret must not be empty | SteamGuard.TOTP validation | Throws `InvalidOperationException` |
| Output always 5 chars | SteamGuard.TOTP contract | N/A (guaranteed) |
| Uses Steam charset | SteamGuard.TOTP implementation | N/A (guaranteed) |
| No rate limiting | None | Unlimited invocations |
| No secret caching | Stateless design | N/A |

### Data Flow

```
Input (CLI)
    │
    ▼
args[0]: string (Base32, e.g., "JBSWY3DPEHPK3PXP")
    │
    ▼
SteamGuard.GenerateAuthenticationCode
    │
    ├── Base32 decode ──▶ byte[] key (10-32 bytes typical)
    │
    ├── Time step ──▶ long (Unix seconds / 30)
    │
    ├── HMAC-SHA1(key, timeStepBytes) ──▶ byte[20] hash
    │
    ├── Dynamic truncation ──▶ int (0 to 2^31-1)
    │
    ├── Modulo 5^5 (Steam charset size) ──▶ 5 indices
    │
    └── Charset mapping ──▶ string (5 chars)
    │
    ▼
Output (stdout): "R7V3M\n"
```

### Known Test Vectors (from SteamGuard.TOTP)

| Secret (Base32) | Time Step | Expected Code |
|-----------------|-----------|---------------|
| `DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3` | 613 | `D57RM` |
| `JBSWY3DPEHPK3PXP` | (current) | Time-dependent |

### Integration Contracts

#### Upstream (Caller → SteamTOTP)
| Parameter | Type | Required | Validation |
|-----------|------|----------|------------|
| `shared_secret` | string (Base32) | Yes | None (crashes if missing/invalid) |

#### Downstream (SteamTOTP → SteamGuard.TOTP)
| Call | Parameters | Returns | Exceptions |
|------|------------|---------|------------|
| `new SteamGuard()` | None | `SteamGuard` | None |
| `GenerateAuthenticationCode(string)` | Base32 secret | 5-char code | `FormatException`, `InvalidOperationException`, `NullReferenceException` |

#### External (SteamTOTP → OS)
| Channel | Content | Encoding |
|---------|---------|----------|
| stdout | `XXXXX\n` | UTF-8 |
| stderr | Exception stack traces (on failure) | UTF-8 |
| exit code | 0 (success), non-zero (failure) | Integer |

### Non-Functional Requirements

| Attribute | Requirement | Current Status |
|-----------|-------------|----------------|
| Latency | <100ms cold start | ✅ (trimmed single-file) |
| Throughput | N/A (single-shot) | N/A |
| Memory | <10MB RSS | ✅ (trimmed) |
| Binary size | ~3-5MB | ✅ (PublishTrimmed) |
| Startup time | <50ms | ✅ (ReadyToRun, trimmed) |
| Cross-platform | Linux, macOS, Windows | ✅ (net10.0) |
| Determinism | Same (secret, time) → same code | ✅ |

### Failure Scenarios

| Scenario | Detection | Recovery |
|----------|-----------|----------|
| Missing argument | `IndexOutOfRangeException` at `args[0]` | User must re-run with secret |
| Invalid Base32 | `FormatException` in `EncodeToBase32` | User must provide valid secret |
| Empty secret | `InvalidOperationException` | User must provide non-empty secret |
| Clock skew >30s | Code rejected by Steam | User must sync system clock (NTP) |
| Trimmed binary missing type | `MissingMethodException` | Rebuild without trimming or add `TrimmerRootAssembly` |

### Related Capabilities

| Capability | Relationship |
|------------|--------------|
| `CAP-TOTP-GENERATE` | Primary (only) capability |
| (Future) `CAP-TOTP-VERIFY` | Could verify a code against secret |
| (Future) `CAP-TOTP-BATCH` | Could generate multiple codes for time range |
| (Future) `CAP-TOTP-QR` | Could generate QR code for enrollment |

### Implementation Location

| Artifact | Path |
|----------|------|
| Entry point | `SteamTOTP/Program.cs:7` |
| TOTP logic | `SteamGuard.TOTP` (NuGet, `SteamGuard.cs`) |
| Time abstraction | `SteamGuard.TOTP` (`ITimeStepProvider`, `DefaultTimeStepProvider`) |
| Base32 decode | `SteamGuard.TOTP` (`EncodeToBase32`) |
| HMAC-SHA1 | `System.Security.Cryptography.HMACSHA1` |
| Charset | `SteamGuard.TOTP` (constant in `SteamGuard.cs`) |

### Verification Checklist

- [ ] Valid secret at known time step produces expected code
- [ ] Invalid Base32 throws appropriate exception
- [ ] Empty secret throws appropriate exception
- [ ] Missing argument throws `IndexOutOfRangeException`
- [ ] Output format exactly 5 chars + LF
- [ ] Output uses only Steam charset characters
- [ ] Consecutive calls within same 30s window return same code
- [ ] Calls across window boundary return different codes
- [ ] Exit code 0 on success
- [ ] Non-zero exit code on all failure modes