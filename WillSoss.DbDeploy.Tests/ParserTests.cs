using FluentAssertions;

namespace WillSoss.DbDeploy.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("9.1-nine", "9.1", "nine")]
        [InlineData("8.1_eight", "8.1", "eight")]
        [InlineData("7.1 seven", "7.1", "seven")]
        [InlineData("6.1- _ six", "6.1", "six")]
        [InlineData("5.6.7.8-five", "5.6.7.8", "five")]
        [InlineData("5.6.7-four", "5.6.7", "four")]
        [InlineData("5.3 thr-e_e", "5.3", "thr-e_e")]
        [InlineData("4.1 2 two", "4.1", "2 two")]
        [InlineData("1.0 Create Schema", "1.0", "Create Schema")]
        public void ShouldParseScriptNames(string file, string versionExpected, string nameExpected)
        {
            Parser.TryParseFolderName(file, out string? versionActual, out string? nameActual);

            versionActual!.Should().Be(versionExpected);
            nameActual!.Should().Be(nameExpected);
        }

        [Theory]
        [InlineData("no number")]
        [InlineData("nospace")]
        [InlineData("1.1.1.1.1 too many numbers")]
        public void ShouldThrowExceptionOnInvalidFileName(string file)
        {
            var ex = Assert.Throws<InvalidScriptNameException>(() => Parser.ParseFolderName(file));

            ex.Path.Should().Be(file);
        }
    }
}