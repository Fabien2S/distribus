using System.IO.Hashing;

namespace Distribus.Files;

public record FileEntryChunk(ulong Hash, int Size)
{
    public const int MaxSize = 4 * 1024 * 1024;

    public static FileEntryChunk FromContent(ReadOnlySpan<byte> content, int size) => new(
        XxHash3.HashToUInt64(content[..size]),
        size
    );

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