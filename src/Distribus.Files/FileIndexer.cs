using System.Text.Json;

namespace Distribus.Files;

public static class FileIndexer
{
    public const int ChunkSize = 4 * 1024 * 1024;
    public const int ChunkBlockSize = 8192;

    private const string IndexPath = "index.json";

    public static bool IsFileIgnored(string path)
    {
        return path.Equals(IndexPath, StringComparison.Ordinal);
    }

    public static async Task SerializeIndexAsync(string path, IFileIndexer indexer)
    {
        var directoryName = Path.GetDirectoryName(path);
        var indexPath = Path.Join(directoryName, IndexPath);
        var fileIndex = await indexer.RetrieveIndexAsync();

        await using var fileStream = new FileStream(indexPath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(fileStream, fileIndex, FileIndexerSerializerContext.Default.FileIndex);
    }
}