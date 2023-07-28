using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Distribus.Files;

namespace Distribus.Client;

public partial class MainWindow : Window, IProgress<FileIndexerStatistics>
{
    private readonly LocalFileIndexer _localFileIndexer;
    private readonly RemoteFileIndexer _remoteFileIndexer;

    public MainWindow()
    {
        _localFileIndexer = new LocalFileIndexer("data/");
        _remoteFileIndexer = new RemoteFileIndexer("http://127.0.0.1:8000");

        InitializeComponent();
    }

    private async void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _localFileIndexer.SynchronizeFilesAsync(_remoteFileIndexer, this);
            StatusLabel.Text = "Done.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;

            DetailsLabel.IsVisible = false;
            StatusProgress.IsIndeterminate = true;

            throw;
        }
    }

    public void Report(FileIndexerStatistics stats)
    {
        StatusLabel.Text = stats.Status;

        var progress = (double)stats.DownloadedBytes / stats.TotalBytes;
        var hasProgress = double.IsFinite(progress);

        PercentLabel.IsVisible = hasProgress;
        PercentLabel.Text = progress.ToString("P1");

        DetailsLabel.IsVisible = hasProgress;
        DetailsLabel.Text = $"{FormatBytes(stats.DownloadedBytes)} / {FormatBytes(stats.TotalBytes)}";

        StatusProgress.IsIndeterminate = !hasProgress;
        StatusProgress.Value = stats.DownloadedBytes;
        StatusProgress.Maximum = stats.TotalBytes;
    }

    private static string FormatBytes(long bytes)
    {
        var units = new[]
        {
            ("PB", 1_000_000_000_000_000L),
            ("TB", 1_000_000_000_000L),
            ("GB", 1_000_000_000L),
            ("MB", 1_000_000L),
            ("KB", 1_000L),
        };

        foreach (var (suffix, unit) in units)
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