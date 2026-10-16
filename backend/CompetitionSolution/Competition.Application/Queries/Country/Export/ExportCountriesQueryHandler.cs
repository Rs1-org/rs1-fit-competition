using Competition.Application.Common.ImportExport;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Country.Export;

public class ExportCountriesQueryHandler : IRequestHandler<ExportCountriesQuery, ExportFileDto>
{
    /// <summary>Same columns the import reads, so an exported file can be edited and imported back.</summary>
    public static readonly string[] Columns = ["PublicId", "Name", "Slug", "Created"];

    private readonly ICountry _country;
    private readonly ITabularFileService _files;

    public ExportCountriesQueryHandler(ICountry country, ITabularFileService files)
    {
        _country = country;
        _files = files;
    }

    public async Task<ExportFileDto> Handle(ExportCountriesQuery request, CancellationToken cancellationToken)
    {
        var filter = new CountryListFilter(request.Name, request.Slug, request.HasPicture, request.CreatedFrom, request.CreatedTo,
            new SortSpec(request.SortBy, request.SortDir));
        var countries = await _country.GetFilteredListAsync(filter, ImportExportFormat.MaxExportRows, cancellationToken);

        var rows = countries.Select(c => (IReadOnlyList<object?>)
            [c.PublicId, c.Name, c.Slug, c.CreatedAt.ToDateTimeUtc().Date]);

        var format = ImportExportFormat.Parse(request.Format);
        return ImportExportFormat.ToFile(_files.Write(Columns, rows, format, "Countries"), format, "countries");
    }
}
