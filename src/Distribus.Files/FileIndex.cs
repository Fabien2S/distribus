using System.Text.Json.Serialization;

namespace Distribus.Files;

/// <summary>
///     Represents a file index.
/// </summary>
/// <param name="Files">The file collection.</param>
public record FileIndex(FileEntry[] Files)
{
    /// <summary>
    ///     Gets the total bytes count
    /// </summary>
    [JsonIgnore]
    public long Length { get; } = Files.Sum(c => c.Length);
}