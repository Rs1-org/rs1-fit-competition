using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Competition.Application.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;
        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];
            _blobContainerClient=new BlobContainerClient(connectionString,containerName);
        }
        public async Task DeleteAsync(string fileUrl)
        {
            var uri= new Uri(fileUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> UploadAsync(Stream file, string fileName, string contentType)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(file, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

    }
}
