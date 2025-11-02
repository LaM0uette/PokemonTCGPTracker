using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace PokemonTCGPTracker.Client.Pages;

public class DeckViewBase : ComponentBase
{
    [Inject] protected HttpClient Http { get; set; } = null!;

    protected string DeckImageUrl { get; set; } = "/img/tcgp/deck.png";

    protected IBrowserFile? _selectedFile;
    protected bool _isUploading;
    protected string? _message;

    protected override Task OnInitializedAsync()
    {
        // Avoid HTTP calls during server-side prerendering
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && OperatingSystem.IsBrowser())
        {
            await RefreshUrlAsync();
            StateHasChanged();
        }
    }

    protected void OnFileSelected(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
        _message = null;
    }

    protected async Task UploadAsync()
    {
        if (_selectedFile is null) return;
        try
        {
            _isUploading = true;
            _message = null;

            using MultipartFormDataContent content = new MultipartFormDataContent();
            StreamContent sc = new StreamContent(_selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
            sc.Headers.ContentType = new MediaTypeHeaderValue(_selectedFile.ContentType);
            content.Add(sc, "file", _selectedFile.Name);

            HttpResponseMessage res = await Http.PostAsync("/deck/upload", content);
            if (!res.IsSuccessStatusCode)
            {
                _message = $"Upload failed: {(int)res.StatusCode} {res.ReasonPhrase}";
            }
            else
            {
                try
                {
                    var body = await res.Content.ReadFromJsonAsync<UrlDto>();
                    if (body is not null && !string.IsNullOrWhiteSpace(body.url))
                    {
                        DeckImageUrl = AddCacheBuster(body.url);
                    }
                    else
                    {
                        await RefreshUrlAsync();
                    }
                }
                catch
                {
                    await RefreshUrlAsync();
                }
                _message = "Uploaded!";
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _message = ex.Message;
        }
        finally
        {
            _isUploading = false;
        }
    }

    private async Task RefreshUrlAsync()
    {
        try
        {
            var resp = await Http.GetFromJsonAsync<UrlDto>("/deck/url");
            if (resp is not null && !string.IsNullOrWhiteSpace(resp.url))
            {
                DeckImageUrl = AddCacheBuster(resp.url);
            }
            else
            {
                DeckImageUrl = AddCacheBuster("/img/tcgp/deck.png");
            }
        }
        catch
        {
            DeckImageUrl = AddCacheBuster("/img/tcgp/deck.png");
        }
    }

    private static string AddCacheBuster(string url)
    {
        char sep = url.Contains('?') ? '&' : '?';
        string tick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        return $"{url}{sep}r={tick}";
    }

    private class UrlDto
    {
        public string url { get; set; } = string.Empty;
    }
}