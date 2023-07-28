using Distribus.Files;

var indexer = new LocalFileIndexer(Environment.CurrentDirectory);
await indexer.SerializeIndexAsync();