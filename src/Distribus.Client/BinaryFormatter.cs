namespace Distribus.Client;

internal static class BinaryFormatter
{
    private static readonly (string Suffix, long Unit)[] Units =
    {
        ("PB", 1_000_000_000_000_000L),
        ("TB", 1_000_000_000_000L),
        ("GB", 1_000_000_000L),
        ("MB", 1_000_000L),
        ("KB", 1_000L),
    };

    public static string Format(long bytes)
    {
        foreach (var (suffix, unit) in Units)
        {
            if (bytes < unit)
            {
                continue;
            }

            return $"{(double)bytes / unit:F1} {suffix}";
        }

        return bytes.ToString("F1");
    }
}