// File: Salubrity.Application/Interfaces/Storage/IFileStorage.cs
using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Application.Interfaces.Storage
{
    public interface IFileStorage
    {
        /// <summary> Saves bytes to a relative path under the public root and returns the public URL. </summary>
        Task<string> SaveAsync(byte[] content, string relativeFolder, string fileName, string contentType, CancellationToken ct = default);

        Task<bool> DeleteAsync(string relativeFolder, string fileName, CancellationToken ct = default);
    }
}
