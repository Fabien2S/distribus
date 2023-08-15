using Distribus.Files;
using Distribus.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, FileIndexerSerializerContext.Default);
});

var app = builder.Build();
var fileIndexer = new LocalFileIndexer(app.Environment.ContentRootPath);

app.MapGet(IFileIndexer.IndexPath, async () => await fileIndexer.RetrieveIndexAsync());
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = PathString.Empty,
    FileProvider = new PhysicalFileProvider(fileIndexer.FullPath, ExclusionFilters.Sensitive),
    RedirectToAppendTrailingSlash = false,
    ContentTypeProvider = new DownloadContentTypeProvider(),
    ServeUnknownFileTypes = true,
    HttpsCompression = HttpsCompressionMode.Compress
});

app.Run();