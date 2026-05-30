using CreadorDeRequerimientos.API.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.ConfigureHostUrls();

builder.Services
    .AddPresentation()
    .AddWorkspaceApplication(builder.Configuration, builder.Environment)
    .AddWorkspaceAuthentication(builder.Configuration)
    .AddLocalClientCors();

var app = builder.Build();
var staticFileCacheOptions = new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
        context.Context.Response.Headers.Pragma = "no-cache";
        context.Context.Response.Headers.Expires = "0";
    }
};

app.UseDefaultFiles();
app.UseStaticFiles(staticFileCacheOptions);
app.UseCors(ApplicationServiceExtensions.LocalClientsCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapFallbackToFile("index.html", staticFileCacheOptions);

app.Run();
