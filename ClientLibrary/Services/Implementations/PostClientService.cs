using BaseLibrary.DTOs.Posts;
using BaseLibrary.Helpers;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface IPostClientService
{
    Task<PaginatedResponse<PostSummaryDto>?> GetFeedAsync(Guid? groupId = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<PostDetailDto>?> GetPostDetailAsync(Guid id);
    Task<ApiResponse<PostDetailDto>?> CreatePostAsync(CreatePostDto dto);
    Task<ApiResponse<PostDetailDto>?> UpdatePostAsync(Guid id, CreatePostDto dto);
    Task<ApiResponse<bool>?> DeletePostAsync(Guid id);
    Task<ApiResponse<bool>?> TogglePinPostAsync(Guid id);
    Task<ApiResponse<bool>?> InteractAsync(Guid postId, int selectedIndex);
    Task<ApiResponse<double>?> RatePostAsync(Guid postId, int rating);
    // Report
    Task<ApiResponse<List<ReportReasonDto>>?> GetReportReasonsAsync();
    Task<ApiResponse<ReportReasonDto>?> CreateReportReasonAsync(CreateReportReasonDto dto);
    Task<ApiResponse<bool>?> DeleteReportReasonAsync(Guid id);
    Task<ApiResponse<bool>?> ReportPostAsync(Guid postId, SubmitReportDto dto);
    Task<ApiResponse<PaginatedResponse<PostSummaryDto>>?> SearchPostsAsync(SearchFilterDto filter);
}

public class PostClientService : IPostClientService
{
    private readonly HttpClient _http;
    public PostClientService(HttpClient http) { _http = http; }

    public async Task<PaginatedResponse<PostSummaryDto>?> GetFeedAsync(Guid? groupId = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var url = groupId.HasValue ? $"api/posts?groupId={groupId}&page={page}&pageSize={pageSize}" : $"api/posts?page={page}&pageSize={pageSize}";
            var res = await _http.GetFromJsonAsync<ApiResponse<PaginatedResponse<PostSummaryDto>>>(url);
            return res?.Data;
        }
        catch { return null; }
    }

    public async Task<ApiResponse<PostDetailDto>?> GetPostDetailAsync(Guid id)
    {
        var res = await _http.GetFromJsonAsync<ApiResponse<PostDetailDto>>($"api/posts/{id}");
        return res;
    }

    public async Task<ApiResponse<PostDetailDto>?> CreatePostAsync(CreatePostDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/posts", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<PostDetailDto>>();
    }

    public async Task<ApiResponse<PostDetailDto>?> UpdatePostAsync(Guid id, CreatePostDto dto)
    {
        var res = await _http.PutAsJsonAsync($"api/posts/{id}", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<PostDetailDto>>();
    }

    public async Task<ApiResponse<bool>?> DeletePostAsync(Guid id)
    {
        var res = await _http.DeleteAsync($"api/posts/{id}");
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> TogglePinPostAsync(Guid id)
    {
        var res = await _http.PutAsync($"api/posts/{id}/pin", null);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> InteractAsync(Guid postId, int selectedIndex)
    {
        var res = await _http.PostAsJsonAsync($"api/posts/{postId}/interact", selectedIndex);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<double>?> RatePostAsync(Guid postId, int rating)
    {
        var res = await _http.PostAsync($"api/posts/{postId}/rate?rating={rating}", null);
        return await res.Content.ReadFromJsonAsync<ApiResponse<double>>();
    }

    // Report
    public async Task<ApiResponse<List<ReportReasonDto>>?> GetReportReasonsAsync()
    {
        try { return await _http.GetFromJsonAsync<ApiResponse<List<ReportReasonDto>>>("api/posts/report-reasons"); }
        catch { return null; }
    }

    public async Task<ApiResponse<ReportReasonDto>?> CreateReportReasonAsync(CreateReportReasonDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/posts/report-reasons", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<ReportReasonDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteReportReasonAsync(Guid id)
    {
        var res = await _http.DeleteAsync($"api/posts/report-reasons/{id}");
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> ReportPostAsync(Guid postId, SubmitReportDto dto)
    {
        var res = await _http.PostAsJsonAsync($"api/posts/{postId}/report", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<PaginatedResponse<PostSummaryDto>>?> SearchPostsAsync(SearchFilterDto filter)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/posts/search", filter);
            return await res.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<PostSummaryDto>>>();
        }
        catch { return null; }
    }
}
