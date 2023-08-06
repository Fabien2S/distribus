namespace Distribus.Files;

public interface IFileSynchronizer
{
    /// <summary>
    ///     Synchronizes files locally with the given <see cref="IFileIndexer"/>
    /// </summary>
    /// <param name="sourceIndexer">The source indexer.</param>
    /// <param name="progress">The progress provider.</param>
    Task SynchronizeFilesAsync(IFileIndexer sourceIndexer, IProgress<FileIndexerStatistics> progress);
}