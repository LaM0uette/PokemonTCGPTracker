using Microsoft.AspNetCore.SignalR;
using PokemonTCGPTracker.Hubs;
using System.Security.Cryptography;

namespace PokemonTCGPTracker.Endpoints;

public static class DeckEndpoint
{
    #region Statements

    private const string _deckImgRelativePath = "img/tcgp/deck.png";

    public static IEndpointRouteBuilder MapDeckEndpoint(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/deck");
        
        // Allow non-browser scripts (e.g., Python) to POST without antiforgery tokens
        group.DisableAntiforgery();

        group.MapPost("/upload", UploadAsync);
        group.MapGet("/url", GetUrl);
        
        return app;
    }

    #endregion

    #region Methods

    private static async Task<IResult> UploadAsync(IWebHostEnvironment env, IHubContext<DeckHub> hub, HttpRequest request, CancellationToken ct)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest("Content-Type must be multipart/form-data");
        }

        // Try any first file if user did not use name "file"
        IFormFile? file = request.Form.Files.GetFile("file") ?? (request.Form.Files.Count > 0 ? request.Form.Files[0] : null);
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
        if (file.Length is <= 0 or > maxBytes)
        {
            return Results.BadRequest("Invalid file size (max 10MB)");
        }

        string dir = Path.Combine(env.WebRootPath, "img", "tcgp");
        Directory.CreateDirectory(dir);
        
        string path = Path.Combine(dir, "deck.png");
        
        await using (FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            await file.CopyToAsync(fileStream, ct);
            await fileStream.FlushAsync(ct); // ✅ Force disk write before closing
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
        string url = BuildDeckUrl(version);

        // Notify clients via SignalR that the deck image has been updated
        try
        {
            await hub.Clients.All.SendAsync("DeckImageUpdated", url, ct);
        }
        catch
        {
            // Ignore notification errors to not break the upload
        }

        // Return new image URL (version encoded server-side)
        return Results.Ok(new { url });
    }

    private static IResult GetUrl(IWebHostEnvironment env)
    {
        string fullPath = Path.Combine(env.WebRootPath, _deckImgRelativePath.Replace('/', Path.DirectorySeparatorChar));
        string version = GetFileVersion(fullPath);
        string url = BuildDeckUrl(version);
        
        return Results.Ok(new { url });
    }
    
    
    private static string GetFileVersion(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
            {
                // Use content hash as version to guarantee change only when bytes change
                using FileStream fileStream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] hash = SHA256.HashData(fileStream);
                
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

    private static string BuildDeckUrl(string version)
    {
        return $"/{_deckImgRelativePath}?v={version}";
    }

    #endregion
}