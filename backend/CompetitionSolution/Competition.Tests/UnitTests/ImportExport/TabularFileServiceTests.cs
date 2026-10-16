using System.Text;
using Competition.Application.Interfaces;
using Competition.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.ImportExport;

/// <summary>COMP-50: CSV and Excel writing/reading used by import/export.</summary>
public class TabularFileServiceTests
{
    private readonly TabularFileService _service = new();
    private static readonly string[] Headers = ["Name", "Population", "IsCapital", "Latitude", "Founded"];

    private static IReadOnlyList<object?>[] Rows() =>
    [
        ["Sarajevo, \"capital\"", 275_524, true, 43.8563m, new DateTime(1461, 1, 1)],
        ["Banja Luka\nnorth", 185_042, false, 44.7722m, null],
        ["Čapljina", 26_000, false, -43.12m, new DateTime(2000, 5, 17)],
    ];

    private async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> RoundTrip(TabularFormat format)
    {
        var bytes = _service.Write(Headers, Rows(), format, "Cities");
        return await _service.ReadAsync(new MemoryStream(bytes), format);
    }

    [Theory]
    [InlineData(TabularFormat.Csv)]
    [InlineData(TabularFormat.Xlsx)]
    [InlineData(TabularFormat.Json)]
    public async Task WriteThenRead_ShouldKeepEveryValue(TabularFormat format)
    {
        var records = await RoundTrip(format);

        records.Should().HaveCount(3);
        records[0]["Name"].Should().Be("Sarajevo, \"capital\"");
        records[0]["Population"].Should().Be("275524");
        records[0]["IsCapital"].Should().Be("true");
        records[0]["Latitude"].Should().Be("43.8563");
        records[0]["Founded"].Should().Be("1461-01-01");
        records[1]["Name"].Should().Be("Banja Luka\nnorth");
        records[1]["Founded"].Should().BeEmpty();
        records[2]["Name"].Should().Be("Čapljina");
        records[2]["Latitude"].Should().Be("-43.12");
    }

    [Fact]
    public void Csv_ShouldStartWithUtf8BomForExcel()
    {
        var bytes = _service.Write(Headers, Rows(), TabularFormat.Csv, "Cities");

        bytes.Take(3).Should().Equal(Encoding.UTF8.GetPreamble());
    }

    [Fact]
    public async Task Csv_TextStartingWithFormulaChar_ShouldBePrefixedButNumbersNot()
    {
        var bytes = _service.Write(["Name", "Latitude"], [["=HYPERLINK(\"x\")", -43.12m]], TabularFormat.Csv, "X");

        var records = await _service.ReadAsync(new MemoryStream(bytes), TabularFormat.Csv);

        records[0]["Name"].Should().Be("'=HYPERLINK(\"x\")");
        records[0]["Latitude"].Should().Be("-43.12");
    }

    [Theory]
    [InlineData("{\"Name\": \"Mostar\"}")]
    [InlineData("[1, 2]")]
    [InlineData("[{\"Name\": ")]
    public async Task Json_NotAnArrayOfObjects_ShouldThrowInvalidData(string json)
    {
        var act = () => _service.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes(json)), TabularFormat.Json);

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Fact]
    public async Task Csv_SemicolonDelimitedAndCaseInsensitiveHeaders_ShouldBeRead()
    {
        var csv = "name;POPULATION\r\nMostar;105000\r\n;\r\n";

        var records = await _service.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)), TabularFormat.Csv);

        records.Should().ContainSingle("the fully empty row is skipped");
        records[0]["Name"].Should().Be("Mostar");
        records[0]["Population"].Should().Be("105000");
    }

    [Fact]
    public async Task Csv_UnclosedQuote_ShouldThrowInvalidData()
    {
        var act = () => _service.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes("Name\r\n\"Mostar\r\n")), TabularFormat.Csv);

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Fact]
    public async Task Xlsx_NotAWorkbook_ShouldThrowInvalidData()
    {
        var act = () => _service.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes("Name\r\nMostar")), TabularFormat.Xlsx);

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Fact]
    public async Task EmptyFile_ShouldThrowInvalidData()
    {
        var act = () => _service.ReadAsync(new MemoryStream(), TabularFormat.Csv);

        await act.Should().ThrowAsync<InvalidDataException>();
    }
}
