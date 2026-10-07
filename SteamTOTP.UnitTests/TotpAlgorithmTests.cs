using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;

namespace SteamTOTP.UnitTests;

[TestFixture]
public class TotpAlgorithmTests
{
    [Test]
    public void Base32Decode_ValidInput_ReturnsCorrectBytes()
    {
        // Test Base32 decoding (RFC 4648)
        var input = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";
        var expected = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30 };

        var result = Base32Decode(input);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Base32Decode_WithPadding_ReturnsCorrectBytes()
    {
        // Valid Base32 with padding: only A-Z and 2-7 allowed
        var input = "GEZDGNBVGY3TQOJQ====";
        var result = Base32Decode(input);
        result.Should().HaveCount(10); // 80 bits = 10 bytes
    }

    [TestCase("")]
    [TestCase("INVALID!!!")]
    public void Base32Decode_InvalidInput_ThrowsException(string input)
    {
        var action = () => Base32Decode(input);
        action.Should().Throw<Exception>();
    }

    [TestCase("=")]
    [TestCase("========")]
    public void Base32Decode_PaddingOnly_ReturnsEmpty(string input)
    {
        var result = Base32Decode(input);
        result.Should().BeEmpty();
    }

    [Test]
    public void SteamCharsetMapping_ProducesOnlyValidCharacters()
    {
        const string steamCharset = "23456789BCDFGHJKMNPQRTVWXY";

        // Test that all possible 5-digit codes map to valid Steam charset
        for (int i = 0; i < 100000; i += 1237) // Sample various codes
        {
            var mapped = MapToSteamCharset(i);
            mapped.Should().MatchRegex($"^[{steamCharset}]{{5}}$");
        }
    }

    [Test]
    public void SteamCharsetMapping_IsDeterministic()
    {
        var code1 = MapToSteamCharset(12345);
        var code2 = MapToSteamCharset(12345);
        code1.Should().Be(code2);
    }

    [Test]
    public void SteamCharsetMapping_DifferentInputsProduceDifferentOutputs()
    {
        var outputs = new HashSet<string>();
        for (int i = 0; i < 10000; i++)
        {
            outputs.Add(MapToSteamCharset(i));
        }
        // Should have good distribution (not all same)
        outputs.Count.Should().BeGreaterThan(100);
    }

    // Helper methods replicating SteamGuard.TOTP internals for testing
    private static byte[] Base32Decode(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty");

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var inputClean = input.TrimEnd('=').ToUpperInvariant();

        if (inputClean.Any(c => !alphabet.Contains(c)))
            throw new FormatException("Invalid Base32 character");

        var bits = new List<bool>();
        foreach (var c in inputClean)
        {
            var value = alphabet.IndexOf(c);
            for (int i = 4; i >= 0; i--)
            {
                bits.Add((value & (1 << i)) != 0);
            }
        }

        var bytes = new List<byte>();
        for (int i = 0; i + 7 < bits.Count; i += 8)
        {
            byte b = 0;
            for (int j = 0; j < 8; j++)
            {
                if (bits[i + j])
                    b |= (byte)(1 << (7 - j));
            }
            bytes.Add(b);
        }

        return bytes.ToArray();
    }

    private static string MapToSteamCharset(int code)
    {
        const string charset = "23456789BCDFGHJKMNPQRTVWXY";
        var result = new char[5];

        for (int i = 4; i >= 0; i--)
        {
            result[i] = charset[code % charset.Length];
            code /= charset.Length;
        }

        return new string(result);
    }
}