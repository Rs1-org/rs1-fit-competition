using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken = default);
        Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
