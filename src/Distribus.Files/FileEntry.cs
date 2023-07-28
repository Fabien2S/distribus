using System.Text.Json.Serialization;

namespace Distribus.Files;

/// <summary>
///     Represents a file entry.
/// </summary>
/// <param name="Path">The relative path to the index.</param>
/// <param name="Chunks">The chunk collection.</param>
public record FileEntry(string Path, FileEntryChunk[] Chunks)
{
    /// <summary>
    ///     Gets the file bytes count
    /// </summary>
    [JsonIgnore]
    public long Length { get; } = Chunks.Sum(c => (long)c.Size);
}