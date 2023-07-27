using System.Text.Json.Serialization;

namespace Distribus.Files;

public record FileIndex(FileEntry[] Files)
{
    /// <summary>
    ///     Gets the total bytes count
    /// </summary>
    [JsonIgnore]
    public long Length { get; } = Files.Sum(c => c.Length);
}