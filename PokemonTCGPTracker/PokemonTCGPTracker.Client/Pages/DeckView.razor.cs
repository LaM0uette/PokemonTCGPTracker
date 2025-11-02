using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using DeckRequester;

namespace PokemonTCGPTracker.Client.Pages;

public class DeckViewBase : ComponentBase, IAsyncDisposable
{
    #region Statements
    
    private const string FALLBACK_DECK_IMG = "data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEA"; // 1x1 transparent pixel

    
    protected string DeckImageUrl { get; private set; } = FALLBACK_DECK_IMG;
    
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IDeckRequester _deckRequester { get; set; } = null!;
    
    

    protected IBrowserFile? SelectedFile;
    protected bool IsUploading;
    protected string? Message;

    private HubConnection? _deckHub;

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
            await EnsureDeckHubConnectedAsync();
            StateHasChanged();
        }
    }

    #endregion

    #region Methods

    protected void OnFileSelected(InputFileChangeEventArgs e)
    {
        SelectedFile = e.File;
        Message = null;
    }

    protected async Task UploadAsync()
    {
        if (SelectedFile is null) return;
        try
        {
            IsUploading = true;
            Message = null;

            await using Stream stream = SelectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            string url = await _deckRequester.UploadDeckImageAsync(stream, SelectedFile.Name, SelectedFile.ContentType);
            DeckImageUrl = AddCacheBuster(url);
            Message = "Uploaded!";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsUploading = false;
        }
    }

    private async Task RefreshUrlAsync()
    {
        try
        {
            string url = await _deckRequester.GetDeckImageUrlAsync();
            if (!string.IsNullOrWhiteSpace(url))
            {
                DeckImageUrl = AddCacheBuster(url);
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

    private async Task EnsureDeckHubConnectedAsync()
    {
        if (_deckHub == null)
        {
            Uri url = new Uri(new Uri(_navigationManager.BaseUri), "hubs/deck");
            _deckHub = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            _deckHub.On<string>("DeckImageUpdated", updatedUrl =>
            {
                if (!string.IsNullOrWhiteSpace(updatedUrl))
                {
                    DeckImageUrl = AddCacheBuster(updatedUrl);
                    _ = InvokeAsync(StateHasChanged);
                }
            });
        }

        if (_deckHub.State == HubConnectionState.Disconnected)
        {
            await _deckHub.StartAsync();
        }
    }

    private static string AddCacheBuster(string url)
    {
        char sep = url.Contains('?') ? '&' : '?';
        string tick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        return $"{url}{sep}r={tick}";
    }

    #endregion

    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        if (_deckHub != null)
        {
            await _deckHub.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }

    #endregion
}