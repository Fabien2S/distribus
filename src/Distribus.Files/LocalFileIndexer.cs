using System.Text.Json;

namespace Distribus.Files;

public class LocalFileIndexer : IFileIndexer, IFileSynchronizer
{
    public const string IndexPath = "index.json";
    
    private const int ChunkSize = 4 * 1024 * 1024;
    private const int ChunkBlockSize = 8192;

    private static readonly byte[] ChunkBuffer = new byte[ChunkSize];
    private static readonly byte[] ChunkBlockBuffer = new byte[ChunkBlockSize];

    public string FullPath => _directory.FullName;

    private readonly DirectoryInfo _directory;

    public LocalFileIndexer(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _directory = new DirectoryInfo(path);
    }

    private IEnumerable<FileInfo> EnumerateFiles()
    {
        return _directory.Exists ? _directory.EnumerateFiles("*", SearchOption.AllDirectories) : Enumerable.Empty<FileInfo>();
    }

    private FileInfo ResolvePath(FileEntry fileEntry)
    {
        var ioPath = Path.Join(_directory.FullName, fileEntry.Path);

        var fullPath = Path.GetFullPath(ioPath);
        if (!fullPath.StartsWith(_directory.FullName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Path is outside directory", nameof(fileEntry));
        }

        return new FileInfo(fullPath);
    }

    public async Task<FileIndex> RetrieveIndexAsync()
    {
        var fileCache = new List<FileEntry>();
        var chunkCache = new List<FileEntryChunk>();

        foreach (var fileInfo in EnumerateFiles())
        {
            var filePath = Path.GetRelativePath(_directory.FullName, fileInfo.FullName);
            if (filePath.Equals(IndexPath, StringComparison.Ordinal))
            {
                continue;
            }

            await using var ioStream = fileInfo.OpenRead();

            while (true)
            {
                var chunk = await ReadChunkAsync(ioStream, chunkCache.Count);
                if (chunk == null)
                {
                    break;
                }

                chunkCache.Add(chunk);
            }

            fileCache.Add(new FileEntry(
                filePath,
                chunkCache.ToArray()
            ));
            chunkCache.Clear();
        }

        return new FileIndex(
            fileCache.ToArray()
        );
    }

    public async Task<Stream> RequestChunkRangeAsync(FileEntry fileEntry, Range chunkRange)
    {
        var ioFile = ResolvePath(fileEntry);
        var (byteOffset, byteLength) = FileEntryChunk.GetByteOffsetAndLength(fileEntry.Chunks, chunkRange);

        // TODO Add support for large files
        if (byteLength > int.MaxValue)
            throw new NotSupportedException($"Large files (> {int.MaxValue} bytes) are not supported in {nameof(LocalFileIndexer)}");

        // TODO Replace MemoryStream with custom stream wrapper which restrict the Stream's position and length
        var ioBuffer = new byte[byteLength];
        await using var ioStream = ioFile.OpenRead();

        ioStream.Seek(byteOffset, SeekOrigin.Begin);
        await ioStream.ReadExactlyAsync(ioBuffer, 0, (int)byteLength);

        return new MemoryStream(ioBuffer, false);
    }

    public async Task SynchronizeFilesAsync(IFileIndexer sourceIndexer, IProgress<FileIndexerStatistics> progress)
    {
        var remoteIndex = await sourceIndexer.RetrieveIndexAsync();

        var stats = new FileIndexerStatistics(
            string.Empty,
            true,
            0,
            remoteIndex.Length
        );

        var localFiles = new Dictionary<string, FileInfo>();

        foreach (var fileInfo in EnumerateFiles())
        {
            var filePath = Path.GetRelativePath(_directory.FullName, fileInfo.FullName);
            localFiles.Add(filePath, fileInfo);
        }

        foreach (var remoteEntry in remoteIndex.Files)
        {
            var ioFile = ResolvePath(remoteEntry);
            var ioName = ioFile.Name;

            stats.Status = ioName;
            progress.Report(stats);

            ioFile.Directory?.Create();
            await using var ioStream = ioFile.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            ioStream.SetLength(remoteEntry.Length);

            var chunkCount = remoteEntry.Chunks.Length;
            for (var chunkIdx = 0; chunkIdx < chunkCount; chunkIdx++)
            {
                var remoteChunk = remoteEntry.Chunks[chunkIdx];
                if (await ReadChunkAsync(ioStream, chunkIdx) == remoteChunk)
                {
                    stats.DownloadedBytes += remoteChunk.Size;
                    progress.Report(stats);
                    continue;
                }

                // chunk mismatch, determines how many continuous chunks are incorrect

                var rangeStart = chunkIdx;
                var rangeEnd = Index.End;
                for (chunkIdx++; chunkIdx < chunkCount; chunkIdx++)
                {
                    if (await ReadChunkAsync(ioStream, chunkIdx) != remoteEntry.Chunks[chunkIdx])
                    {
                        continue;
                    }

                    rangeEnd = chunkIdx;
                    break;
                }

                var dlRange = rangeStart..rangeEnd;
                Console.WriteLine($"Downloading chunk {dlRange.GetOffsetAndLength(chunkCount)} for '{remoteEntry.Path}'");
                await using var sourceStream = await sourceIndexer.RequestChunkRangeAsync(remoteEntry, dlRange);
                ioStream.Seek((long)rangeStart * ChunkSize, SeekOrigin.Begin);

                while (true)
                {
                    var read = await sourceStream.ReadAsync(ChunkBlockBuffer);
                    if (read == 0)
                    {
                        break;
                    }

                    var readMemory = ChunkBlockBuffer.AsMemory(0, read);
                    await ioStream.WriteAsync(readMemory);

                    stats.DownloadedBytes += read;
                    progress.Report(stats);
                }
            }

            localFiles.Remove(remoteEntry.Path);
        }

        foreach (var (_, fileInfo) in localFiles)
        {
            fileInfo.Delete();
        }

        stats.Status = string.Empty;
        stats.IsDownloading = false;
        stats.DownloadedBytes = stats.TotalBytes;
        progress.Report(stats);
    }

    /// <summary>
    ///     Serializes the index to the given <see cref="Stream"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Stream"/>.</param>
    public async Task SerializeIndexAsync(Stream destination)
    {
        var fileIndex = await RetrieveIndexAsync();
        await JsonSerializer.SerializeAsync(destination, fileIndex, FileIndexerSerializerContext.Default.FileIndex);
    }

    private static async Task<FileEntryChunk?> ReadChunkAsync(FileStream stream, int chunkIdx)
    {
        var byteOffset = (long)chunkIdx * ChunkSize;
        if (stream.Length < byteOffset)
        {
            return null;
        }

        stream.Seek(byteOffset, SeekOrigin.Begin);
        var chunkSize = await stream.ReadAtLeastAsync(ChunkBuffer, ChunkSize, false);

        return chunkSize != 0 ? FileEntryChunk.FromContent(ChunkBuffer, chunkSize) : null;
    }
}