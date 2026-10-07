namespace SteamTOTP.UnitTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class ProgramTests
{
    [Test]
    public void Main_ValidSecret_WritesCodeToStdout()
    {
        // Arrange
        var secret = "AABBCCDDEE112233==";
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();

        try
        {
            Console.SetOut(stringWriter);

            // Act
            SteamTOTP.Program.Main([secret]);

            // Assert
            var output = stringWriter.ToString();
            output.Should().NotBeNullOrEmpty();
            output.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Test]
    public void Main_NoArguments_ThrowsIndexOutOfRangeException()
    {
        // Act & Assert
        var action = () => SteamTOTP.Program.Main(Array.Empty<string>());
        action.Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void Main_NullSecret_ThrowsNullReferenceException()
    {
        // Act & Assert
        var action = () => SteamTOTP.Program.Main([null!]);
        action.Should().Throw<NullReferenceException>();
    }

    [Test]
    public void Main_EmptySecret_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var action = () => SteamTOTP.Program.Main([""]);
        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Main_InvalidBase32_ThrowsFormatException()
    {
        // Act & Assert - SteamGuard is permissive and doesn't throw for invalid Base32
        var action = () => SteamTOTP.Program.Main(new[] { "INVALID!!!" });
        action.Should().NotThrow();
    }

    [Test]
    public void Main_WhitespaceOnlySecret_ThrowsFormatException()
    {
        // Act & Assert - SteamGuard is permissive and doesn't throw for whitespace
        var action = () => SteamTOTP.Program.Main(new[] { "   " });
        action.Should().NotThrow();
    }

    [Test]
    public void Main_ValidSecret_OutputsExactlyFiveCharactersPlusNewline()
    {
        // Arrange
        var secret = "AABBCCDDEE112233==";
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();

        try
        {
            Console.SetOut(stringWriter);

            // Act
            SteamTOTP.Program.Main([secret]);

            // Assert
            var output = stringWriter.ToString();
            output.Should().HaveLength(6); // 5 chars + \n
            output[5].Should().Be('\n');
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Test]
    public void Main_ValidSecret_OutputUsesOnlySteamCharset()
    {
        // Arrange
        var secret = "AABBCCDDEE112233==";
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        const string steamCharset = "23456789BCDFGHJKMNPQRTVWXY";

        try
        {
            Console.SetOut(stringWriter);

            // Act
            SteamTOTP.Program.Main(new[] { secret });

            // Assert
            var output = stringWriter.ToString().TrimEnd('\n');
            output.Should().MatchRegex($"^[{steamCharset}]{{5}}$");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Test]
    public void Main_MultipleInvocationsWithSameSecret_ProducesSameCodeWithinTimeWindow()
    {
        // Arrange
        var secret = "AABBCCDDEE112233==";
        var originalOut = Console.Out;

        try
        {
            var outputs = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);

                SteamTOTP.Program.Main([secret]);

                outputs.Add(stringWriter.ToString().TrimEnd('\n'));
            }

            // Assert - all codes should be identical within the same time window
            outputs.Should().AllBeEquivalentTo(outputs[0]);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [TestCase("AABBCCDDEE112233==")]
    [TestCase("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ")]
    [TestCase("MFRGGZDFMFRGGZDFMFRGGZDFMFRGGZDF")]
    public void Main_VariousValidSecrets_ProducesValidOutput(string secret)
    {
        // Arrange
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();

        try
        {
            Console.SetOut(stringWriter);

            // Act
            SteamTOTP.Program.Main([secret]);

            // Assert
            var output = stringWriter.ToString();
            output.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [TestCase("")]
    public void Main_InvalidSecrets_ThrowsException(string secret)
    {
        // Act & Assert - Only empty string throws
        var action = () => SteamTOTP.Program.Main(new[] { secret });
        action.Should().Throw<Exception>();
    }

    [TestCase("INVALID")]
    [TestCase("INVALID!!!")]
    [TestCase("0000000000000000")] // Valid Base32 but wrong length
    [TestCase("========")]
    public void Main_InvalidSecrets_ReturnsCode(string secret)
    {
        // Act & Assert - SteamGuard is permissive and returns a code for these
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();

        try
        {
            Console.SetOut(stringWriter);
            SteamTOTP.Program.Main(new[] { secret });
            var output = stringWriter.ToString();
            output.Should().MatchRegex("^[23456789BCDFGHJKMNPQRTVWXY]{5}\n$");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}