using BaseLibrary.DTOs.Auth;
using BaseLibrary.Helpers;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface IAuthClientService
{
    Task<ApiResponse<LoginResponseDto>?> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<LoginResponseDto>?> RegisterTestAsync(RegisterRequestDto request);
}

public class AuthClientService : IAuthClientService
{
    private readonly HttpClient _httpClient;
    
    public AuthClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<LoginResponseDto>?> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
    }

    public async Task<ApiResponse<LoginResponseDto>?> RegisterTestAsync(RegisterRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register-test", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
    }
}
