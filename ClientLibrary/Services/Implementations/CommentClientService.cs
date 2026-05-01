using BaseLibrary.DTOs.Posts;
using BaseLibrary.Helpers;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface ICommentClientService
{
    Task<List<CommentDto>> GetCommentsAsync(Guid postId);
    Task<ApiResponse<CommentDto>?> AddCommentAsync(CreateCommentDto dto);
    Task<ApiResponse<bool>?> VoteCommentAsync(Guid commentId, short voteType);
}

public class CommentClientService : ICommentClientService
{
    private readonly HttpClient _http;
    public CommentClientService(HttpClient http) { _http = http; }

    public async Task<List<CommentDto>> GetCommentsAsync(Guid postId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<CommentDto>>($"api/comments/{postId}") ?? new();
        }
        catch { return new(); }
    }

    public async Task<ApiResponse<CommentDto>?> AddCommentAsync(CreateCommentDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/comments", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<CommentDto>>();
    }

    public async Task<ApiResponse<bool>?> VoteCommentAsync(Guid commentId, short voteType)
    {
        var res = await _http.PostAsJsonAsync($"api/comments/{commentId}/vote", voteType);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }
}
