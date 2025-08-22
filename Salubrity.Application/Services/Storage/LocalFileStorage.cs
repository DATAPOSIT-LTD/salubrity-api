// File: Salubrity.Infrastructure/Storage/LocalFileStorage.cs
using Microsoft.Extensions.Options;
using Salubrity.Application.Interfaces.Storage;

namespace Salubrity.Infrastructure.Storage
{
    public sealed class LocalFileStorage : IFileStorage
    {
        private readonly string _publicRoot;
        private readonly string _publicBaseUrl;

        public sealed class Options
        {
            public string PublicRoot { get; set; } = "wwwroot";
            public string PublicBaseUrl { get; set; } = "";
        }

        public LocalFileStorage(IOptions<Options> opts)
        {
            _publicRoot = opts.Value.PublicRoot;
            _publicBaseUrl = opts.Value.PublicBaseUrl.TrimEnd('/');
        }

        public async Task<string> SaveAsync(byte[] content, string relativeFolder, string fileName, string contentType, CancellationToken ct = default)
        {
            // Ensure folder exists
            var folderFs = Path.Combine(_publicRoot, relativeFolder.Replace('\\', '/'));
            Directory.CreateDirectory(folderFs);

            var fileFs = Path.Combine(folderFs, fileName);
            await File.WriteAllBytesAsync(fileFs, content, ct);

            // Build public URL
            var rel = $"{relativeFolder.Replace('\\', '/')}/{fileName}";
            return $"{_publicBaseUrl}/{rel}";
        }

        public Task<bool> DeleteAsync(string relativeFolder, string fileName, CancellationToken ct = default)
        {
            var fileFs = Path.Combine(_publicRoot, relativeFolder.Replace('\\', '/'), fileName);
            if (File.Exists(fileFs))
            {
                File.Delete(fileFs);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
