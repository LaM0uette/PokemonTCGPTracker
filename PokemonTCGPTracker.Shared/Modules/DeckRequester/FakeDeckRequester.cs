namespace DeckRequester;

public class FakeDeckRequester : IDeckRequester
{
    public Task<string> GetDeckImageUrlAsync(CancellationToken ct = default)
    {
        return Task.FromResult("/img/tcgp/deck.png");
    }

    public Task<string> UploadDeckImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
    {
        return Task.FromResult("/img/tcgp/deck.png");
    }
}