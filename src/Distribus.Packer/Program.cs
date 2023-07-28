using Distribus.Files;

/*
 * Generates an index.json file in the current directory
 * containing a list of all the files in all the subdirectories
 *
 * The index.json file is used by the client to validate the local files
 */

var indexer = new LocalFileIndexer(Environment.CurrentDirectory);
await using var fileStream = new FileStream(LocalFileIndexer.IndexPath, FileMode.Create, FileAccess.Write);
await indexer.SerializeIndexAsync(fileStream);