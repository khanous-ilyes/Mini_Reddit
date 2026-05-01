using BaseLibrary.DTOs.Media;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface IMediaClientService
{
    Task<UploadResponseDto?> UploadAsync(MultipartFormDataContent content);
}

public class MediaClientService : IMediaClientService
{
    private readonly HttpClient _http;

    public MediaClientService(HttpClient http)
    {
        _http = http;
    }

    public async Task<UploadResponseDto?> UploadAsync(MultipartFormDataContent content)
    {
        var res = await _http.PostAsync("api/media/upload", content);
        return await res.Content.ReadFromJsonAsync<UploadResponseDto>();
    }
}
