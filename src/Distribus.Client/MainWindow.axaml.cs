using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Distribus.Files;

namespace Distribus.Client;

public partial class MainWindow : Window
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
            await _localFileIndexer.DownloadFilesAsync(_remoteFileIndexer, UpdateStatus);

            // var totalBytes = fileIndex.Length;
            // var processBytes = 0L;
            //
            // foreach (var fileInfo in fileIndex.Files)
            // {
            //     var fileName = Path.GetFileName(fileInfo.Path);
            //     UpdateStatus(fileName, processBytes);
            //
            //     var chunkStartIdx = -1;
            //
            //     var chunks = fileInfo.Chunks;
            //     var ioChunks = await _localFileIndexer.RetrieveChunksAsync(fileInfo);
            //     for (var chunkIdx = 0; chunkIdx < chunks.Length; chunkIdx++)
            //     {
            //         var chunk = chunks[chunkIdx];
            //         var chunkIsValid = chunkIdx < ioChunks.Length && chunk == ioChunks[chunkIdx];
            //
            //         if (!chunkIsValid)
            //         {
            //             if (chunkStartIdx == -1)
            //             {
            //                 chunkStartIdx = chunkIdx;
            //             }
            //         }
            //         else if (chunkStartIdx != -1)
            //         {
            //             var chunkEndIdx = chunkIdx - 1;
            //
            //             await _remoteFileIndexer.DownloadChunkAsync(fileInfo, chunkStartIdx..chunkEndIdx,);
            //             // TODO Write content to local file
            //             chunkStartIdx = -1;
            //         }
            //
            //         processBytes += chunk.Size;
            //         UpdateStatus($"{fileName}#{chunkIdx:0000}", processBytes);
            //     }
            //
            //     // TODO Remove excessive data in io chunk
            //     // ioChunk.Size - chunk.Size > 0
            // }
            //
            //
            // _state = State.DownloadingFiles;
            // UpdateStatus("Done", processBytes);
        }
        catch (Exception ex)
        {
            UpdateStatus(ex.Message, 0, 0);
            throw;
        }
    }

    private void UpdateStatus(string status, long downloadedBytes, long totalBytes)
    {
        StatusLabel.Text = status;

        var ratio = (double)downloadedBytes / totalBytes;
        DetailsLabel.Text = ratio.ToString("P0");
        DetailsLabel.IsVisible = double.IsFinite(ratio);
        StatusProgress.Value = downloadedBytes;
        StatusProgress.Maximum = totalBytes;
        StatusProgress.IsIndeterminate = double.IsNaN(ratio);
    }
}