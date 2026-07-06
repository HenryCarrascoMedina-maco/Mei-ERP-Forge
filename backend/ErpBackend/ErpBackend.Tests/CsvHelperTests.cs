using ErpBackend.CrossCutting.Helpers;
using Xunit;

namespace ErpBackend.Tests;

public class CsvHelperTests
{
    [Fact]
    public void Escape_QuotesValuesContainingDelimitersAndQuotes()
    {
        Assert.Equal("\"a,b\"", CsvHelper.Escape("a,b"));
        Assert.Equal("\"she said \"\"hi\"\"\"", CsvHelper.Escape("she said \"hi\""));
        Assert.Equal("plain", CsvHelper.Escape("plain"));
    }

    [Fact]
    public void BuildLine_And_Parse_Roundtrip()
    {
        var line = CsvHelper.BuildLine(new object?[] { "Acme, Inc.", "a@b.com", 42 });
        var rows = CsvHelper.Parse(line);

        Assert.Single(rows);
        Assert.Equal(new[] { "Acme, Inc.", "a@b.com", "42" }, rows[0]);
    }

    [Fact]
    public void Parse_HandlesMultipleRows()
    {
        var rows = CsvHelper.Parse("Name,Age\nAlice,30\nBob,25");
        Assert.Equal(3, rows.Count);
        Assert.Equal(new[] { "Bob", "25" }, rows[2]);
    }
}
