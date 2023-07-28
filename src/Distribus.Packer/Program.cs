using Distribus.Files;

var indexer = new LocalFileIndexer(Environment.CurrentDirectory);
await using var fileStream = new FileStream(LocalFileIndexer.IndexPath, FileMode.Create, FileAccess.Write);
await indexer.SerializeIndexAsync(fileStream);