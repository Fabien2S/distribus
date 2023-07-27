using System.Text.Json.Serialization;

namespace Distribus.Files;

public record FileEntry(string Path, FileEntryChunk[] Chunks)
{
    /// <summary>
    ///     Gets the file bytes count
    /// </summary>
    [JsonIgnore]
    public long Length { get; } = Chunks.Sum(c => (long)c.Size);
}