[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/steam-totp)](https://github.com/hmlendea/steam-totp/releases/latest)
[![Build Status](https://github.com/hmlendea/steam-totp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steam-totp/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# Steam TOTP

A lightweight .NET console utility to generate [Steam Guard Mobile Authenticator](https://help.steampowered.com/en/faqs/view/06B0-26E6-2CF8-254C) codes (TOTP) from a shared secret.

Useful for scripting Steam logins or integrating two-factor authentication into automated workflows without a mobile device.

## Requirements

- .NET SDK/runtime with support for `net10.0`

## Usage

Pass your Steam Guard shared secret as the first argument:

```bash
steam-totp <shared_secret>
```

The current 5-digit TOTP code is printed to stdout.

**Example:**

```bash
steam-totp AABBCCDDEE112233==
# 12345
```

The shared secret can be found in your Steam authenticator backup (e.g. from the `shared_secret` field of a `.maFile` exported by tools like [Steam Desktop Authenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)).

## Installation

### From a release binary

Download the latest pre-built single-file binary from the [Releases](https://github.com/hmlendea/steam-totp/releases/latest) page and place it somewhere on your `PATH`.

### From source

```bash
git clone https://github.com/hmlendea/steam-totp.git
cd steam-totp
dotnet publish
```

The self-contained, trimmed binary will be placed under `bin/Release/net10.0/`.

## Development

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run -- <shared_secret>
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from: `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## Contributing

Contributions are welcome. Please:
- Keep changes cross-platform
- Keep the existing public API intact unless a breaking change is intentional
- Keep pull requests focused and consistent with the existing code style
- Update documentation when behaviour changes

## Support

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding).

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
