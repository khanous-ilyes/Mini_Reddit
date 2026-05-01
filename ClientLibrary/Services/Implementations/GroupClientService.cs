using BaseLibrary.DTOs.Groups;
using BaseLibrary.Helpers;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations;

public interface IGroupClientService
{
    Task<IEnumerable<GroupSummaryDto>> GetGroupsAsync();
    Task<ApiResponse<GroupSummaryDto>?> CreateGroupAsync(CreateGroupDto dto);
    Task<ApiResponse<GroupSummaryDto>?> UpdateGroupAsync(UpdateGroupDto dto);
    Task<ApiResponse<bool>?> DeleteGroupAsync(Guid id);
    Task<ApiResponse<bool>?> JoinGroupAsync(Guid id);
    Task<ApiResponse<bool>?> LeaveGroupAsync(Guid id);
    Task<IEnumerable<DomainDto>> GetDomainsAsync(Guid groupId);
}

public class GroupClientService : IGroupClientService
{
    private readonly HttpClient _http;
    public GroupClientService(HttpClient http) { _http = http; }

    public async Task<IEnumerable<GroupSummaryDto>> GetGroupsAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<GroupSummaryDto>>>("api/groups");
            return res?.Data ?? Array.Empty<GroupSummaryDto>();
        }
        catch { return Array.Empty<GroupSummaryDto>(); }
    }

    public async Task<ApiResponse<GroupSummaryDto>?> CreateGroupAsync(CreateGroupDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/groups", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<GroupSummaryDto>>();
    }

    public async Task<ApiResponse<GroupSummaryDto>?> UpdateGroupAsync(UpdateGroupDto dto)
    {
        var res = await _http.PutAsJsonAsync("api/groups", dto);
        return await res.Content.ReadFromJsonAsync<ApiResponse<GroupSummaryDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteGroupAsync(Guid id)
    {
        var res = await _http.DeleteAsync($"api/groups/{id}");
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> JoinGroupAsync(Guid id)
    {
        var res = await _http.PostAsync($"api/groups/{id}/join", null);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> LeaveGroupAsync(Guid id)
    {
        var res = await _http.PostAsync($"api/groups/{id}/leave", null);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<IEnumerable<DomainDto>> GetDomainsAsync(Guid groupId)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<DomainDto>>>($"api/groups/{groupId}/domains");
            return res?.Data ?? Array.Empty<DomainDto>();
        }
        catch { return Array.Empty<DomainDto>(); }
    }
}
