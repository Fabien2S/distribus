using Distribus.Files;
using Distribus.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

const string dataPath = "data/";

var fileIndexer = new LocalFileIndexer(dataPath);

app.MapGet(LocalFileIndexer.IndexPath, async context =>
{
    var fileIndex = await fileIndexer.RetrieveIndexAsync();
    await context.Response.WriteAsJsonAsync(fileIndex, FileIndexerSerializerContext.Default.FileIndex);
});

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = PathString.Empty,
    FileProvider = new PhysicalFileProvider(dataPath, ExclusionFilters.Sensitive),
    RedirectToAppendTrailingSlash = false,
    ContentTypeProvider = new DownloadContentTypeProvider(),
    ServeUnknownFileTypes = true,
    HttpsCompression = HttpsCompressionMode.Compress
});

app.Run();