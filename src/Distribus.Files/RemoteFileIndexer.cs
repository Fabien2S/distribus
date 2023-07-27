using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Distribus.Files;

public class RemoteFileIndexer : IFileIndexer
{
    private static readonly HttpClient Client = new();

    private readonly Uri _baseUri;

    public RemoteFileIndexer([StringSyntax(StringSyntaxAttribute.Uri)] string baseAddress)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);

        _baseUri = new Uri(baseAddress);
    }

    public async Task<FileIndex> RetrieveIndexAsync()
    {
        var indexUri = new Uri(_baseUri, "index.json");
        var response = await Client.GetAsync(indexUri, HttpCompletionOption.ResponseHeadersRead);
        await using var stream = await response.Content.ReadAsStreamAsync();

        var fileIndex = await JsonSerializer.DeserializeAsync(stream, FileIndexerSerializerContext.Default.FileIndex);
        return fileIndex ?? throw new FormatException("Failed to deserialize file index");
    }

    public async Task DownloadChunkAsync(FileEntry fileEntry, Range chunkRange, Stream destination, Action<int> progressDelegate)
    {
        ArgumentNullException.ThrowIfNull(fileEntry);
        ArgumentNullException.ThrowIfNull(destination);

        var chunks = fileEntry.Chunks;
        var chunkCount = chunks.Length;
        var (chunkOffset, chunkLength) = chunkRange.GetOffsetAndLength(chunkCount);

        var byteOffset = chunkOffset * FileIndexer.ChunkSize;
        var byteLength = 0;
        for (var i = 0; i < chunkLength; i++)
        {
            byteLength += chunks[chunkOffset + i].Size;
        }

        var response = await Client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_baseUri, fileEntry.Path),
            Headers =
            {
                Range = new RangeHeaderValue(byteOffset, byteOffset + byteLength)
            }
        }, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? -1;
        if (contentLength != byteLength)
        {
            throw new FormatException($"Mismatch received chunk length (expected: {byteLength}, actual: {contentLength})");
        }

        destination.Seek(byteOffset, SeekOrigin.Begin);

        var blockBuffer = new byte[FileIndexer.ChunkBlockSize];
        var responseStream = await response.Content.ReadAsStreamAsync();
        while (true)
        {
            var read = await responseStream.ReadAsync(blockBuffer);
            if (read == 0)
            {
                break;
            }

            var readMemory = blockBuffer.AsMemory(0, read);
            await destination.WriteAsync(readMemory);
            progressDelegate.Invoke(read);
        }
    }
}