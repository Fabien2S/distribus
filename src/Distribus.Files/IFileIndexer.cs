namespace Distribus.Files;

public interface IFileIndexer
{
    Task<FileIndex> RetrieveIndexAsync();
    Task<Stream> DownloadChunkAsync(FileEntry fileEntry, Range chunkRange);
}