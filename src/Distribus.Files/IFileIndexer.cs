namespace Distribus.Files;

public interface IFileIndexer
{
    /// <summary>
    ///     Retrieves the index from the indexer.
    /// </summary>
    /// <returns>The file index.</returns>
    Task<FileIndex> RetrieveIndexAsync();

    /// <summary>
    ///     Retrieves a chunk range from the indexer.
    /// </summary>
    /// <param name="fileEntry">The file entry to download.</param>
    /// <param name="chunkRange">The chunk range to download.</param>
    /// <returns>The stream containing the requested chunks</returns>
    Task<Stream> RequestChunkRangeAsync(FileEntry fileEntry, Range chunkRange);
}