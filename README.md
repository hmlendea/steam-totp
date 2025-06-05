[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Build Status](https://github.com/hmlendea/steam-totp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steam-totp/actions/workflows/dotnet.yml) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/steam-totp)](https://github.com/hmlendea/steam-totp/releases/latest)

A lightweight .NET console utility to generate [Steam Guard Mobile Authenticator](https://help.steampowered.com/en/faqs/view/06B0-26E6-2CF8-254C) codes (TOTP) using your shared secret.

# Steam TOTP

## 🔧 Build

Requires .NET SDK 9.0 or newer.

```bash
dotnet build -c Release
```

## 🚀 Usage

```bash
dotnet run <TOTP_Key>
```

Or after building:

```bash
./bin/Release/net9.0/steam-totp <TOTP_Key>
```

It will output your current TOTP code for use with Steam Guard.

## 📦 Release

You can download the latest binary from the [releases page](https://github.com/hmlendea/steam-totp/releases/latest).

## ❤️ Support

If you find this project helpful, consider [donating here](https://hmlendea.go.ro/fund.html).

## 📄 License

Licensed under the [GPL-3.0](./LICENSE) License.
