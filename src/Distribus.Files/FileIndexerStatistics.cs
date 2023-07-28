namespace Distribus.Files;

public class FileIndexerStatistics
{
    public string Status
    {
        get => _status;
        set => _status = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsDownloading { get; set; }
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }

    private string _status;

    public FileIndexerStatistics(string status, bool isDownloading, long downloadedBytes, long totalBytes)
    {
        ArgumentNullException.ThrowIfNull(status);

        _status = status;

        IsDownloading = isDownloading;
        DownloadedBytes = downloadedBytes;
        TotalBytes = totalBytes;
    }
}