namespace DougaAPI.Clients;

/// <summary>
/// ServerClient in this context means the <i>server</i> where a media is uploaded to
/// </summary>
public sealed class ServerClient
{
    private readonly HttpClient _client;

    private const string UploadApiLink = "https://litterbox.catbox.moe/resources/internals/api.php";

    public ServerClient(HttpClient client)
        => _client = client;

    public async Task<Uri> UploadToServer(string path, CancellationToken token)
    {
        await using var fileStream = File.OpenRead(path);
        using MultipartFormDataContent uploadFileRequest = new()
        {
            { new StringContent("fileupload"), "reqtype" },
            { new StringContent("24h"), "time" },
            { new StreamContent(fileStream), "fileToUpload", path }
        };
        using var response = await _client.PostAsync(UploadApiLink, uploadFileRequest, token);
        var responseString = await response.Content.ReadAsStringAsync(token);

        if (!Uri.TryCreate(responseString, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("Invalid response from server.");

        return uri;
    }
}