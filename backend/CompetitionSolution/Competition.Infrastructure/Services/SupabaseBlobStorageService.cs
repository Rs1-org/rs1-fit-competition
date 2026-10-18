using System.Net.Http.Headers;
using System.Net.Http.Json;
using Competition.Application.Services;
using Competition.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Competition.Infrastructure.Services;

// Optional DEV storage provider; controllers and gallery persistence keep their existing contracts.
public class SupabaseBlobStorageService(HttpClient client, IOptions<SupabaseStorageSettings> options) : IBlobStorageService
{
    private readonly SupabaseStorageSettings settings = options.Value;

    private Uri ObjectEndpoint => new(new Uri(settings.ProjectUrl), $"/storage/v1/object/{settings.BucketName}/");
    private Uri PublicEndpoint => new(new Uri(settings.ProjectUrl), $"/storage/v1/object/public/{settings.BucketName}/");

    private HttpRequestMessage Request(HttpMethod method, Uri uri)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceRoleKey);
        request.Headers.Add("apikey", settings.ServiceRoleKey);
        return request;
    }

    private static string FileName(string name)
    {
        // ImageService generates flat GUID filenames. Reject paths rather than normalizing them.
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." ||
            name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '.' and not '-' and not '_'))
            throw new ArgumentException("Storage object names must be flat filenames.", nameof(name));
        return name;
    }

    public async Task<string> UploadAsync(Stream file, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var name = FileName(fileName);
        using var request = Request(HttpMethod.Post, new Uri(ObjectEndpoint, name));
        request.Content = new StreamContent(file);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return new Uri(PublicEndpoint, name).AbsoluteUri;
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        // Never send privileged credentials to a URL supplied by the caller/database.
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri) || !PublicEndpoint.IsBaseOf(uri) ||
            uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0)
            throw new ArgumentException("Image URL does not belong to the configured storage bucket.", nameof(fileUrl));
        var name = FileName(Uri.UnescapeDataString(PublicEndpoint.MakeRelativeUri(uri).ToString()));
        using var request = Request(HttpMethod.Delete, new Uri(ObjectEndpoint.AbsoluteUri.TrimEnd('/')));
        request.Content = JsonContent.Create(new { prefixes = new[] { name } });
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
