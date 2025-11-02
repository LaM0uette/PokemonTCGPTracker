using System.Net.Http.Json;

namespace DeckRequester;

public sealed class DeckRequester : IDeckRequester
{
    private readonly HttpClient _http;

    public DeckRequester(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetDeckImageUrlAsync(CancellationToken ct = default)
    {
        UrlDto? dto = await _http.GetFromJsonAsync<UrlDto>("/deck/url", ct);
        return dto?.url ?? "/img/tcgp/deck.png";
    }

    public async Task<string> UploadDeckImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
    {
        using MultipartFormDataContent content = new MultipartFormDataContent();
        StreamContent sc = new StreamContent(imageStream);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(sc, "file", fileName);

        using HttpResponseMessage res = await _http.PostAsync("/deck/upload", content, ct);
        res.EnsureSuccessStatusCode();
        UrlDto? dto = await res.Content.ReadFromJsonAsync<UrlDto>(cancellationToken: ct);
        return dto?.url ?? "/img/tcgp/deck.png";
    }

    private sealed class UrlDto
    {
        public string url { get; set; } = string.Empty;
    }
}
