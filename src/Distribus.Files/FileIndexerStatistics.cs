namespace Distribus.Files;

public class FileIndexerStatistics
{
    public string Status
    {
        get => _status;
        set => _status = value ?? throw new ArgumentNullException(nameof(value));
    }

    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }

    private string _status;

    public FileIndexerStatistics(string status, long downloadedBytes, long totalBytes)
    {
        ArgumentNullException.ThrowIfNull(status);

        _status = status;
        DownloadedBytes = downloadedBytes;
        TotalBytes = totalBytes;
    }
}