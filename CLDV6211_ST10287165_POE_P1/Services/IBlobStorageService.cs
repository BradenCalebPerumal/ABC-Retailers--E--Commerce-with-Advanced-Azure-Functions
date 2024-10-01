using System.IO;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadBlobAsync(string containerName, Stream content, string contentType, string blobName); //Updated method signature
        Task<Stream> DownloadBlobAsync(string containerName, string blobName);
        Task DeleteBlobAsync(string containerName, string blobName);
    }
}

