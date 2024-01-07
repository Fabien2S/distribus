using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Distribus.Files;

namespace Distribus.Client;

public partial class MainWindow : Window, IProgress<FileIndexerStatistics>
{
    private readonly RemoteFileIndexer _remoteFileIndexer;
    private readonly LocalFileIndexer _localFileIndexer;

    public MainWindow()
    {
        var remoteHost = Environment.GetEnvironmentVariable("APP_HOST") ?? throw new ArgumentException("Missing APP_HOST env");
        var localPath = Environment.GetEnvironmentVariable("APP_PATH") ?? throw new ArgumentException("Missing APP_PATH env");

        _remoteFileIndexer = new RemoteFileIndexer(remoteHost);
        _localFileIndexer = new LocalFileIndexer(localPath);

        InitializeComponent();
    }

    private async void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _localFileIndexer.SynchronizeFilesAsync(_remoteFileIndexer, this);
            StatusLabel.Text = Client.Resources.LaunchingApplication;
        }
        catch (Exception ex)
        {
            Report(new FileIndexerStatistics(
                ex.Message, false, 0L, 0L
            ));
        }
    }

    public void Report(FileIndexerStatistics stats)
    {
        StatusLabel.Text = stats.Status;

        if (stats.IsDownloading)
        {
            ButtonPanel.IsVisible = false;
            DownloadPanel.IsVisible = true;
            DownloadProgress.IsIndeterminate = false;

            PercentLabel.Text = ((double)stats.DownloadedBytes / stats.TotalBytes).ToString("P1");
            DetailsLabel.Text = $"{BinaryFormatter.Format(stats.DownloadedBytes)} / {BinaryFormatter.Format(stats.TotalBytes)}";

            DownloadProgress.Value = stats.DownloadedBytes;
            DownloadProgress.Maximum = stats.TotalBytes;
        }
        else
        {
            ButtonPanel.IsVisible = true;
            DownloadPanel.IsVisible = false;
        }
    }

    private void ActionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}