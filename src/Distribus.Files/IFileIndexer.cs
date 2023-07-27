namespace Distribus.Files;

public interface IFileIndexer
{
    Task<FileIndex> RetrieveIndexAsync();
}