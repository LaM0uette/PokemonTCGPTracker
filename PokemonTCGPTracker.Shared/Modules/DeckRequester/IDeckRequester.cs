using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DeckRequester;

public interface IDeckRequester
{
    Task<string> GetDeckImageUrlAsync(CancellationToken ct = default);

    Task<string> UploadDeckImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default);
}