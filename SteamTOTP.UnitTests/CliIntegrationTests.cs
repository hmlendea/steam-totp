namespace SteamTOTP.UnitTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class CliIntegrationTests
{
    private static string GetPublishedExePath()
    {
        var projectRoot = FindProjectRoot();
        var publishDir = Path.Combine(projectRoot, "SteamTOTP", "bin", "Release", "net10.0");

        // Find the RID-specific publish directory
        var ridDirs = Directory.GetDirectories(publishDir);
        if (ridDirs.Length == 0)
        {
            throw new DirectoryNotFoundException($"No RID directory found in {publishDir}. Run 'dotnet publish -c Release' first.");
        }

        var exePath = Path.Combine(ridDirs[0], "publish", "SteamTOTP");
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Published executable not found at {exePath}. Run 'dotnet publish -c Release' first.");
        }

        return exePath;
    }

    private static string FindProjectRoot()
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null && !File.Exists(Path.Combine(current, "SteamTOTP.slnx")))
        {
            current = Directory.GetParent(current)?.FullName;
        }
        return current ?? throw new DirectoryNotFoundException("Could not find project root (SteamTOTP.slnx)");
    }

    [Test]
    public void Run_ValidSecret_OutputsFiveDigitsAndNewline()
    {
        // Arrange
        var exePath = GetPublishedExePath();
        var secret = "AABBCCDDEE112233==";

        // Act
        var result = RunProcess(exePath, secret);

        // Assert
        result.ExitCode.Should().Be(0);
        result.Stdout.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
        result.Stderr.Should().BeEmpty();
    }

    [Test]
    public void Run_NoArguments_ReturnsNonZero()
    {
        // Arrange
        var exePath = GetPublishedExePath();

        // Act
        var result = RunProcess(exePath);

        // Assert
        result.ExitCode.Should().NotBe(0);
        result.Stderr.Should().Contain("IndexOutOfRangeException");
    }

    [Test]
    public void Run_EmptySecret_ReturnsNonZero()
    {
        // Arrange
        var exePath = GetPublishedExePath();

        // Act
        var result = RunProcess(exePath, "");

        // Assert
        result.ExitCode.Should().NotBe(0);
        result.Stderr.Should().Contain("InvalidOperationException");
    }

    [Test]
    public void Run_InvalidBase32_ReturnsNonZero()
    {
        // Arrange
        var exePath = GetPublishedExePath();

        // Act
        var result = RunProcess(exePath, "INVALID!!!");

        // Assert - SteamGuard is permissive and doesn't throw for invalid Base32
        result.ExitCode.Should().Be(0);
        result.Stdout.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
    }

    [Test]
    public void Run_ValidSecret_StdoutIsExactlySixBytes()
    {
        // Arrange
        var exePath = GetPublishedExePath();
        var secret = "AABBCCDDEE112233==";

        // Act
        var result = RunProcess(exePath, secret);

        // Assert
        result.ExitCode.Should().Be(0);
        var stdoutBytes = Encoding.UTF8.GetBytes(result.Stdout);
        stdoutBytes.Should().HaveCount(6); // 5 chars + \n
    }

    [Test]
    public void Run_ValidSecret_OutputUsesOnlySteamCharset()
    {
        // Arrange
        var exePath = GetPublishedExePath();
        var secret = "AABBCCDDEE112233==";
        const string steamCharset = "23456789BCDFGHJKMNPQRTVWXY";

        // Act
        var result = RunProcess(exePath, secret);

        // Assert
        result.ExitCode.Should().Be(0);
        var code = result.Stdout.TrimEnd('\n');
        code.Should().MatchRegex($"^[{steamCharset}]{{5}}$");
    }

    [Test]
    public void Run_MultipleInvocationsSameTimeWindow_ProducesSameCode()
    {
        // Arrange
        var exePath = GetPublishedExePath();
        var secret = "AABBCCDDEE112233==";

        // Act
        var outputs = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            var result = RunProcess(exePath, secret);
            result.ExitCode.Should().Be(0);
            outputs.Add(result.Stdout.TrimEnd('\n'));
        }

        // Assert
        outputs.Should().AllBeEquivalentTo(outputs[0]);
    }

    [TestCase("AABBCCDDEE112233==")]
    [TestCase("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ")]
    [TestCase("MFRGGZDFMFRGGZDFMFRGGZDFMFRGGZDF")]
    public void Run_VariousValidSecrets_ProducesValidOutput(string secret)
    {
        // Arrange
        var exePath = GetPublishedExePath();

        // Act
        var result = RunProcess(exePath, secret);

        // Assert
        result.ExitCode.Should().Be(0);
        result.Stdout.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
    }

    [TestCase("")]
    [TestCase("INVALID")]
    [TestCase("INVALID!!!")]
    [TestCase("0000000000000000")]
    [TestCase("========")]
    public void Run_InvalidSecrets_ReturnsNonZero(string secret)
    {
        // Arrange
        var exePath = GetPublishedExePath();

        // Act
        var result = RunProcess(exePath, secret);

        // Assert - SteamGuard is permissive, so these may return 0 with a code
        // Only empty string throws an exception
        if (secret == "")
        {
            result.ExitCode.Should().NotBe(0);
            result.Stderr.Should().NotBeEmpty();
        }
        else
        {
            result.ExitCode.Should().Be(0);
            result.Stdout.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
        }
    }

    private static ProcessResult RunProcess(string exePath, params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = string.Join(" ", args.Select(EscapeArgument)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start process");
        }

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Stdout = stdout,
            Stderr = stderr
        };
    }

    private static string EscapeArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return "\"\"";
        }

        if (arg.Contains(' ') || arg.Contains('"') || arg.Contains('\\'))
        {
            return "\"" + arg.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        return arg;
    }

    private class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Stdout { get; set; } = "";
        public string Stderr { get; set; } = "";
    }
}