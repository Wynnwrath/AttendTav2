using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Storage
{
    public class VenueImageStorage : IVenueImageStorage
    {
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;
        private readonly string _storageRoot;

        public VenueImageStorage(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var setupRoot = configuration.GetSetupRootDirectoryPath();
            if (string.IsNullOrWhiteSpace(setupRoot))
            {
                throw new InvalidOperationException("Common:SetupRoot must be configured for venue image storage.");
            }

            _storageRoot = Path.GetFullPath(Path.Combine(setupRoot, "uploads"));
            var webRoot = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
            var webRootPrefix = webRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (_storageRoot.StartsWith(webRootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Venue images must be stored outside wwwroot.");
            }
        }

        public async Task<string> SaveAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new InvalidDataException("A venue photo is required.");
            }

            if (image.Length > MaxImageSizeBytes)
            {
                throw new InvalidDataException("Venue photos must be 5 MB or smaller.");
            }

            var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                throw new InvalidDataException("Venue photos must be JPEG or PNG images.");
            }

            await using var input = image.OpenReadStream();
            var header = new byte[8];
            var bytesRead = await input.ReadAsync(header, 0, header.Length);
            var isPng = bytesRead == 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            var isJpeg = bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

            if ((extension == ".png" && !isPng) || ((extension == ".jpg" || extension == ".jpeg") && !isJpeg))
            {
                throw new InvalidDataException("The uploaded file content does not match a supported image format.");
            }

            input.Position = 0;
            var relativePath = $"venues/{Guid.NewGuid():N}{extension}";
            var fullPath = ResolveSafePath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await input.CopyToAsync(output);
            }

            return relativePath;
        }

        public void Delete(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var fullPath = ResolveSafePath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private string ResolveSafePath(string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException("Stored image paths must be relative.");
            }

            var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            var rootPrefix = _storageRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The image path is outside the configured storage directory.");
            }

            return fullPath;
        }
    }
}
