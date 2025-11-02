using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Builder;
using PokemonTCGPTracker.Hubs;
using System.Security.Cryptography;

namespace PokemonTCGPTracker.Endpoints;

public static class DeckEndpoint
{
    private const string DeckRelativePath = "img/tcgp/deck.png";

    public static IEndpointRouteBuilder MapDeckEndpoint(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/deck");
        // Allow non-browser scripts (e.g., Python) to POST without antiforgery tokens
        group.DisableAntiforgery();

        group.MapPost("/upload", UploadAsync);
        group.MapGet("/version", GetVersionAsync);
        group.MapGet("/url", GetUrlAsync);
        return app;
    }

    private static async Task<IResult> UploadAsync(HttpContext ctx)
    {
        if (!ctx.Request.HasFormContentType)
        {
            return Results.BadRequest("Content-Type must be multipart/form-data");
        }

        IFormFile? file = ctx.Request.Form.Files.GetFile("file");
        if (file is null)
        {
            // Try any first file if user did not use name "file"
            file = ctx.Request.Form.Files.Count > 0 ? ctx.Request.Form.Files[0] : null;
        }
        if (file is null)
        {
            return Results.BadRequest("Missing file");
        }

        // Basic validation
        string contentType = file.ContentType.ToLowerInvariant();
        if (!contentType.StartsWith("image/"))
        {
            return Results.BadRequest("Only image files are accepted");
        }

        // Limit size to ~10 MB
        const long maxBytes = 10 * 1024 * 1024;
        if (file.Length <= 0 || file.Length > maxBytes)
        {
            return Results.BadRequest("Invalid file size (max 10MB)");
        }

        string webRoot = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        string dir = Path.Combine(webRoot, "img", "tcgp");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "deck.png");

        await using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fs, ctx.RequestAborted);
        }

        // Ensure timestamp definitely changes even on same-millisecond writes
        try
        {
            File.SetLastWriteTimeUtc(path, DateTimeOffset.UtcNow.UtcDateTime);
        }
        catch
        {
            // ignore
        }

        string version = GetFileVersion(path);
        string url = BuildDeckUrl(version, ctx);

        // Notify clients via SignalR that the deck image has been updated
        try
        {
            var hub = ctx.RequestServices.GetRequiredService<IHubContext<PokemonTCGPTracker.Hubs.DeckHub>>();
            await hub.Clients.All.SendAsync("DeckImageUpdated", url, ctx.RequestAborted);
        }
        catch
        {
            // Ignore notification errors to not break the upload
        }

        // Return new image URL (version encoded server-side)
        return Results.Ok(new { url });
    }

    private static async Task<IResult> GetVersionAsync(HttpContext ctx)
    {
        string webRoot = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        string fullPath = Path.Combine(webRoot, DeckRelativePath.Replace('/', Path.DirectorySeparatorChar));
        string version = GetFileVersion(fullPath);
        return Results.Ok(new { version });
    }

    private static async Task<IResult> GetUrlAsync(HttpContext ctx)
    {
        string webRoot = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        string fullPath = Path.Combine(webRoot, DeckRelativePath.Replace('/', Path.DirectorySeparatorChar));
        string version = GetFileVersion(fullPath);
        string url = BuildDeckUrl(version, ctx);
        return Results.Ok(new { url });
    }

    private static string BuildDeckUrl(string version, HttpContext ctx)
    {
        // Use relative URL so it works behind reverse proxies as well
        return $"/{DeckRelativePath}?v={version}";
    }

    private static string GetFileVersion(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
            {
                // Use content hash as version to guarantee change only when bytes change
                using FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] hash = SHA256.HashData(fs);
                // Shorten for URL length but keep enough entropy
                string hex = Convert.ToHexString(hash).ToLowerInvariant();
                return hex[..16];
            }
        }
        catch
        {
            // ignore
        }
        // fallback to time to force change
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    }
}