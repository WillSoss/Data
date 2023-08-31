using FluentAssertions;

namespace WillSoss.DbDeploy.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("9-nine.sql", "9", "nine")]
        public void ShouldParseScriptNames(string file, string version, string name)
        {
            var files = new string[]
            {
                
                "8_eight.SQL",
                "7 seven.sql",
                "6- _ six.sql",
                "5.6.7.8-five.sql",
                "5.6.7-four.sql",
                "5.3 thr-e_e.sql",
                "4.1 2 two.sql",
                "1 Create Schema.sql"
            };

            var expected = new (string version, string file)[]
            {
                ("1", "Create Schema"),
                ("4.1", "2 two"),
                ("5.3", "thr-e_e"),
                ("5.6.7", "four"),
                ("5.6.7.8", "five"),
                ("6", "six"),
                ("7", "seven"),
                ("8", "eight"),
                ()
            };

            Parser.TryParseFolderName(file, out string? version, )

            parser.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("no number.sql")]
        [InlineData("nospace.sql")]
        [InlineData("1.1 - not sql.txt")]
        [InlineData("1.1.1.1.1 too many numbers.sql")]
        public void ShouldThrowExceptionOnInvalidFileName(string file)
        {
            var ex = Assert.Throws<InvalidScriptNameException>(() => Parser.ParseFolderName(file));

            ex.Path.Should().Be(file);
        }
    }
}