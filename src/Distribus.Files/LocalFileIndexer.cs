namespace Distribus.Files;

public class LocalFileIndexer : IFileIndexer
{
    private static readonly byte[] ChunkBuffer = new byte[FileIndexer.ChunkSize];

    private readonly DirectoryInfo _directory;

    public LocalFileIndexer(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _directory = new DirectoryInfo(path);
    }

    public async Task<FileIndex> RetrieveIndexAsync()
    {
        var fileCache = new List<FileEntry>();
        var chunkCache = new List<FileEntryChunk>();

        foreach (var fileInfo in _directory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var filePath = Path.GetRelativePath(_directory.FullName, fileInfo.FullName);
            if (FileIndexer.IsFileIgnored(filePath))
            {
                continue;
            }

            await using var ioStream = fileInfo.OpenRead();

            while (true)
            {
                var chunkSize = await ReadChunkAsync(ioStream, ChunkBuffer, chunkCache.Count);
                if (chunkSize == 0)
                {
                    break;
                }

                chunkCache.Add(FileEntryChunk.FromContent(ChunkBuffer, chunkSize));
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

    public async Task DownloadFilesAsync(RemoteFileIndexer remoteIndexer, DownloadProgressDelegate progressDelegate)
    {
        var remoteIndex = await remoteIndexer.RetrieveIndexAsync();
        
        var downloadedBytes = 0L;
        var totalBytes = remoteIndex.Length;

        foreach (var remoteEntry in remoteIndex.Files)
        {
            var ioPath = Path.Join(_directory.FullName, remoteEntry.Path);
            var ioName = Path.GetFileName(ioPath);

            var ioDirectory = Path.GetDirectoryName(ioPath);
            if (ioDirectory != null)
            {
                Directory.CreateDirectory(ioDirectory);
            }

            await using var ioStream = new FileStream(ioPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            var remoteChunks = remoteEntry.Chunks;
            for (var chunkIdx = 0; chunkIdx < remoteChunks.Length; chunkIdx++)
            {
                var remoteChunk = remoteChunks[chunkIdx];
                if (await IsChunkValidAsync(ioStream, chunkIdx, remoteChunk))
                {
                    downloadedBytes += remoteChunk.Size;
                    continue;
                }

                // chunk mismatch, determines how many continuous chunks are incorrect

                var dlStart = chunkIdx;
                var dlEnd = Index.End;
                for (chunkIdx++; chunkIdx < remoteChunks.Length; chunkIdx++)
                {
                    var isValid = await IsChunkValidAsync(ioStream, chunkIdx, remoteChunk);
                    if (!isValid)
                    {
                        continue;
                    }

                    dlEnd = chunkIdx - 1;
                }

                var dlRange = dlStart..dlEnd;
                Console.WriteLine($"Downloading chunk #{dlRange} for '{remoteEntry.Path}'");
                await remoteIndexer.DownloadChunkAsync(remoteEntry, dlRange, ioStream, read =>
                {
                    downloadedBytes += read;
                    progressDelegate?.Invoke(ioName, downloadedBytes, totalBytes);
                });
            
                progressDelegate?.Invoke(ioName, downloadedBytes, totalBytes);
            }
        }
    }

    private static async Task<bool> IsChunkValidAsync(Stream stream, int chunkIdx, FileEntryChunk expectedChunk)
    {
        var chunkSize = await ReadChunkAsync(stream, ChunkBuffer, chunkIdx);
        if (chunkSize == 0)
        {
            return false;
        }

        return FileEntryChunk.FromContent(ChunkBuffer, chunkSize) == expectedChunk;
    }

    private static async Task<int> ReadChunkAsync(Stream stream, Memory<byte> buffer, int chunkIdx)
    {
        var byteOffset = chunkIdx * FileIndexer.ChunkSize;
        if (stream.Length < byteOffset)
        {
            return 0;
        }

        stream.Seek(byteOffset, SeekOrigin.Begin);
        return await stream.ReadAtLeastAsync(buffer, buffer.Length, false);
    }
}