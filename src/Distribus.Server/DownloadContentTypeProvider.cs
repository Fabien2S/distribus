using Microsoft.AspNetCore.StaticFiles;

namespace Distribus.Server;

public class DownloadContentTypeProvider : IContentTypeProvider
{
    public bool TryGetContentType(string subpath, out string contentType)
    {
        contentType = "application/octet-stream";
        return true;
    }
}