using BlazorServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "products.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbInitializer.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

static void DisableClientCache(StaticFileResponseContext context)
{
    context.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
    context.Context.Response.Headers.Pragma = "no-cache";
    context.Context.Response.Headers.Expires = "0";
}

var clientSourceWwwroot = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    "..",
    "BlazorClient",
    "wwwroot"));
var clientProjectPath = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    "..",
    "BlazorClient"));
var clientBuildConfiguration = Directory.Exists(Path.Combine(clientProjectPath, "bin", "Debug", "net10.0", "wwwroot"))
    ? "Debug"
    : "Release";
var clientBuildWwwroot = Path.GetFullPath(Path.Combine(
    clientProjectPath,
    "bin",
    clientBuildConfiguration,
    "net10.0",
    "wwwroot"));
var clientBuildRoot = Path.GetFullPath(Path.Combine(
    clientProjectPath,
    "bin",
    clientBuildConfiguration,
    "net10.0"));
var clientPublishedWwwroot = Path.GetFullPath(Path.Combine(
    clientProjectPath,
    "..",
    "PublishedClient",
    "wwwroot"));
var legacyClientPublishedWwwroot = Path.GetFullPath(Path.Combine(
    clientProjectPath,
    "published",
    "wwwroot"));
var clientRuntimeWwwroot = Directory.Exists(clientPublishedWwwroot)
    ? clientPublishedWwwroot
    : Directory.Exists(legacyClientPublishedWwwroot)
        ? legacyClientPublishedWwwroot
    : clientSourceWwwroot;
var clientFrameworkRoot = Directory.Exists(clientRuntimeWwwroot)
    ? Path.Combine(clientRuntimeWwwroot, "_framework")
    : Path.Combine(clientBuildWwwroot, "_framework");
var clientScopedCss = Path.GetFullPath(Path.Combine(
    clientProjectPath,
    "obj",
    clientBuildConfiguration,
    "net10.0",
    "scopedcss",
    "bundle"));

app.UseStaticFiles();
if (Directory.Exists(clientRuntimeWwwroot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientRuntimeWwwroot),
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream",
        OnPrepareResponse = DisableClientCache
    });
}

if (Directory.Exists(clientBuildWwwroot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientBuildWwwroot),
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream",
        OnPrepareResponse = DisableClientCache
    });
}

if (Directory.Exists(clientBuildRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientBuildRoot),
        RequestPath = "/_framework",
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream",
        OnPrepareResponse = DisableClientCache
    });
}

if (Directory.Exists(clientScopedCss))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientScopedCss),
        OnPrepareResponse = DisableClientCache
    });
}

app.UseRouting();

app.MapControllers();
app.MapGet("/_framework/blazor.webassembly.js", () =>
{
    var entryScript = Directory.Exists(clientFrameworkRoot)
        ? Directory.GetFiles(clientFrameworkRoot, "blazor.webassembly.*.js").FirstOrDefault()
        : null;

    return entryScript is null
        ? Results.NotFound()
        : Results.File(entryScript, "text/javascript");
});
app.MapFallback(() =>
{
    var indexPath = Path.Combine(clientRuntimeWwwroot, "index.html");

    return File.Exists(indexPath)
        ? Results.File(indexPath, "text/html")
        : Results.NotFound();
});

app.Run();
