using System.IO.Hashing;

namespace Distribus.Files;

public record FileEntryChunk(ulong Hash, int Size)
{
    public static FileEntryChunk FromContent(ReadOnlySpan<byte> content, int size) => new(
        XxHash3.HashToUInt64(content[..size]),
        size
    );
}