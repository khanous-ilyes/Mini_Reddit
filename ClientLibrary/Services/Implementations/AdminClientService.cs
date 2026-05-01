using BaseLibrary.DTOs;
using BaseLibrary.Helpers;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface IAdminClientService
{
    Task<AdminDashboardDto?> GetDashboardAsync();
    Task<PaginatedResponse<UserRankDto>?> GetReportersAsync(int page = 1, int pageSize = 10);
}

public class AdminClientService : IAdminClientService
{
    private readonly HttpClient _http;
    public AdminClientService(HttpClient http) { _http = http; }

    public async Task<AdminDashboardDto?> GetDashboardAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<AdminDashboardDto>>("api/admin/dashboard");
            return res?.Data;
        }
        catch { return null; }
    }

    public async Task<PaginatedResponse<UserRankDto>?> GetReportersAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<PaginatedResponse<UserRankDto>>>($"api/admin/reporters?page={page}&pageSize={pageSize}");
            return res?.Data;
        }
        catch { return null; }
    }
}
