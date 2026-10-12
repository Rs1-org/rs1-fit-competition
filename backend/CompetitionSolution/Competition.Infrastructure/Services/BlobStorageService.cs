using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Competition.Application.Services;
using Competition.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly Lazy<BlobContainerClient> _blobContainerClient;
        public BlobStorageService(IOptions<BlobStorageSettings> options)
        {
            var settings = options.Value;
            // Created on first use so requests that never touch images work without blob credentials.
            _blobContainerClient = new Lazy<BlobContainerClient>(() =>
            {
                if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                    throw new InvalidOperationException(
                        "AzureBlobStorage:ConnectionString is not configured. Set it via user-secrets or the AzureBlobStorage__ConnectionString environment variable.");
                return new BlobContainerClient(settings.ConnectionString, settings.ContainerName);
            });
        }
        public async Task DeleteAsync(string fileUrl)
        {
            var uri= new Uri(fileUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var blobClient = _blobContainerClient.Value.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> UploadAsync(Stream file, string fileName, string contentType)
        {
            var blobClient = _blobContainerClient.Value.GetBlobClient(fileName);
            await blobClient.UploadAsync(file, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

    }
}
