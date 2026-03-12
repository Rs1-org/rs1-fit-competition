using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(Stream file, string fileName, string contentType);
        Task DeleteAsync(string fileUrl);
    }
}
