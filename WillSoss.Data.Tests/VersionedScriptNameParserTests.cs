using FluentAssertions;

namespace WillSoss.Data.Tests
{
    public class VersionedScriptNameParserTests
    {
        [Fact]
        public void ShouldParseScriptNames()
        {
            var files = new string[]
            {
                "9-nine.sql",
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
                ("9", "nine")
            };

            var parser = new VersionedScriptNameParser(files);

            parser.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("no number.sql")]
        [InlineData("nospace.sql")]
        [InlineData("1.1 - not sql.txt")]
        [InlineData("1.1.1.1.1 too many numbers.sql")]
        public void ShouldThrowExceptionOnInvalidFileName(string file)
        {
            var parser = new VersionedScriptNameParser(new string[] { file });

            var ex = Assert.Throws<InvalidScriptNameException>(() => parser.GetEnumerator().MoveNext());

            ex.Path.Should().Be(file);
        }
    }
}