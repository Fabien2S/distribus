using System.IO.Hashing;

namespace Distribus.Files;

/// <summary>
///     Represents a chunk of a file entry.
/// </summary>
/// <param name="Hash">The hash of the chunk's content.</param>
/// <param name="Size">The size of the chunk's content.</param>
public record FileEntryChunk(ulong Hash, int Size)
{
    /// <summary>
    ///     The maximum size of a chunk
    /// </summary>
    public const int MaxSize = 4 * 1024 * 1024;

    /// <summary>
    ///     Creates a <see cref="FileEntryChunk"/> from a chunk's content.
    /// </summary>
    /// <param name="content">The content of the chunk.</param>
    /// <param name="size">The content size of the chunk.</param>
    /// <returns>The created <see cref="FileEntryChunk"/>.</returns>
    public static FileEntryChunk FromContent(ReadOnlySpan<byte> content, int size)
    {
        return new FileEntryChunk(
            XxHash3.HashToUInt64(content[..size]),
            size
        );
    }

    /// <summary>
    ///     Gets the byte offset and length from the given chunk range.
    /// </summary>
    /// <param name="chunks">The chunk collection.</param>
    /// <param name="range">The chunk range.</param>
    /// <returns>The byte offset and length.</returns>
    public static (long ByteOffset, long ByteLength) GetByteOffsetAndLength(FileEntryChunk[] chunks, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(chunks.Length);

        var byteOffset = (long)offset * MaxSize;
        var byteLength = 0L;

        for (var i = 0; i < length; i++)
        {
            byteLength += chunks[offset + i].Size;
        }

        return (byteOffset, byteLength);
    }
}