using BaseLibrary.DTOs;
using BaseLibrary.Helpers;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface IProfileClientService
{
    Task<ApiResponse<UserProfileDto>?> GetProfileAsync(string userId);
    Task<ApiResponse<bool>?> UpdateProfileAsync(UpdateProfileDto dto);
}

public class ProfileClientService : IProfileClientService
{
    private readonly HttpClient _http;
    public ProfileClientService(HttpClient http) { _http = http; }

    public async Task<ApiResponse<UserProfileDto>?> GetProfileAsync(string userId)
    {
        try
        {
            return await _http.GetFromJsonAsync<ApiResponse<UserProfileDto>>($"api/profile/{userId}");
        }
        catch { return null; }
    }

    public async Task<ApiResponse<bool>?> UpdateProfileAsync(UpdateProfileDto dto)
    {
        try
        {
            var res = await _http.PutAsJsonAsync("api/profile", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch { return null; }
    }
}
