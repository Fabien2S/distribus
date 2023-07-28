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

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        var fileIndex = await JsonSerializer.DeserializeAsync(stream, FileIndexerSerializerContext.Default.FileIndex);

        return fileIndex ?? throw new FormatException("Failed to deserialize file index");
    }

    public async Task<Stream> RequestChunkRangeAsync(FileEntry fileEntry, Range chunkRange)
    {
        ArgumentNullException.ThrowIfNull(fileEntry);

        var (byteOffset, byteLength) = FileEntryChunk.GetByteOffsetAndLength(fileEntry.Chunks, chunkRange);
        var response = await Client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_baseUri, fileEntry.Path),
            Headers =
            {
                Range = new RangeHeaderValue(byteOffset, byteOffset + byteLength - 1)
            }
        }, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? -1;
        if (contentLength != byteLength)
        {
            throw new FormatException($"Mismatch received chunk length (expected: {byteLength}, actual: {contentLength})");
        }

        return await response.Content.ReadAsStreamAsync();
    }
}