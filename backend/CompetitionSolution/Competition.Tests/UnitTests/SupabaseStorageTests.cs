using System.Net;
using Competition.Infrastructure.Services;
using Competition.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Competition.Tests.UnitTests;

public class SupabaseStorageTests
{
    private static SupabaseBlobStorageService Service(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) =>
        new(new HttpClient(new Handler(send)), Options.Create(new SupabaseStorageSettings
        {
            ProjectUrl = "https://dev-test.supabase.co", ServiceRoleKey = "test-only-key", BucketName = "images-dev",
        }));

    [Fact]
    public async Task Upload_StreamsImageWithServerCredentials_AndReturnsPublicUrl()
    {
        var service = Service(async request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsoluteUri.Should().Be("https://dev-test.supabase.co/storage/v1/object/images-dev/test.png");
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.GetValues("apikey").Should().ContainSingle().Which.Should().Be("test-only-key");
            request.Content!.Headers.ContentType!.MediaType.Should().Be("image/png");
            (await request.Content.ReadAsByteArrayAsync()).Should().Equal(1, 2, 3);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var url = await service.UploadAsync(new MemoryStream([1, 2, 3]), "test.png", "image/png");
        url.Should().Be("https://dev-test.supabase.co/storage/v1/object/public/images-dev/test.png");
    }

    [Fact]
    public async Task Delete_UsesTrustedEndpointAndObjectPrefix()
    {
        var service = Service(async request =>
        {
            request.Method.Should().Be(HttpMethod.Delete);
            request.RequestUri!.AbsoluteUri.Should().Be("https://dev-test.supabase.co/storage/v1/object/images-dev");
            (await request.Content!.ReadAsStringAsync()).Should().Be("{\"prefixes\":[\"test.png\"]}");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        await service.DeleteAsync("https://dev-test.supabase.co/storage/v1/object/public/images-dev/test.png");
    }

    [Theory]
    [InlineData("https://attacker.invalid/storage/v1/object/public/images-dev/test.png")]
    [InlineData("https://dev-test.supabase.co/storage/v1/object/public/other/test.png")]
    [InlineData("https://dev-test.supabase.co/storage/v1/object/public/images-dev/folder/test.png")]
    public async Task Delete_RejectsForeignUrlsBeforeSendingCredentials(string url)
    {
        var service = Service(_ => throw new Exception("No HTTP request expected."));
        await FluentActions.Awaiting(() => service.DeleteAsync(url)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Upload_PropagatesProviderFailureInsteadOfReturningAStoredUrl()
    {
        var service = Service(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)));
        await FluentActions.Awaiting(() => service.UploadAsync(new MemoryStream([1]), "test.png", "image/png"))
            .Should().ThrowAsync<HttpRequestException>();
    }

    private class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
}
