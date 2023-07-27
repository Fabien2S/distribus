using Distribus.Files;

var localFileIndexer = new LocalFileIndexer(Environment.CurrentDirectory);
await FileIndexer.SerializeIndexAsync(Environment.CurrentDirectory, localFileIndexer);